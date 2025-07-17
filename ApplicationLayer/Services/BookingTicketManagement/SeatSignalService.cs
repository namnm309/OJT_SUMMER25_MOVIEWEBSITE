using Application.ResponseCode;
using ApplicationLayer.DTO.BookingTicketManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.BookingTicketManagement
{
    public interface ISeatSignalService
    {
        Task<IActionResult> HoldSeatsAsync(HoldSeatRequestDto dto);
        Task<IActionResult> GetSummaryAsync(SeatSummaryRequestDto request);
        Task <IActionResult> ReleaseSeatsAsync(Guid bookingId);
        public class SeatSignalService : BaseService, ISeatSignalService
        {
            private readonly IGenericRepository<SeatLog> _seatLogRepo;
            private readonly IGenericRepository<Seat> _seatRepo;
            private readonly IGenericRepository<Booking> _bookingRepo;
            private readonly IGenericRepository<BookingDetail> _bookingDetailRepo;
            private readonly IGenericRepository<ShowTime> _showTimeRepo;
            private readonly IGenericRepository<Users> _userRepo;
            private readonly IGenericRepository<Promotion> _promotionRepo;
            private readonly IHubContext<SeatHub> _hubContext;
            private readonly IMapper _mapper;
            private readonly IHttpContextAccessor _httpCtx;

            public SeatSignalService(IGenericRepository<SeatLog> seatLogRepo,
                                    IGenericRepository<Seat> seatRepo,
                                    IGenericRepository<Booking> bookingRepo,
                                    IGenericRepository<BookingDetail> bookingDetailRepo,
                                    IGenericRepository<ShowTime> showTimeRepo,
                                    IGenericRepository<Users> userRepo,
                                    IGenericRepository<Promotion> promotionRepo,
                                    IHubContext<SeatHub> hubContext, 
                                    IMapper mapper, 
                                    IHttpContextAccessor httpCtx) : base(mapper, httpCtx)
            {
                _seatLogRepo = seatLogRepo;
                _seatRepo = seatRepo;
                _bookingRepo = bookingRepo;
                _bookingDetailRepo = bookingDetailRepo;
                _showTimeRepo = showTimeRepo;
                _userRepo = userRepo;
                _promotionRepo = promotionRepo;
                _hubContext = hubContext;
                _mapper = mapper;
                _httpCtx = httpCtx;
            }

            private string GenerateBookingCode()
            {
                // Logic tạo mã booking duy nhất (ví dụ: kết hợp thời gian và random string)
                return "BK" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

            }

            public async Task<IActionResult> HoldSeatsAsync(HoldSeatRequestDto dto)
            {
                var payload = ExtractPayload();
                if (payload == null)
                    return ErrorResp.Unauthorized("Invaild token");

                var userId = payload.UserId;
                var now = DateTime.UtcNow;
                var expiredAt = now.AddMinutes(15);

                var exitsShowTime = await _showTimeRepo.FindByIdAsync(dto.ShowTimeId);
                if (exitsShowTime == null)
                    return ErrorResp.NotFound("ShowTime Not Found");

                // Kiểm tra ghế
                var seat = await _seatRepo.FindByIdAsync(dto.SeatId);
                if (seat == null)
                    return ErrorResp.NotFound("Seat not found");

                // Check nếu ghế đang Pending hoặc Selected thì không giữ được nữa
                if (seat.Status != SeatStatus.Available)
                    return ErrorResp.BadRequest("Seat is already held or booked");

                // Cập nhật trạng thái ghế
                seat.Status = SeatStatus.Pending;
                await _seatRepo.UpdateAsync(seat);

                // Tạo SeatLog mới cho 1 ghế
                var seatLog = new SeatLog
                {
                    Id = Guid.NewGuid(),
                    ShowTimeId = dto.ShowTimeId,
                    UserId = userId,
                    SeatId = seat.Id,
                    ExpiredAt = expiredAt,
                    Status = SeatStatus.Pending
                };
                await _seatLogRepo.CreateAsync(seatLog);

                // Push real-time update qua SignalR
                await _hubContext.Clients.Group(dto.ShowTimeId.ToString())
                    .SendAsync("SeatsHeld", new
                    {
                        SeatId = seat.Id,
                        SeatCode = seat.SeatCode,
                        Status = SeatStatus.Pending.ToString()
                    });

                return SuccessResp.Ok(new
                {
                    SeatLogId = seatLog.Id,
                    SeatId = seat.Id,
                    ExpiredAt = seatLog.ExpiredAt
                });
            }

            public async Task<IActionResult> GetSummaryAsync(SeatSummaryRequestDto request)
            {
                var payload = ExtractPayload();
                if (payload == null)
                    return ErrorResp.Unauthorized("Invalid token");

                var userId = payload.UserId;

                var user = await _userRepo.FindByIdAsync(userId);
                if (user == null)
                    return ErrorResp.NotFound("User Not Found");

                if (request.SeatLogId == null || request.SeatLogId.Count == 0)
                    return ErrorResp.BadRequest("SeatLogId list is required");

                var now = DateTime.UtcNow;

                // Lấy danh sách seatLogs còn hiệu lực và include ghế
                var seatLogs = await _seatLogRepo.WhereAsync(s => request.SeatLogId.Contains(s.Id) && s.ExpiredAt > now, "Seat");

                if (seatLogs == null || seatLogs.Count == 0)
                    return ErrorResp.BadRequest("No valid seat logs found");

                // Dữ liệu tính toán
                var seatCodes = seatLogs.Select(x => x.Seat.SeatCode).ToList();
                var totalPrice = seatLogs.Sum(x => x.Seat.PriceSeat);
                var showTimeId = seatLogs.First().ShowTimeId;
                var expiredAt = seatLogs.Max(x => x.ExpiredAt);

                decimal finalPrice = totalPrice;
                int discountPercent = 0;

                if (request.PromotionId.HasValue)
                {
                    var promotion = await _promotionRepo.FindByIdAsync(request.PromotionId.Value);
                    if (promotion != null && promotion.StartDate <= now && promotion.EndDate >= now)
                    {
                        discountPercent = promotion.DiscountPercent;
                        finalPrice = totalPrice * (1 - discountPercent / 100m);
                    }
                }

                // Tạo Booking
                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.Phone,
                    IdentityCardNumber = user.IdentityCard,
                    BookingCode = GenerateBookingCode(),
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.Pending,
                    TotalPrice = finalPrice,
                    TotalSeats = seatLogs.Count,
                    ShowTimeId = showTimeId,
                    UserId = payload.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _bookingRepo.CreateAsync(booking);

                // Tạo các BookingDetail tương ứng
                foreach (var log in seatLogs)
                {
                    var bookingDetail = new BookingDetail
                    {
                        Id = Guid.NewGuid(),
                        BookingId = booking.Id,
                        SeatId = log.SeatId,
                        Price = log.Seat.PriceSeat,
                        CreatedAt = now
                    };

                    await _bookingDetailRepo.CreateAsync(bookingDetail);
                }

                return SuccessResp.Ok(new SeatSummaryDto
                {
                    BookingId = booking.Id,
                    BookingCode = booking.BookingCode,
                    SeatCodes = seatCodes,
                    Quantity = seatCodes.Count,
                    TotalPrice = totalPrice,
                    DiscountPercent = discountPercent,
                    FinalPrice = finalPrice,
                    ExpiredAt = expiredAt
                });
            }

            public async Task<IActionResult> ReleaseSeatsAsync(Guid seatLogId)
            {
                var payload = ExtractPayload();
                if (payload == null)
                    return ErrorResp.Unauthorized("Invalid token");

                // Chỉ Include SeatLogDetails, không Include Seat => tránh EF tracking dư thừa
                var seatLog = await _seatLogRepo.FoundOrThrowAsync(
                    seatLogId,
                    "Seat log not found",
                    "SeatLogDetails"
                );

                // Chỉ người tạo log mới được xóa
                if (seatLog.UserId != payload.UserId)
                    return ErrorResp.Forbidden("You are not allowed to release this seat");

                var seat = seatLog.Seat;
                seat.Status = SeatStatus.Available;

                /// Cập nhật trạng thái ghế
                await _seatRepo.UpdateAsync(seat);

                // Xóa seat log sau khi release
                await _seatLogRepo.DeleteAsync(seatLog);

                // Gửi thông báo tới tất cả client đang xem showtime đó
                await _hubContext.Clients.Group(seatLog.ShowTimeId.ToString())
                    .SendAsync("SeatsReleased", new List<Guid> { seat.Id });

                return SuccessResp.Ok("Seats released successfully");
            }
        }
        
    }
}
