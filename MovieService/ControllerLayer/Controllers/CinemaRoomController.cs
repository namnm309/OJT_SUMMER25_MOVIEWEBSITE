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

        [HttpPatch("Update/{id}")]
        public async Task<IActionResult> UpdateCinemaRoom(Guid id, [FromBody] CinemaRoomUpdateDto dto)
        {
            _logger.LogInformation("Update CinemaRoom with ID: {Id}", id);
            return await _cinemaRoomService.UpdateCinemaRoom(id, dto);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteCinemaRoom(Guid id)
        {
            _logger.LogInformation("Delete CinemaRoom with ID: {Id}", id);
            return await _cinemaRoomService.DeleteCinemaRoom(id);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCinemaRoom([FromQuery] string? keyword)
        {
            _logger.LogInformation("Search CinemaRoom");
            return await _cinemaRoomService.SearchCinemaRoom(keyword);
        }

        //admin
        // Đặt rồi - True
        [HttpGet("rooms-True/{roomId}/seats")]
        public async Task<IActionResult> ViewSeatsTrue(Guid roomId)
        {
            _logger.LogInformation("View Seats");
            return await _cinemaRoomService.ViewSeatTrue(roomId);
        }

        // Chưa đặt - False
        [HttpGet("rooms-False/{roomId}/seats")]
        public async Task<IActionResult> ViewSeatsFalse(Guid roomId)
        {
            _logger.LogInformation("View Seats");
            return await _cinemaRoomService.ViewSeatFalse(roomId);
        }

        //admin
        [HttpPut("rooms/seats/update")]
        public async Task<IActionResult> UpdateSeats([FromBody] UpdateSeatTypesRequest request)
        {
            _logger.LogInformation("Update seats");
            return await _cinemaRoomService.UpdateSeatTypes(request);
        }

        [HttpPost("migration/add-layout-columns")]
        public async Task<IActionResult> AddLayoutColumns()
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection("Host=localhost;Port=5432;Database=OJT_SUMMER25_Group2_Movie;Username=postgres;Password=12345");
                await connection.OpenAsync();
                
                var sql = @"
                    ALTER TABLE tbl_cinema_rooms 
                    ADD COLUMN IF NOT EXISTS ""NumberOfRows"" integer NOT NULL DEFAULT 0;
                    
                    ALTER TABLE tbl_cinema_rooms 
                    ADD COLUMN IF NOT EXISTS ""NumberOfColumns"" integer NOT NULL DEFAULT 0;
                    
                    ALTER TABLE tbl_cinema_rooms 
                    ADD COLUMN IF NOT EXISTS ""DefaultSeatPrice"" numeric NOT NULL DEFAULT 100000;
                    
                    UPDATE tbl_cinema_rooms 
                    SET ""NumberOfRows"" = 10, ""NumberOfColumns"" = 10, ""DefaultSeatPrice"" = 100000 
                    WHERE ""NumberOfRows"" = 0 OR ""NumberOfColumns"" = 0;
                ";
                
                using var command = new Npgsql.NpgsqlCommand(sql, connection);
                var result = await command.ExecuteNonQueryAsync();
                
                return Ok(new { success = true, message = "Layout columns added successfully", rowsAffected = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


    }
}
