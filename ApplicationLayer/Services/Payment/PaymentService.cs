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
        private readonly ITicketService _ticketService;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpCtx;

        public PaymentService(IGenericRepository<Booking> bookingRepo, IGenericRepository<Transaction> transactionRepo, IGenericRepository<BookingDetail> bookingDetailRepo, IGenericRepository<Seat> seatRepo, IGenericRepository<SeatLog> seatLogRepo, ITicketService ticketService, IConfiguration config, IMapper mapper, IHttpContextAccessor httpCtx) : base(mapper, httpCtx)
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
                BookingCode = booking != null ? booking.BookingCode : string.Empty
            };
        }
    }
}
