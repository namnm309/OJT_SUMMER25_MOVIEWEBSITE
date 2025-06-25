using Application.ResponseCode;
using ApplicationLayer.DTO.BookingTicketManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.BookingTicketManagement
{
    public interface IBookingTicketService
    {
        Task<IActionResult> GetAvailableMovies(); //Lấy danh sách phim
        Task<IActionResult> GetShowDatesByMovie(Guid movieId); //Lấy danh sách ngày chiếu cho phim
        Task<IActionResult> GetShowTimesByMovieAndDate(Guid movieId, DateTime selectedDate); //Lấy các giờ chiếu trong ngày
        Task<IActionResult> GetBookingDetailsAsync(Guid bookingId, Guid userId);

    }
    public class BookingTicketService : IBookingTicketService
    {
        private readonly IGenericRepository<Movie> _movieRepo;
        private readonly IGenericRepository<ShowTime> _showtimeRepo;
        private readonly IMapper _mapper;
        private readonly IBookingRepository _bookingRepository;

        public BookingTicketService(IGenericRepository<Movie> movieRepo, IGenericRepository<ShowTime> showtimeRepo, IMapper mapper, IBookingRepository bookingRepository)
        {
            _movieRepo = movieRepo;
            _showtimeRepo = showtimeRepo;
            _mapper = mapper;
            _bookingRepository = bookingRepository;
        }

        public async Task<IActionResult> GetAvailableMovies()
        {
            var movie = await _movieRepo.WhereAsync(m => m.Status == MovieStatus.NowShowing);
            if (movie == null)
                return ErrorResp.NotFound("Movie Not Found");

            var result = _mapper.Map<List<MovieDropdownDto>>(movie);

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetShowDatesByMovie(Guid movieId)
        {
            var showtime = await _showtimeRepo.WhereAsync(s => s.MovieId == movieId);
            if (showtime == null)
                return ErrorResp.NotFound("Not Found");

            var date = showtime
                .Where(s => s.ShowDate.HasValue)
                .Select(s => s.ShowDate.Value.Date)
                .Distinct() // loại bỏ giá trị trùng lặp trong danh sách
                .OrderBy(d => d) //sắp xếp danh sách tăng dần
                .ToList();

            return SuccessResp.Ok(date);
        }

        public async Task<IActionResult> GetShowTimesByMovieAndDate(Guid movieId, DateTime selectedDate)
        {
            var showtime = await _showtimeRepo.WhereAsync(s =>
                s.MovieId == movieId &&
                s.ShowDate.HasValue &&
                s.ShowDate.Value.Date == selectedDate.Date);

            var timeList = showtime
                .Select(s => new
                {
                    s.Id,
                    Time = s.ShowDate.Value.ToString("HH:mm")
                }).ToList();

            return SuccessResp.Ok(timeList);
        }

        public async Task<IActionResult> GetBookingDetailsAsync(Guid bookingId, Guid userId)
        {
            // Get booking with details
            var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId, userId);
            if (booking == null)
            {
                return ErrorResp.NotFound("Booking not found");
            }

            // Check if booking has all required data
            if (booking.ShowTime?.Movie == null || booking.ShowTime?.Room == null ||
                !booking.BookingDetails.Any() || booking.User == null)
            {
                return ErrorResp.BadRequest("Incomplete booking data. Please return and complete seat selection.");
            }

            // Map booking to DTO
            var bookingDetailsDto = new BookingDetailsDto
            {
                BookingId = booking.Id,
                BookingCode = booking.BookingCode,
                MovieName = booking.ShowTime.Movie.Title,
                RoomName = booking.ShowTime.Room.RoomName,
                ShowDate = booking.ShowTime.ShowDate ?? DateTime.UtcNow,
                SeatCodes = booking.BookingDetails.Select(bd => bd.Seat.SeatCode).ToList(),
                UnitPrice = booking.BookingDetails.FirstOrDefault()?.Price ?? 0,
                TotalPrice = booking.TotalPrice,
                TotalSeats = booking.TotalSeats,
                FullName = booking.User.FullName,
                Email = booking.User.Email,
                IdentityCard = booking.User.IdentityCard,
                Phone = booking.User.Phone
            };

            return SuccessResp.Ok(bookingDetailsDto);
        }
    }
}
