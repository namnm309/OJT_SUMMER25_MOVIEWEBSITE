using DomainLayer.Entities;

namespace InfrastructureLayer.Repository
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<Booking?> GetBookingWithDetailsAsync(Guid bookingId, Guid userId);
    }
}
