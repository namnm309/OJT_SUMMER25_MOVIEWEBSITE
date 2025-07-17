using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.Middlewares;
using ApplicationLayer.Services.BookingTicketManagement;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/seatsignal")]
    public class SeatSignalController : ControllerBase
    {
        private readonly ISeatSignalService _seatSignalService;

        public SeatSignalController(ISeatSignalService seatSignalService)
        {
            _seatSignalService = seatSignalService;
        }

        [Protected]
        [HttpPost("hold")]
        public async Task<IActionResult> Hold([FromBody] HoldSeatSignalRequest req)
        {
            var result = await _seatSignalService.HoldSeatAsync(req);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(new
            {
                bookingId = result.BookingId,
                seatIds = req.SeatIds,
                expiredAt = result.ExpiredAt
            });
        }

        [Protected]
        [HttpPost("release")]
        public async Task<IActionResult> Release([FromBody] ReleaseSeatSignalRequest req)
        {
            var result = await _seatSignalService.ReleaseSeatAsync(req);
            if (result.Success)
                return Ok(new { message = "Released successfully" });
            return BadRequest(new { message = result.Message });
        }
    }
}
