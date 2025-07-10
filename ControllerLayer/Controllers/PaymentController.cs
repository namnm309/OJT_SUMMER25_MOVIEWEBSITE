using Application.ResponseCode;
using ApplicationLayer.DTO.Payment;
using ApplicationLayer.Middlewares;
using ApplicationLayer.Services.Payment;
using Microsoft.AspNetCore.Mvc;

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
                        return Ok(response);
                    }

                    return BadRequest(new
                    {
                        success = false,
                        message = "Thanh toán thất bại",
                        vnPayResponseCode = response.VnPayResponseCode
                    });
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
    }
}
