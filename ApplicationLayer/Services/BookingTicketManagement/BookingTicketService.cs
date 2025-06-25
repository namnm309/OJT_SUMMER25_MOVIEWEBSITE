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

    }
    public class BookingTicketService : IBookingTicketService
    {
        private readonly IGenericRepository<Movie> _movieRepo;
        private readonly IGenericRepository<ShowTime> _showtimeRepo;
        private readonly IMapper _mapper;

        public BookingTicketService(IGenericRepository<Movie> movieRepo, IGenericRepository<ShowTime> showtimeRepo, IMapper mapper)
        {
            _movieRepo = movieRepo;
            _showtimeRepo = showtimeRepo;
            _mapper = mapper;
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
    }
}
