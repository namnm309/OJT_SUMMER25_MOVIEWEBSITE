using ApplicationLayer.DTO.CinemaRoomManagement;

namespace ApplicationLayer.Services.CinemaRoomManagement
{
    public interface ICinemaRoomService
    {
        // Get all cinema rooms (mapped to DTO)
        Task<List<CinemaRoomDto>> GetAllCinemaRoomsAsync();

        // Add a new cinema room (accept DTO)
        Task AddCinemaRoomAsync(CinemaRoomDto cinemaRoom);

        // Search cinema rooms by keyword (mapped to DTO)
        Task<List<CinemaRoomDto>> SearchCinemaRoomsAsync(string keyword);
    }
}
