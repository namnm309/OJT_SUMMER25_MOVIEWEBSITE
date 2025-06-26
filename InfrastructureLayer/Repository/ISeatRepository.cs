using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Entities;

namespace InfrastructureLayer.Repository
{
    public interface ISeatRepository
    {
        Task<(string RoomName, List<Seat> Seats, List<Guid> BookedSeatIds)> GetSeatInfoAsync(Guid showTimeId);
        Task<bool> ValidateSeatsAsync(Guid showTimeId, List<Guid> seatIds);

        Task<Seat?> GetByIdAsync(Guid seatId);

        Task UpdateSeatAsync(Seat seat);
    }
}
