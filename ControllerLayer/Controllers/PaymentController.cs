using Application.ResponseCode;
using ApplicationLayer.DTO.Payment;
using ApplicationLayer.Middlewares;
using ApplicationLayer.Services.Payment;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/payment")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [Protected]
        [HttpPost("vnpay")]
        public async Task<IActionResult> CreateVnpayPayment([FromBody] PaymentRequestDto Dto)
        {
            _logger.LogInformation("Creating VNPay payment for BookingId={BookingId}, Amount={Amount}", Dto.BookingId, Dto.Amount);

            var response = await _paymentService.CreateVnPayPayment(HttpContext, Dto);

            return Ok(response);
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnpayReturn()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    // Lấy các tham số từ query string
                    var response = await _paymentService.CallBack(Request.Query);

                    if (response.Success)
                    {
                        // Redirect to UI success page dựa trên booking source
                        //var uiBase = "https://cinemacity-frontend-dcayhqe2h3f7djhq.eastasia-01.azurewebsites.net"; // Production URL
                        var uiBase = "https://www.cinemacity.app";
                        var redirectUrl = GetRedirectUrlByBookingSource(response.BookingSource, response.BookingCode, true);
                        var fullRedirectUrl = $"{uiBase}{redirectUrl}";
                        
                        _logger.LogInformation("VNPay Success Redirect - BookingSource: {BookingSource}, BookingCode: {BookingCode}, RedirectUrl: {RedirectUrl}", 
                            response.BookingSource, response.BookingCode, fullRedirectUrl);
                        
                        return Redirect(fullRedirectUrl);
                    }

                    // Redirect to UI fail page dựa trên booking source
                    //var uiBaseFail = "https://cinemacity-frontend-dcayhqe2h3f7djhq.eastasia-01.azurewebsites.net"; // Production URL
                    var uiBaseFail = "https://www.cinemacity.app";
                    var failRedirectUrl = GetRedirectUrlByBookingSource(response.BookingSource, response.BookingCode, false);
                    return Redirect($"{uiBaseFail}{failRedirectUrl}");
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Đã xảy ra lỗi",
                        error = ex.Message
                    });
                }
            }

            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy thông tin thanh toán"
            });
        }

        /// <summary>
        /// Xác định URL redirect dựa trên booking source và kết quả thanh toán
        /// </summary>
        /// <param name="bookingSource">Nguồn tạo booking (admin_dashboard, user)</param>
        /// <param name="bookingCode">Mã booking</param>
        /// <param name="isSuccess">True nếu thanh toán thành công, False nếu thất bại</param>
        /// <returns>URL redirect tương ứng</returns>
        private string GetRedirectUrlByBookingSource(string bookingSource, string bookingCode, bool isSuccess)
        {
            // Phân biệt nguồn tạo booking
            var isFromAdminDashboard = bookingSource.Equals("admin_dashboard", StringComparison.OrdinalIgnoreCase);

            if (isSuccess)
            {
                // Trang success
                if (isFromAdminDashboard)
                {
                    // Từ admin dashboard → trang admin dashboard
                    return $"/BookingManagement/BookingTicket/PaymentSuccess?bookingCode={bookingCode}";
                }
                else
                {
                    // Từ user thường → trang user thường
                    return $"/BookingManagement/Booking/PaymentSuccess?bookingCode={bookingCode}";
                }
            }
            else
            {
                // Trang fail
                if (isFromAdminDashboard)
                {
                    // Từ admin dashboard → trang admin dashboard
                    return $"/BookingManagement/BookingTicket/PaymentFail?bookingCode={bookingCode}";
                }
                else
                {
                    // Từ user thường → trang user thường
                    return $"/BookingManagement/Booking/PaymentFailed?bookingCode={bookingCode}";
                }
            }
        }
    }
}
