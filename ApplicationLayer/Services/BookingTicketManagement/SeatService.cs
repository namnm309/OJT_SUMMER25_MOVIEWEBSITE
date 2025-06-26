using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.ResponseCode;
using ApplicationLayer.DTO.CinemaRoomManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using DomainLayer.Exceptions;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLayer.Services.BookingTicketManagement
{

    public interface ISeatService
    {
        Task<IActionResult> GetAvailableSeats(Guid showTimeId);
        Task<IActionResult> ValidateSelectedSeats(Guid showTimeId, List<Guid> seatIds);

        Task<IActionResult> GetShowTimeDetails(Guid showTimeId);
    }
    public class SeatService : ISeatService
    {

        private readonly ISeatRepository _seatRepository;
        private readonly IGenericRepository<ShowTime> _showTimeRepository; // Add this
        private readonly IMapper _mapper;

        public SeatService(
            ISeatRepository seatRepository,
            IGenericRepository<ShowTime> showTimeRepository, // Add this to the constructor
            IMapper mapper)
        {
            _seatRepository = seatRepository;
            _showTimeRepository = showTimeRepository; // Assign it
            _mapper = mapper;
        }

        public async Task<IActionResult> GetAvailableSeats(Guid showTimeId)
        {
            try
            {
                var (roomName, seats, bookedSeatIds) = await _seatRepository.GetSeatInfoAsync(showTimeId);

                var seatDtos = seats.Select(s => new SeatDto
                {
                    Id = s.Id,
                    SeatCode = s.SeatCode,
                    SeatType = s.SeatType.ToString(),
                    RowIndex = s.RowIndex,
                    ColumnIndex = s.ColumnIndex,
                    IsAvailable = s.IsActive,
                    Price = s.PriceSeat
                }).ToList();

                return SuccessResp.Ok(new SeatResponse
                {
                    RoomName = roomName,
                    Seats = seatDtos
                });
            }
            catch (NotFoundException ex)
            {
                return ErrorResp.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return ErrorResp.InternalServerError(ex.Message);
            }
        }

        public async Task<IActionResult> ValidateSelectedSeats(Guid showTimeId, List<Guid> seatIds)
        {
            try
            {
                // Validate
                if (seatIds == null)
                    return ErrorResp.BadRequest("Please select seats");

                if (seatIds.Count == 0)
                    return SuccessResp.Ok(new { IsValid = true, Message = "No seats selected" });

                if (seatIds.Count > 8)
                    return ErrorResp.BadRequest("Maximum 8 seats per booking");

                // Kiểm tra ghế có hợp lệ không
                var isValid = await _seatRepository.ValidateSeatsAsync(showTimeId, seatIds);
                if (!isValid)
                    return ErrorResp.BadRequest("Some seats are invalid");

                // Kiểm tra ghế còn trống
                var (_, _, bookedSeatIds) = await _seatRepository.GetSeatInfoAsync(showTimeId);
                var alreadyBooked = seatIds.Any(id => bookedSeatIds.Contains(id));
                if (alreadyBooked)
                    return ErrorResp.BadRequest("Some seats are already booked");

                // Cập nhật trạng thái IsActive của ghế
                foreach (var seatId in seatIds)
                {
                    var seat = await _seatRepository.GetByIdAsync(seatId);
                    if (seat != null)
                    {
                        seat.IsActive = false;
                        // Bạn cần thêm phương thức Update trong repository hoặc sử dụng context trực tiếp
                        await _seatRepository.UpdateSeatAsync(seat);
                        // _context.SaveChangesAsync();
                    }
                }

                return SuccessResp.Ok(new
                {
                    IsValid = true,
                    Message = "Seats have been successfully reserved",
                    SelectedSeatCount = seatIds.Count
                });
            }
            catch (Exception ex)
            {
                return ErrorResp.InternalServerError(ex.Message);
            }
        }


        public async Task<IActionResult> GetShowTimeDetails(Guid showTimeId)
        {
            try
            {
                var showTime = await _showTimeRepository.FindByIdAsync(showTimeId);
                if (showTime == null)
                {
                    return ErrorResp.NotFound($"ShowTime with ID {showTimeId} not found.");
                }

                var response = new ShowTimeDetailsDto
                {
                    MovieId = showTime.MovieId,
                    RoomId = showTime.RoomId,
                    ShowDate = showTime.ShowDate
                };

                return SuccessResp.Ok(response);
            }
            catch (Exception ex)
            {
                return ErrorResp.InternalServerError(ex.Message);
            }
        }
    }
}
