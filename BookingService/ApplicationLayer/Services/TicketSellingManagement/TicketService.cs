using Application.ResponseCode;
using ApplicationLayer.DTO.TicketSellingManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.TicketSellingManagement
{
    public interface ITicketService
    {
        Task<IActionResult> CreateTicketFromBookingAsync(Guid bookingId);
        Task<IActionResult> GetTicketsByBookingIdAsync(Guid bookingId);
        Task<IActionResult> GetTicketsByUserIdAsync();
        Task<IActionResult> GetTicketByIdAsync(Guid ticketId);
    }

    public class TicketService : BaseService, ITicketService
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<BookingDetail> _bookingDetailRepo;
        private readonly IGenericRepository<Seat> _seatRepo;
        private readonly IGenericRepository<ShowTime> _showTimeRepo;
        private readonly IGenericRepository<Movie> _movieRepo;
        private readonly IGenericRepository<CinemaRoom> _cinemaRoomRepo;
        private readonly IGenericRepository<Ticket> _ticketRepo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpCtx;

        public TicketService(IGenericRepository<Booking> bookingRepo, IGenericRepository<BookingDetail> bookingDetailRepo, IGenericRepository<Seat> seatRepo, IGenericRepository<ShowTime> showTimeRepo, IGenericRepository<Movie> movieRepo, IGenericRepository<CinemaRoom> cinemaRoomRepo, IGenericRepository<Ticket> ticketRepo, IMapper mapper, IHttpContextAccessor httpCtx) : base(mapper, httpCtx)
        {
            _bookingRepo = bookingRepo;
            _bookingDetailRepo = bookingDetailRepo;
            _seatRepo = seatRepo;
            _showTimeRepo = showTimeRepo;
            _movieRepo = movieRepo;
            _cinemaRoomRepo = cinemaRoomRepo;
            _ticketRepo = ticketRepo;
            _mapper = mapper;
            _httpCtx = httpCtx;
        }

        public async Task<IActionResult> CreateTicketFromBookingAsync(Guid bookingId)
        {
            var booking = await _bookingRepo.FindByIdAsync(bookingId);
            if (booking == null || booking.Status != BookingStatus.Confirmed)
                return ErrorResp.BadRequest("Invalid booking");

            var showTime = await _showTimeRepo.FindAsync(s => s.Id == booking.ShowTimeId,"Movie", "Room");
            if (showTime == null)
                return ErrorResp.NotFound("ShowTime not found");

            var bookingDetails = await _bookingDetailRepo.FindAllAsync(d => d.BookingId == bookingId);
            if (!bookingDetails.Any())
                return ErrorResp.NotFound("Booking Detail not found");

            var movie = showTime.Movie;
            var room = showTime.Room;

            // Tạo danh sách seat code để gom lại
            var seatCodes = new List<string>();
            decimal totalPrice = 0;

            foreach (var detail in bookingDetails)
            {
                var seat = await _seatRepo.FindByIdAsync(detail.SeatId);
                if (seat != null)
                {
                    seatCodes.Add(seat.SeatCode);
                    totalPrice += detail.Price;
                }
            }

            if (!seatCodes.Any())
                return ErrorResp.BadRequest("No valid seats found");

            var tickets = new List<Ticket>();

            foreach (var detail in bookingDetails)
            {
                var seat = await _seatRepo.FindByIdAsync(detail.SeatId);
                if (seat == null) continue;

                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    BookingId = bookingId,
                    ShowTimeId = showTime.Id,
                    MovieName = movie.Title,
                    Screen = room.RoomName,
                    ShowDate = showTime.ShowDate ?? DateTime.MinValue,
                    ShowTime = showTime.StartTime,
                    SeatCode = string.Join(", ", seatCodes),
                    Price = detail.Price,
                    CreatedAt = DateTime.UtcNow
                };

                tickets.Add(ticket);
            }

            if (!tickets.Any())
                return ErrorResp.BadRequest("Không có vé nào được tạo do dữ liệu ghế không hợp lệ");

            await _ticketRepo.CreateRangeAsync(tickets);

            return SuccessResp.Ok("Tickets created successfully");
        }

        public async Task<IActionResult> GetTicketsByBookingIdAsync(Guid bookingId)
        {
            var tickets = await _ticketRepo.FindAllAsync(t => t.BookingId == bookingId);
            if (tickets == null || !tickets.Any())
                return ErrorResp.NotFound("Tickets not found");

            var ticketDtos = _mapper.Map<List<TicketDto>>(tickets);

            return SuccessResp.Ok(ticketDtos);
        }

        public async Task<IActionResult> GetTicketsByUserIdAsync()
        {
            var payload = ExtractPayload();
            if (payload == null)
                return ErrorResp.Unauthorized("Invalid token");

            var userId = payload.UserId;

            var tickets = await _ticketRepo.FindAllAsync(t => t.Booking.UserId == userId);
            if (tickets == null || !tickets.Any())
                return ErrorResp.NotFound("No tickets found for user");

            var ticketDtos = _mapper.Map<List<TicketDto>>(tickets);

            return SuccessResp.Ok(ticketDtos);
        }

        public async Task<IActionResult> GetTicketByIdAsync(Guid ticketId)
        {
            var payload = ExtractPayload();
            if (payload == null)
                return ErrorResp.Unauthorized("Invalid token");

            var ticket = await _ticketRepo.FindByIdAsync(ticketId);
            if (ticket == null)
                return ErrorResp.NotFound("Ticket not found");

            var ticketDto = _mapper.Map<TicketDto>(ticket);

            return SuccessResp.Ok(ticketDto);
        }
    }
}
