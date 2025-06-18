using ApplicationLayer.DTO.CinemaRoomManagement;
using ApplicationLayer.Services.CinemaRoomManagement;
using DomainLayer.Enum;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ControllerLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class CinemaRoomController : ControllerBase
    {
        private readonly ICinemaRoomService _cinemaRoomService;
        public CinemaRoomController(ICinemaRoomService cinemaRoomService)
        {
            _cinemaRoomService = cinemaRoomService;
        }

        //Cinema Room Management
        // GET /api/cinemarooms
        // Returns all cinema rooms.
        [HttpGet]
        public async Task<ActionResult<List<CinemaRoomDto>>> GetAll()
        {
            var rooms = await _cinemaRoomService.GetAllCinemaRoomsAsync();
            return Ok(rooms);
        }

        // POST /api/cinemarooms
        // Creates a new cinema room.
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CinemaRoomDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _cinemaRoomService.AddCinemaRoomAsync(dto);
            return CreatedAtAction(nameof(GetAll), null);
        }

        // GET /api/cinemarooms/search?keyword=foo
        // Searches cinema rooms by keyword.
        [HttpGet("search")]
        public async Task<ActionResult<List<CinemaRoomDto>>> Search([FromQuery] string keyword)
        {
            try
            {
                var rooms = await _cinemaRoomService.SearchCinemaRoomsAsync(keyword);
                return Ok(rooms);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
