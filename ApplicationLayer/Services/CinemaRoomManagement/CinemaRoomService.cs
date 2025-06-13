using ApplicationLayer.DTO.CinemaRoomManagement;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;

namespace ApplicationLayer.Services.CinemaRoomManagement
{
    public class CinemaRoomService : ICinemaRoomService
    {
        private readonly ICinemaRoomRepository _cinemaRoomRepository;

        public CinemaRoomService(ICinemaRoomRepository cinemaRoomRepository)
        {
            _cinemaRoomRepository = cinemaRoomRepository;
        }

        // Cinema Room Management

        // Get all cinema rooms
        public async Task<List<CinemaRoomDto>> GetAllCinemaRoomsAsync()
        {
            var rooms = await _cinemaRoomRepository.GetAllCinemaRoomsAsync();
            return rooms
                .Select(r => new CinemaRoomDto
                {
                    RoomId = r.RoomId,
                    RoomName = r.RoomName,
                    TotalSeats = r.TotalSeats
                })
                .ToList();
        }

        //Add a new cinema room
        public async Task AddCinemaRoomAsync(CinemaRoomDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var entity = new CinemaRoom
            {
                RoomId = dto.RoomId == Guid.Empty
                            ? Guid.NewGuid() : dto.RoomId,
                RoomName = dto.RoomName,
                TotalSeats = dto.TotalSeats

            };

            await _cinemaRoomRepository.AddCinemaRoomAsync(entity);
        }

        //Search cinema rooms by keyword
        public async Task<List<CinemaRoomDto>> SearchCinemaRoomsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length > 28)
                throw new ArgumentException("Keyword must be 1-28 characters", nameof(keyword));

            var rooms = await _cinemaRoomRepository.SearchCinemaRoomsAsync(keyword);
            return rooms
                .Select(r => new CinemaRoomDto
                {
                    RoomId = r.RoomId,
                    RoomName = r.RoomName,
                    TotalSeats = r.TotalSeats
                })
                .ToList();
        }

        // Seats Management

        //wouldnt you like to know, weather boy

    }
}
