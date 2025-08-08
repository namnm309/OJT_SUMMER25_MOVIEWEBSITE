using ApplicationLayer.Middlewares;
using ApplicationLayer.Services.TicketSellingManagement;
using Microsoft.AspNetCore.Mvc;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/ticket")]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        
        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [Protected]
        [HttpGet("booking")]
        public async Task<IActionResult> GetByBooking(Guid bookingId)
        {
            return await _ticketService.GetTicketsByBookingIdAsync(bookingId);
        }

        [Protected]
        [HttpGet("User")]
        public async Task<IActionResult> GetByUser()
        {
            return await _ticketService.GetTicketsByUserIdAsync();
        }

        [Protected]
        [HttpGet("{ticketId}")]
        public async Task<IActionResult> GetByTicketId(Guid ticketId)
        {
            return await _ticketService.GetTicketByIdAsync(ticketId);
        }
    }
}
