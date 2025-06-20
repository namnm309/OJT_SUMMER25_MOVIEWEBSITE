using ApplicationLayer.DTO;
using ApplicationLayer.DTO.CinemaRoomManagement;
using ApplicationLayer.Services.CinemaRoomManagement;
using Microsoft.AspNetCore.Mvc;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/v1/cinemaroom")]
    public class CinemaRoomController : Controller
    {
        private readonly ICinemaRoomService _cinemaRoomService;
        private readonly ILogger<CinemaRoomController> _logger;

        public CinemaRoomController(ICinemaRoomService cinemaRoomService, ILogger<CinemaRoomController> logger)
        {
            _cinemaRoomService = cinemaRoomService;
            _logger = logger;
        }

        [HttpGet("ViewRoom")]
        public async Task<IActionResult> ViewAllCinemaRoom()
        {
            _logger.LogInformation("View CinemaRoom");
            return await _cinemaRoomService.GetAllCinemaRoom();
        }

        [HttpGet("ViewRoomPagination")]
        public async Task<IActionResult> ViewCinemaRoom([FromQuery] PaginationReq query)
        {
            _logger.LogInformation("View CinemaRoom");
            return await _cinemaRoomService.GetAllCinemaRoomPagination(query);
        }

        [HttpGet("ViewSeat")]
        public async Task<IActionResult> ViewSeat(Guid Id)
        {
            _logger.LogInformation("View Seat");
            return await _cinemaRoomService.GetSeatDetail(Id);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddCinemaRoom(CinemaRoomCreateDto Dto)
        {
            _logger.LogInformation("Add CinemaRoom");
            return await _cinemaRoomService.AddCinemaRoom(Dto);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCinemaRoom([FromQuery] string? keyword)
        {
            _logger.LogInformation("Search CinemaRoom");
            return await _cinemaRoomService.SearchCinemaRoom(keyword);
        }

        //admin
        [HttpGet("rooms/{roomId}/seats")]
        public async Task<IActionResult> ViewSeats(Guid roomId)
        {
            _logger.LogInformation("View Seats");
            return await _cinemaRoomService.ViewSeat(roomId);
        }

        //admin
        [HttpPut("rooms/seats/update")]
        public async Task<IActionResult> UpdateSeats([FromBody] UpdateSeatTypesRequest request)
        {
            _logger.LogInformation("Update seats");
            return await _cinemaRoomService.UpdateSeatTypes(request);
        }


    }
}
