using Application.ResponseCode;
using ApplicationLayer.DTO.BookingTicketManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.BookingTicketManagement
{
    public interface ISeatSignalService
    {
        Task<HoldSeatResult> HoldSeatAsync(HoldSeatSignalRequest req);
        Task<(bool Success, string? Message)> ReleaseSeatAsync(ReleaseSeatSignalRequest req);
    }
    public class SeatSignalService : BaseService, ISeatSignalService
    {
        private readonly IGenericRepository<SeatLog> _seatLogRepo;
        private readonly IGenericRepository<Seat> _seatRepo;
        private readonly IGenericRepository<ShowTime> _showTimeRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<BookingDetail> _bookingDetailRepo;
        private readonly IGenericRepository<Users> _userRepo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpCtx;

        public SeatSignalService(IGenericRepository<SeatLog> seatLogRepo, IGenericRepository<Seat> seatRepo, IGenericRepository<ShowTime> showTimeRepo, IGenericRepository<Booking> bookingRepo, IGenericRepository<BookingDetail> bookingDetailRepo, IGenericRepository<Users> userRepo, IMapper mapper, IHttpContextAccessor httpCtx) : base(mapper, httpCtx)
        {
            _seatLogRepo = seatLogRepo;
            _seatRepo = seatRepo;
            _showTimeRepo = showTimeRepo;
            _bookingRepo = bookingRepo;
            _bookingDetailRepo = bookingDetailRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _httpCtx = httpCtx;
        }

        private string GenerateBookingCode()
        {
            // Logic tạo mã booking duy nhất (ví dụ: kết hợp thời gian và random string)
            return "BK" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

        }

        public async Task<HoldSeatResult> HoldSeatAsync(HoldSeatSignalRequest req)
        {
            var payload = ExtractPayload();
            if (payload == null)
                return new HoldSeatResult { Success = false, Message = "Invalid token" };

            var userId = payload.UserId;

            var showTime = await _showTimeRepo.FindByIdAsync(req.ShowTimeId);
            if (showTime == null)
                return new HoldSeatResult { Success = false, Message = "ShowTime Not Found" };

            var seats = await _seatRepo.FindAllAsync(s => req.SeatIds.Contains(s.Id));
            if (seats.Count() != req.SeatIds.Count)
                return new HoldSeatResult { Success = false, Message = "Some selected seats do not exist" };

            if (seats.Any(s => s.RoomId != showTime.RoomId))
                return new HoldSeatResult { Success = false, Message = "Selected seats do not belong to the specified showtime room" };

            var heldLogs = await _seatLogRepo.FindAllAsync(s =>
                s.ShowTimeId == req.ShowTimeId && s.ExpiredAt > DateTime.UtcNow);

            var conflict = heldLogs.Any(log => req.SeatIds.Contains(log.SeatId));
            if (conflict)
                return new HoldSeatResult { Success = false, Message = "Some seats are already held by another user" };

            var user = await _userRepo.FindByIdAsync(userId);
            if (user == null)
                return new HoldSeatResult { Success = false, Message = "User not found" };

            var totalPrice = seats.Sum(s => s.PriceSeat);

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ShowTimeId = req.ShowTimeId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.Phone,
                IdentityCardNumber = user.IdentityCard,
                BookingCode = GenerateBookingCode(),
                BookingDate = DateTime.UtcNow,
                TotalPrice = totalPrice,
                TotalSeats = req.SeatIds.Count,
                Status = BookingStatus.Pending
            };

            await _bookingRepo.CreateAsync(booking);

            var now = DateTime.UtcNow;
            var expiredAt = now.AddMinutes(5);

            var logs = req.SeatIds.Select(seatId => new SeatLog
            {
                Id = Guid.NewGuid(),
                SeatId = seatId,
                ShowTimeId = req.ShowTimeId,
                UserId = userId,
                BookingId = booking.Id,
                ExpiredAt = expiredAt,
                Status = SeatStatus.Pending
            });

            await _seatLogRepo.CreateRangeAsync(logs);

            // Change status seat
            foreach (var seat in seats)
            {
                seat.Status = SeatStatus.Pending;
            }
            await _seatRepo.UpdateRangeAsync(seats);

            var bookingDetails = req.SeatIds.Select(seatId => new BookingDetail
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                SeatId = seatId,
            });
            await _bookingDetailRepo.CreateRangeAsync(bookingDetails);

            return new HoldSeatResult
            {
                Success = true,
                BookingId = booking.Id,
                BookingCode = booking.BookingCode,
                ExpiredAt = expiredAt
            };
        }

        public async Task<(bool Success, string? Message)> ReleaseSeatAsync(ReleaseSeatSignalRequest req)
        {
            var payload = ExtractPayload();
            if (payload == null)
                return (false, "Invalid token");

            var logs = await _seatLogRepo.FindAllAsync(s =>
                req.SeatIds.Contains(s.SeatId) &&
                s.ShowTimeId == req.ShowTimeId
            );

            if (logs.Any())
            {
                // Xoá seat log
                await _seatLogRepo.DeleteRangeAsync(logs);

                // Cập nhật lại trạng thái seat
                var seatIds = logs.Select(l => l.SeatId).ToList();
                var seatsToUpdate = await _seatRepo.FindAllAsync(s => seatIds.Contains(s.Id));

                foreach (var seat in seatsToUpdate)
                {
                    seat.Status = SeatStatus.Available;
                }

                await _seatRepo.UpdateRangeAsync(seatsToUpdate);
            }

            return (true, null);
        }
    }
}
