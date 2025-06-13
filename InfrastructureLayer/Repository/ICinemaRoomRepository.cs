using DomainLayer.Entities;


namespace InfrastructureLayer.Repository
{
    public interface ICinemaRoomRepository
    {
        Task<List<CinemaRoom>> GetAllCinemaRoomsAsync();
        Task AddCinemaRoomAsync(CinemaRoom cinemaRoom);
        Task<List<CinemaRoom>> SearchCinemaRoomsAsync(string keyword);
        Task<List<Seat>> GetSeatsByCinemaRoomIdAsync(Guid cinemaRoomId);
        Task UpdateSeatTypeAsync(Guid seatId, string seatType);
        Task ToggleSeatSelectionAsync(Guid seatId);
    }
}
