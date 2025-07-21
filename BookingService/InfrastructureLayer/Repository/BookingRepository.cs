using DomainLayer.Entities;
using InfrastructureLayer.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayer.Repository
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(MovieContext context) : base(context)
        {
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(Guid bookingId, Guid userId)
        {
            return await dbSet
                .Include(b => b.User)
                .Include(b => b.ShowTime)
                    .ThenInclude(st => st.Movie)
                .Include(b => b.ShowTime)
                    .ThenInclude(st => st.Room)
                .Include(b => b.BookingDetails)
                    .ThenInclude(bd => bd.Seat)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
        }
    }
}
