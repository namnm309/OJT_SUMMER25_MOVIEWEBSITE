// ControllerLayer.Controllers/SeatsController.cs
using ApplicationLayer.Services.BookingTicketManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/seats")] // You might consider moving this to a more general controller like 'ShowTimesController'
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;
        private readonly ILogger<SeatsController> _logger;

        public SeatsController(ISeatService seatService, ILogger<SeatsController> logger)
        {
            _seatService = seatService;
            _logger = logger;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSeats([FromQuery] Guid showTimeId)
        {
            _logger.LogInformation("Get available seats for showtime: {ShowTimeId}", showTimeId);
            return await _seatService.GetAvailableSeats(showTimeId);
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateSelectedSeats(
            [FromQuery] Guid showTimeId,
            [FromBody] List<Guid> seatIds)
        {
            _logger.LogInformation("Validating selected seats: {SeatIds} for showtime: {ShowTimeId}",
                string.Join(",", seatIds), showTimeId);

            return await _seatService.ValidateSelectedSeats(showTimeId, seatIds);
        }

        [HttpGet("{showTimeId}/details")] // New endpoint
        public async Task<IActionResult> GetShowTimeDetails(Guid showTimeId)
        {
            _logger.LogInformation("Getting details for showtime: {ShowTimeId}", showTimeId);
            return await _seatService.GetShowTimeDetails(showTimeId);
        }
    }
}