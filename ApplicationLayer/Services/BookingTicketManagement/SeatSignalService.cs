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
        Task<IActionResult> GetSummaryAsync(SeatSummaryRequestDto dto);
        Task <IActionResult> ReleaseSeatsAsync(Guid bookingId);
        public class SeatSignalService : BaseService, ISeatSignalService
        {
            private readonly IGenericRepository<SeatLog> _seatLogRepo;
            private readonly IGenericRepository<Seat> _seatRepo;
            private readonly IGenericRepository<SeatLogDetail> _seatLogDetailRepo;
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
                                    IGenericRepository<SeatLogDetail> seatLogDetailRepo,
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
                _seatLogDetailRepo = seatLogDetailRepo;
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

                // Lấy danh sách ghế cần giữ
                var seats = await _seatRepo.WhereAsync(s => dto.SeatIds.Contains(s.Id) && s.Status == SeatStatus.Available);
                if (seats.Count != dto.SeatIds.Count)
                    return ErrorResp.BadRequest("Some seats are not available");

                // Tạo SeatLog
                var seatLog = new SeatLog
                {
                    ShowTimeId = dto.ShowTimeId,
                    UserId = userId,
                    ExpiredAt = expiredAt,
                    Status = SeatStatus.Pending
                };
                seatLog = await _seatLogRepo.CreateAsync(seatLog);

                // Lưu chi tiết ghế log
                var seatLogDetails = seats.Select(seat => new SeatLogDetail
                {
                    SeatId = seat.Id,
                    SeatLogId = seatLog.Id
                }).ToList();

                await _seatLogDetailRepo.CreateRangeAsync(seatLogDetails);

                // Cập nhật trạng thái ghế
                foreach (var seat in seats)
                {
                    seat.Status = SeatStatus.Pending;
                }
                await _seatRepo.UpdateRangeAsync(seats);

                // Gửi tín hiệu tới SignalR group
                await _hubContext.Clients.Group(dto.ShowTimeId.ToString())
                    .SendAsync("SeatsHeld", dto.SeatIds);

                return SuccessResp.Ok(new
                {
                    SeatLogId = seatLog.Id,
                    UserId = userId,
                    ExpiredAt = seatLog.ExpiredAt
                });
            }

            public async Task<IActionResult> GetSummaryAsync(SeatSummaryRequestDto dto)
            {
                var payload = ExtractPayload();
                if (payload == null)
                    return ErrorResp.Unauthorized("Invalid token");

                var userId = payload.UserId;

                var user = await _userRepo.FindByIdAsync(userId);
                if (user == null)
                    return ErrorResp.NotFound("User Not Found");

                var seatLog = await _seatLogRepo.FoundOrThrowAsync(dto.SeatLogId, "Seat log not found", "SeatLogDetails", "SeatLogDetails.Seat");

                var seatDetails = seatLog.SeatLogDetails;

                var seatCodes = seatLog.SeatLogDetails.Select(x => x.Seat.SeatCode).ToList();
                var totalPrice = seatLog.SeatLogDetails.Sum(x => x.Seat.PriceSeat);

                decimal finalPrice = totalPrice;
                int discountPercent = 0;

                if(dto.PromotionId.HasValue)
{
                    var promotion = await _promotionRepo.FindByIdAsync(dto.PromotionId.Value);
                    if (promotion != null && promotion.StartDate <= DateTime.UtcNow && promotion.EndDate >= DateTime.UtcNow)
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
                    TotalSeats = seatDetails.Count,
                    ShowTimeId = seatLog.ShowTimeId,
                    UserId = payload.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _bookingRepo.CreateAsync(booking);

                foreach (var item in seatDetails)
                {
                    var bookingDetail = new BookingDetail
                    {
                        Id = Guid.NewGuid(),
                        BookingId = booking.Id,
                        SeatId = item.SeatId,
                        Price = item.Seat.PriceSeat,
                        CreatedAt = DateTime.UtcNow
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
                    ExpiredAt = seatLog.ExpiredAt
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

                // Lấy SeatId từ SeatLogDetails
                var seatIds = seatLog.SeatLogDetails.Select(x => x.SeatId).ToList();

                // Truy xuất danh sách các Seat cần cập nhật
                var seats = await _seatRepo.WhereAsync(s => seatIds.Contains(s.Id));

                // Cập nhật trạng thái ghế về Available
                foreach (var seat in seats)
                {
                    seat.Status = SeatStatus.Available;
                }
                await _seatRepo.UpdateRangeAsync(seats);

                // Xóa dữ liệu log sau khi release
                await _seatLogDetailRepo.DeleteRangeAsync(seatLog.SeatLogDetails);
                await _seatLogRepo.DeleteAsync(seatLog);

                // Gửi thông báo tới tất cả client đang xem showtime đó
                await _hubContext.Clients.Group(seatLog.ShowTimeId.ToString())
                    .SendAsync("SeatsReleased", seatIds);

                return SuccessResp.Ok("Seats released successfully");
            }
        }
        
    }
}
