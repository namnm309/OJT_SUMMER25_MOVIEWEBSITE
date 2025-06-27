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

                var (roomName, _, _) = await _seatRepository.GetSeatInfoAsync(showTimeId);
                // Lấy thông tin phòng từ showTimeId
                var showTime = await _showTimeRepository.FindByIdAsync(showTimeId);
                if (showTime == null)
                {
                    return ErrorResp.NotFound($"ShowTime with ID {showTimeId} not found.");
                }

                // Lấy toàn bộ ghế trong phòng đó (kể cả inactive)
                var seats = await _seatRepository.GetSeatsByRoomIdAsync(showTime.RoomId);

                // Lấy danh sách ghế đã đặt cho suất chiếu này
                var bookedSeatIds = await _seatRepository.GetBookedSeatIdsForShowTimeAsync(showTimeId);

                // Chuyển đổi sang DTO
                var seatDtos = seats.Select(s => new SeatDto
                {
                    Id = s.Id,
                    SeatCode = s.SeatCode,
                    SeatType = s.SeatType.ToString(),
                    RowIndex = s.RowIndex,
                    ColumnIndex = s.ColumnIndex,
                    IsAvailable = s.IsActive && !bookedSeatIds.Contains(s.Id), // Ghế available khi IsActive=true và chưa được đặt
                    Price = s.PriceSeat,
                  
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
                // Validate input
                if (seatIds == null || seatIds.Count == 0)
                    return ErrorResp.BadRequest("Please select at least one seat");

                if (seatIds.Count > 8)
                    return ErrorResp.BadRequest("Maximum 8 seats per booking");

                // Kiểm tra ghế có hợp lệ không (cùng phòng với suất chiếu)
                var isValid = await _seatRepository.ValidateSeatsAsync(showTimeId, seatIds);
                if (!isValid)
                    return ErrorResp.BadRequest("Some seats are invalid or not in the same room as the showtime");

                // Kiểm tra ghế có đang active (chưa đặt) không
                var seats = new List<Seat>();
                foreach (var seatId in seatIds)
                {
                    var seat = await _seatRepository.GetByIdAsync(seatId);
                    if (seat == null)
                        return ErrorResp.BadRequest($"Seat with ID {seatId} not found");

                    if (!seat.IsActive)
                        return ErrorResp.BadRequest($"Seat {seat.SeatCode} is already booked");

                    seats.Add(seat);
                }

                // Cập nhật trạng thái IsActive của ghế
                foreach (var seat in seats)
                {
                    seat.IsActive = false;
                    await _seatRepository.UpdateSeatAsync(seat);
                }

                return SuccessResp.Ok(new
                {
                    IsValid = true,
                    Message = "Booking seat successfully",
                   
                });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error validating and reserving seats");
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
