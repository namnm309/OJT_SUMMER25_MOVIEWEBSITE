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
        // Thay đổi kiểu trả về nếu muốn truyền thêm thông tin đã validate
        // Hiện tại vẫn giữ IActionResult, nhưng logic bên trong sẽ không cập nhật trạng thái ghế
        Task<IActionResult> ValidateSelectedSeats(Guid showTimeId, List<Guid> seatIds);

        Task<IActionResult> GetShowTimeDetails(Guid showTimeId);
    }
    public class SeatService : ISeatService
    {

        private readonly ISeatRepository _seatRepository;
        private readonly IGenericRepository<ShowTime> _showTimeRepository;
        private readonly IMapper _mapper;

        public SeatService(
            ISeatRepository seatRepository,
            IGenericRepository<ShowTime> showTimeRepository,
            IMapper mapper)
        {
            _seatRepository = seatRepository;
            _showTimeRepository = showTimeRepository;
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

        // PHẦN ĐƯỢC SỬA ĐỔI: ValidateSelectedSeats
        public async Task<IActionResult> ValidateSelectedSeats(Guid showTimeId, List<Guid> seatIds)
        {
            try
            {
                // Validate input
                if (seatIds == null || seatIds.Count == 0)
                    return ErrorResp.BadRequest("Please select at least one seat.");

                if (seatIds.Count > 8)
                    return ErrorResp.BadRequest("Maximum 8 seats per booking.");

                // 1. Kiểm tra ghế có hợp lệ không (cùng phòng với suất chiếu)
                var showTime = await _showTimeRepository.FindByIdAsync(showTimeId, "Room"); // Include Room để lấy RoomId
                if (showTime == null)
                {
                    return ErrorResp.NotFound($"ShowTime with ID {showTimeId} not found.");
                }

                // Lấy tất cả các ghế được chọn
                var selectedSeatsEntities = await _seatRepository.GetSeatsByIdsAsync(seatIds); // Thêm phương thức này vào ISeatRepository/SeatRepository

                if (selectedSeatsEntities.Count != seatIds.Count)
                {
                    return ErrorResp.BadRequest("Some selected seats were not found.");
                }

                // 2. Kiểm tra tất cả các ghế có thuộc cùng một phòng chiếu và đang Active không
                foreach (var seat in selectedSeatsEntities)
                {
                    if (seat.RoomId != showTime.RoomId)
                    {
                        return ErrorResp.BadRequest($"Seat {seat.SeatCode} is not in the correct room for this showtime.");
                    }
                    if (!seat.IsActive)
                    {
                        return ErrorResp.BadRequest($"Seat {seat.SeatCode} is currently inactive."); // Hoặc có thể đã bị vô hiệu hóa
                    }
                }

                // 3. Kiểm tra xem các ghế đã chọn có bị đặt bởi người khác cho suất chiếu này không
                var bookedSeatIds = await _seatRepository.GetBookedSeatIdsForShowTimeAsync(showTimeId);
                var alreadyBookedSeats = seatIds.Where(id => bookedSeatIds.Contains(id)).ToList();

                if (alreadyBookedSeats.Any())
                {
                    // Lấy SeatCode của các ghế đã đặt để hiển thị thông báo thân thiện
                    var bookedSeatCodes = selectedSeatsEntities
                                            .Where(s => alreadyBookedSeats.Contains(s.Id))
                                            .Select(s => s.SeatCode)
                                            .ToList();
                    return ErrorResp.BadRequest($"The following seats are already booked: {string.Join(", ", bookedSeatCodes)}.");
                }

                // Nếu mọi thứ đều ổn
                return SuccessResp.Ok(new
                {
                    IsValid = true,
                    Message = "Selected seats are valid and available for booking."
                });
            }
            catch (Exception ex)
            {
                return ErrorResp.InternalServerError("An error occurred during seat validation. Please try again.");
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