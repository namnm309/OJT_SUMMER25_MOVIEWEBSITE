using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.ResponseCode;
using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.DTO.CinemaRoomManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using DomainLayer.Exceptions;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;

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
    public class SeatService : BaseService, ISeatService
    {

        private readonly ISeatRepository _seatRepository;
        private readonly IGenericRepository<ShowTime> _showTimeRepository;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<BookingDetail> _bookingDetailRepo;
        private readonly IGenericRepository<Users> _userRepo;
        private readonly IGenericRepository<SeatLog> _seatLogRepo;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpCtx;

        public SeatService(
            ISeatRepository seatRepository,
            IGenericRepository<ShowTime> showTimeRepository,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<BookingDetail> bookingDetailRepo,
            IGenericRepository<Users> userRepo,
            IGenericRepository<SeatLog> seatLogRepo,
            IMapper mapper,
            IHttpContextAccessor httpCtx) : base(mapper, httpCtx)
        {
            _seatRepository = seatRepository;
            _showTimeRepository = showTimeRepository;
            _bookingRepo = bookingRepo;
            _bookingDetailRepo = bookingDetailRepo;
            _userRepo = userRepo;
            _seatLogRepo = seatLogRepo;
            _mapper = mapper;
            _httpCtx = httpCtx;
        }

        private string GenerateBookingCode()
        {
            // Logic tạo mã booking duy nhất (ví dụ: kết hợp thời gian và random string)
            return "BK" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

        }

        public async Task<IActionResult> GetAvailableSeats(Guid showTimeId)
        {
            var payload = ExtractPayload();
            if (payload == null)
                return ErrorResp.Unauthorized("Invaild token");

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
                    IsAvailable = s.Status == SeatStatus.Available && !bookedSeatIds.Contains(s.Id),
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
            var payload = ExtractPayload();
            if (payload == null)
                return ErrorResp.Unauthorized("Invaild token");

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
                var selectedSeatsEntities = await _seatRepository.GetSeatsByIdsAsync(seatIds); 

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
                    if (seat.Status != SeatStatus.Available)
                    {
                        return ErrorResp.BadRequest($"Seat {seat.SeatCode} is not available.");
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