using Application.ResponseCode;
using ApplicationLayer.DTO.Payment;
using ApplicationLayer.Service;
using ApplicationLayer.Services.TicketSellingManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.Payment
{
    public interface IPaymentService
    {
        Task<string> CreateVnPayPayment(HttpContext context, PaymentRequestDto Dto);
        Task<PaymentResponseDto> CallBack(IQueryCollection queryParams);
    }
    public class PaymentService : BaseService, IPaymentService
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<Transaction> _transactionRepo;
        private readonly IGenericRepository<BookingDetail> _bookingDetailRepo;
        private readonly IGenericRepository<Seat> _seatRepo;
        private readonly IGenericRepository<SeatLog> _seatLogRepo;
        private readonly IGenericRepository<Users> _userRepo;
        private readonly IGenericRepository<PointHistory> _pointHistoryRepo;
        private readonly IGenericRepository<Promotion> _promotionRepo;
        private readonly IGenericRepository<UserPromotion> _userPromotionRepo;
        private readonly ITicketService _ticketService;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpCtx;

        public PaymentService(IGenericRepository<Booking> bookingRepo,
            IGenericRepository<Transaction> transactionRepo,
            IGenericRepository<BookingDetail> bookingDetailRepo,
            IGenericRepository<Seat> seatRepo,
            IGenericRepository<SeatLog> seatLogRepo,
            IGenericRepository<Users> userRepo,
            IGenericRepository<PointHistory> pointHistoryRepo,
            IGenericRepository<Promotion> promotionRepo,
            IGenericRepository<UserPromotion> userPromotionRepo,
            ITicketService ticketService,
            IConfiguration config,
            IMapper mapper,
            IHttpContextAccessor httpCtx) : base(mapper, httpCtx)
        {
            _bookingRepo = bookingRepo;
            _transactionRepo = transactionRepo;
            _bookingDetailRepo = bookingDetailRepo;
            _seatRepo = seatRepo;
            _seatLogRepo = seatLogRepo;
            _ticketService = ticketService;
            _config = config;
            _mapper = mapper;
            _httpCtx = httpCtx;
            _userRepo = userRepo;
            _pointHistoryRepo = pointHistoryRepo;
            _promotionRepo = promotionRepo;
            _userPromotionRepo = userPromotionRepo;
        }

        public async Task<string> CreateVnPayPayment(HttpContext context, PaymentRequestDto Dto)
        {
            var payload = ExtractPayload();
            if (payload == null)
                throw new UnauthorizedAccessException("Invalid token");

            var userId = payload.UserId;

            var existsbooking = await _bookingRepo.FindByIdAsync(Dto.BookingId);
            if (existsbooking == null)
                throw new UnauthorizedAccessException("Invalid booking");

            if (existsbooking.Status != DomainLayer.Enum.BookingStatus.Pending)
                throw new Exception("This booking has already been processed");

            var merchantTxnId = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BookingId = Dto.BookingId,
                Amount = Dto.Amount,
                PaymentStatus = DomainLayer.Enum.PaymentStatusEnum.Pending,
                PaymentMethod = "VnPay",
                Description = Dto.Decription,
                MerchantTransactionId = merchantTxnId
            };

            await _transactionRepo.CreateAsync(transaction);

            // Lấy thông tin cấu hình từ appsettings.json
            var vnp_TmnCode = _config["Vnpay:TmnCode"];
            var vnp_HashSecret = _config["Vnpay:HashSecret"];
            var vnp_BaseUrl = _config["Vnpay:BaseUrl"];
            var vnp_ReturnUrl = _config["Vnpay:returnUrl"];

            // Tạo request thanh toán
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", ((int)(Dto.Amount * 100)).ToString()); // nhân 100 theo yêu cầu VNPAY
            vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán vé phim");
            vnpay.AddRequestData("vnp_OrderType", "billpayment");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", merchantTxnId);

            var paymentUrl = vnpay.CreateRequestUrl(vnp_BaseUrl, vnp_HashSecret);

            return paymentUrl;
        }

        public async Task<PaymentResponseDto> CallBack(IQueryCollection queryParams)
        {
            if (queryParams == null || !queryParams.Any())
            {
                return new PaymentResponseDto { Success = false, OrderDescription = "Query parameters are missing." };
            }

            // Tạo instance VnPayLibrary và parse dữ liệu trả về
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in queryParams)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    vnpay.AddResponseData(key, value);
            }

            var vnp_merchantTransactionId = vnpay.GetResponseData("vnp_TxnRef");
            var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_SecureHash = queryParams.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");

            // Validate chữ ký hash để chống giả mạo
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _config["VnPay:HashSecret"]);
            if (!checkSignature)
            {
                return new PaymentResponseDto
                {
                    Success = false
                };
            }

            // Tìm giao dịch theo MerchantTransactionId
            var transaction = await _transactionRepo.FindAsync(t => t.MerchantTransactionId == vnp_merchantTransactionId);
            if (transaction == null)
            {
                return new PaymentResponseDto { Success = false };
            }

            Booking? booking = null;

            // Xử lý phản hồi theo mã kết quả từ VNPAY
            if (vnp_ResponseCode == "00") // Thanh toán thành công
            {
                transaction.PaymentStatus = PaymentStatusEnum.Successfully;
                transaction.CreatedAt = DateTime.UtcNow;

                // Cập nhật trạng thái booking
                booking = await _bookingRepo.FindAsync(b => b.Id == transaction.BookingId);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Completed;
                    await _bookingRepo.UpdateAsync(booking);

                    // Cập nhật trạng thái ghế từ Pending → Selected
                    var bookingDetails = await _bookingDetailRepo.FindAllAsync(d => d.BookingId == booking.Id);
                    foreach (var detail in bookingDetails)
                    {
                        var seat = await _seatRepo.FindByIdAsync(detail.SeatId);
                        if (seat != null)
                        {
                            seat.Status = SeatStatus.Selected;
                            await _seatRepo.UpdateAsync(seat);
                        }
                    }

                    // Tạo TICKET
                    await _ticketService.CreateTicketFromBookingAsync(booking.Id);

                    //Xóa SeatLog sau khi thanh toán thành công
                    var logs = await _seatLogRepo.FindAllAsync(l =>l.ShowTimeId == booking.ShowTimeId && l.UserId == booking.UserId);

                    if (logs.Any())
                    {
                        await _seatLogRepo.DeleteRangeAsync(logs);
                    }

                    // Award points and vouchers
                    await AwardPointsAndVouchersAsync(booking.UserId, booking.TotalSeats, booking.Id, booking.BookingCode);

                }
            }
            else
            {
                transaction.PaymentStatus = PaymentStatusEnum.Failed;

                // Nếu thất bại hoặc bị hủy, cập nhật booking sang Canceled
                booking = await _bookingRepo.FindAsync(b => b.Id == transaction.BookingId);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Canceled;
                    await _bookingRepo.UpdateAsync(booking);

                    var bookingDetails = await _bookingDetailRepo.FindAllAsync(d => d.BookingId == booking.Id);
                    foreach (var detail in bookingDetails)
                    {
                        var seat = await _seatRepo.FindByIdAsync(detail.SeatId);
                        if (seat != null)
                        {
                            seat.Status = SeatStatus.Available;
                            await _seatRepo.UpdateAsync(seat);
                        }
                    }
                }
            }

            // Lưu trạng thái transaction
            await _transactionRepo.UpdateAsync(transaction);

            // Lấy thông tin user role để phân biệt user vs admin
            // Sử dụng role của user hiện tại đang tạo payment, không phải role của user trong booking
            string userRole = "Member"; // Default
            string bookingSource = "user"; // Default
            
            // Lấy role của user hiện tại từ JWT token
            var payload = ExtractPayload();
            if (payload != null)
            {
                userRole = payload.Role.ToString();
                
                // Phân biệt nguồn tạo booking dựa trên role
                if (payload.Role == UserRole.Admin || payload.Role == UserRole.Staff)
                {
                    bookingSource = "admin_dashboard";
                }
                else
                {
                    bookingSource = "user";
                }
            }
            else
            {
                // Nếu không lấy được payload, thử lấy từ transaction
                if (transaction != null)
                {
                    var transactionUser = await _userRepo.FindByIdAsync(transaction.UserId);
                    if (transactionUser != null)
                    {
                        userRole = transactionUser.Role.ToString();
                        if (transactionUser.Role == UserRole.Admin || transactionUser.Role == UserRole.Staff)
                        {
                            bookingSource = "admin_dashboard";
                        }
                        else
                        {
                            bookingSource = "user";
                        }
                    }
                }
            }

            // Trả kết quả về cho người dùng
            return new PaymentResponseDto
            {
                Success = transaction.PaymentStatus == PaymentStatusEnum.Successfully,
                PaymentMethod = "VNPAY",
                OrderDescription = vnp_OrderInfo,
                OrderId = vnp_merchantTransactionId,
                TransactionId = vnp_TransactionId.ToString(),
                Token = vnp_SecureHash,
                VnPayResponseCode = vnp_ResponseCode,
                BookingCode = booking != null ? booking.BookingCode : string.Empty,
                UserRole = userRole,
                BookingSource = bookingSource
            };
        }

        private async Task AwardPointsAndVouchersAsync(Guid userId, int seatsBooked, Guid bookingId, string bookingCode)
        {
            var user = await _userRepo.FindByIdAsync(userId);
            if(user == null) return;

            // Nếu seatsBooked là 0 hoặc không truyền đúng thì truy vấn lại từ BookingDetail
            if (seatsBooked <= 0)
            {
                seatsBooked = await _bookingDetailRepo.CountAsync(bd => bd.BookingId == bookingId);
            }

            // Tính điểm dựa trên số lượng ghế
            int earned = seatsBooked switch
            {
                >= 10 => 130,
                >= 5 => 30,
                >= 2 => 10,
                _ => 0     // Dưới 2 ghế thì không có điểm
            };

            if (earned > 0)
            {
                user.Score += earned;
                await _userRepo.UpdateAsync(user);

                await _pointHistoryRepo.CreateAsync(new PointHistory
                {
                    UserId = user.Id,
                    Points = earned,
                    Type = PointType.Earned,
                    Description = $"Earned {earned} pts for booking {bookingCode} ({seatsBooked} seats)",
                    BookingId = bookingId,
                    IsUsed = false
                });
            }


            var silver = await _promotionRepo.FirstOrDefaultAsync(p=>p.Title.ToLower().Contains("silver"));
            var gold = await _promotionRepo.FirstOrDefaultAsync(p=>p.Title.ToLower().Contains("gold"));
            if(silver!=null && user.Score>=2000)
            {
                var exist=await _userPromotionRepo.FirstOrDefaultAsync(u=>u.UserId==user.Id && u.PromotionId==silver.Id);
                if(exist==null) await _userPromotionRepo.CreateAsync(new UserPromotion{UserId=user.Id,PromotionId=silver.Id});
            }
            if(gold!=null && user.Score>=5000)
            {
                var exist=await _userPromotionRepo.FirstOrDefaultAsync(u=>u.UserId==user.Id && u.PromotionId==gold.Id);
                if(exist==null) await _userPromotionRepo.CreateAsync(new UserPromotion{UserId=user.Id,PromotionId=gold.Id});
            }
        }
    }
}
