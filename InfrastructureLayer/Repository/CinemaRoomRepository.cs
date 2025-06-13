using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repository
{
    public class CinemaRoomRepository : ICinemaRoomRepository
    {
        private readonly MovieContext _context;

        public CinemaRoomRepository(MovieContext context)
        {
            _context = context;
        }

        // 1. Get all cinema rooms
        public async Task<List<CinemaRoom>> GetAllCinemaRoomsAsync()
        {
            return await _context.CinemaRooms
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        // 2. Add a new cinema room
        public async Task AddCinemaRoomAsync(CinemaRoom cinemaRoom)
        {
            await _context.CinemaRooms.AddAsync(cinemaRoom);
            await _context.SaveChangesAsync();
        }

        // 3. Search cinema rooms by keyword
        public async Task<List<CinemaRoom>> SearchCinemaRoomsAsync(string keyword)
        {
            return await _context.CinemaRooms
                .AsNoTracking()
                .Where(cr =>
                    cr.RoomName.Contains(keyword) ||
                    cr.RoomId.ToString().Contains(keyword))
                .Take(28)
                .ToListAsync();
        }

        // 4. Get seats for a specific cinema room
        public async Task<List<Seat>> GetSeatsByCinemaRoomIdAsync(Guid cinemaRoomId)
        {
            return await _context.Seats
                .AsNoTracking()
                .Where(s => s.RoomId == cinemaRoomId && s.IsActive)
                .ToListAsync();
        }

        // 5. Update a seat’s type (e.g., "Normal" or "VIP")
        public async Task UpdateSeatTypeAsync(Guid seatId, string seatType)
        {
            if (string.IsNullOrWhiteSpace(seatType))
                throw new ArgumentException("seatType cannot be null or empty", nameof(seatType));

            var seat = await _context.Seats.FindAsync(seatId);
            if (seat == null) return;

            // parse string → enum
            if (!Enum.TryParse(seatType, out SeatType parsed))
                throw new ArgumentException($"Invalid SeatType: {seatType}");

            seat.SeatType = parsed;
            await _context.SaveChangesAsync();
        }

        // 6. Toggle between Normal ↔ VIP
        public async Task ToggleSeatSelectionAsync(Guid seatId)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            if (seat == null) return;

            seat.SeatType = seat.SeatType == SeatType.Normal
                ? SeatType.VIP
                : SeatType.Normal;

            await _context.SaveChangesAsync();
        }
    }
}
