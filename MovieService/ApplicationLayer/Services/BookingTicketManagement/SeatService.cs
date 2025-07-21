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
        //Task<IActionResult> ConfirmBookingAsync(ConfirmBookingRequest request);

        //Task<IActionResult> CancelBooking(Guid bookingId);
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

        //public async Task<IActionResult> ConfirmBookingAsync(ConfirmBookingRequest request)
        //{
            

        //    if (request.SeatIds == null || request.SeatIds.Count == 0)
        //        return ErrorResp.BadRequest("Please select at least one seat");

        //    if (request.SeatIds.Count > 8)
        //        return ErrorResp.BadRequest("Maximum 8 seats per booking");

        //    var showtime = await _showTimeRepository.FindByIdAsync(request.ShowTimeId);
        //    if (showtime == null)
        //        return ErrorResp.NotFound("Showtime Not Found");

        //    var seats = await _seatRepository.GetSeatsByIdsAsync(request.SeatIds);
        //    if (seats.Count != request.SeatIds.Count)
        //        return ErrorResp.BadRequest("Some seats are invalid");

        //    var bookedSeatsIds = await _seatRepository.GetBookedSeatIdsForShowTimeAsync(request.ShowTimeId);

        //    var alreadyBooked = seats.Where(s => bookedSeatsIds.Contains(s.Id)).ToList();
        //    if (alreadyBooked.Any())
        //    {
        //        var seatCodes = string.Join(", ", alreadyBooked.Select(s => s.SeatCode));
        //        return ErrorResp.BadRequest($"The following seats are already booked: {seatCodes}");
        //    }

        //    var user = await _userRepo.FindByIdAsync(userId);
        //    if (user == null)
        //        return ErrorResp.NotFound("User not found");

        //    // Tạo booking
        //    decimal totalPrice = seats.Sum(s => s.PriceSeat);
        //    var booking = new Booking
        //    {
        //        Id = Guid.NewGuid(),
        //        UserId = userId,
        //        ShowTimeId = request.ShowTimeId,
        //        BookingCode = GenerateBookingCode(),
        //        BookingDate = DateTime.UtcNow,
        //        TotalPrice = totalPrice,
        //        TotalSeats = seats.Count,
        //        Status = BookingStatus.Pending,
        //        FullName = user.FullName,
        //        Email = user.Email,
        //        PhoneNumber = user.Phone,
        //        IdentityCardNumber = user.IdentityCard,
        //        CreatedAt = DateTime.UtcNow,
        //        UpdatedAt = DateTime.UtcNow
        //    };

        //    await _bookingRepo.CreateAsync(booking);

        //    //Ghi log từng ghế
        //    var details = seats.Select(s => new BookingDetail
        //    {
        //        Id = Guid.NewGuid(),
        //        BookingId = booking.Id,
        //        SeatId = s.Id,
        //        Price = s.PriceSeat,
        //        CreatedAt = DateTime.UtcNow,
        //        UpdatedAt = DateTime.UtcNow
        //    }).ToList();

        //    await _bookingDetailRepo.CreateRangeAsync(details);

        //    foreach (var seat in seats)
        //    {
        //        seat.Status = SeatStatus.Pending;
        //        await _seatRepository.UpdateSeatAsync(seat);
        //    }

        //    return SuccessResp.Ok(new
        //    {
        //        BookingId = booking.Id,
        //        Total = totalPrice,
        //        Seats = seats.Select(s => s.SeatCode)
        //    });
        //}

        //public async Task<IActionResult> CancelBooking(Guid bookingId)
        //{
            

        //    var booking = await _bookingRepo.FindAsync(b => b.Id == bookingId && b.UserId == userId);
        //    if (booking == null)
        //        return ErrorResp.NotFound("Booking not found or access denied.");

        //    if (booking.Status != BookingStatus.Pending)
        //        return ErrorResp.BadRequest("Only pending bookings can be canceled.");

        //    // 2. Đổi trạng thái booking
        //    booking.Status = BookingStatus.Canceled;
        //    booking.UpdatedAt = DateTime.UtcNow;
        //    await _bookingRepo.UpdateAsync(booking);

        //    // 3. Lấy danh sách BookingDetails và danh sách ghế
        //    var bookingDetails = await _bookingDetailRepo.FindAllAsync(d => d.BookingId == booking.Id);
        //    var seatIds = bookingDetails.Select(d => d.SeatId).ToList();
        //    var seats = await _seatRepository.GetSeatsByIdsAsync(seatIds);

        //    // Cập nhật trạng thái ghế từ Pending -> Available
        //    foreach (var seat in seats)
        //    {
        //        if (seat.Status == SeatStatus.Pending)
        //        {
        //            seat.Status = SeatStatus.Available;
        //        }
        //    }

        //    // Cập nhật danh sách ghế
        //    await _seatRepository.UpdateSeatsAsync(seats);

        //    return SuccessResp.Ok(new
        //    {
        //        Message = "Booking has been canceled successfully.",
        //        BookingId = booking.Id,
        //        CanceledAt = booking.UpdatedAt
        //    });
        //}
    }
}