using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.Services.BookingTicketManagement;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/seatsignal")]
    public class SeatSignalController : Controller
    {
        private readonly ISeatSignalService _seatSignalService;

        public SeatSignalController(ISeatSignalService seatSignalService)
        {
            _seatSignalService = seatSignalService;
        }

        [HttpPost("hold")]
        public async Task<IActionResult> HoldSeats([FromBody] HoldSeatRequestDto dto)
        {
            return await _seatSignalService.HoldSeatsAsync(dto);
        }

        
        [HttpPost("summary")]
        public async Task<IActionResult> GetSummary([FromBody] SeatSummaryRequestDto dto )
        {
            var summary = await _seatSignalService.GetSummaryAsync(dto);
            return Ok(summary);
        }

        
        [HttpDelete("release/{seatLogId}")]
        public async Task<IActionResult> ReleaseSeats([FromRoute] Guid seatLogId)
        {
            return await _seatSignalService.ReleaseSeatsAsync(seatLogId);
        }
    }
}
