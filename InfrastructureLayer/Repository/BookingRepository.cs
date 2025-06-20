using DomainLayer.Entities;
using InfrastructureLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace InfrastructureLayer.Repository
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(MovieContext context) : base(context)
        { }
        public async Task<Booking?> GetBookingWithDetailsAsync(Guid bookingId, Guid userId)
        {
            return await dbSet
                .Include(b => b.ShowTime)
                    .ThenInclude(st => st.Movie)
                .Include( b => b.ShowTime)
                    .ThenInclude(st => st.Room)
                .Include(b => b.User)
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.Seat)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
        }
    }
}
