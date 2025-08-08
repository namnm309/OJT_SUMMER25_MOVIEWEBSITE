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
        // ValidateSeatsAsync không cần sửa, nhưng logic gọi nó trong SeatService đã thay đổi
        Task<bool> ValidateSeatsAsync(Guid showTimeId, List<Guid> seatIds); // Giữ lại nếu có chỗ khác dùng, hoặc xem xét bỏ nếu không cần thiết sau khi sửa ValidateSelectedSeats

        Task<Seat?> GetByIdAsync(Guid seatId);

        Task UpdateSeatAsync(Seat seat);
        Task UpdateSeatsAsync(IEnumerable<Seat> seats); // THÊM PHƯƠNG THỨC NÀY

        Task<List<Seat>> GetSeatsByRoomIdAsync(Guid roomId);
        Task<List<Guid>> GetBookedSeatIdsForShowTimeAsync(Guid showTimeId);
        Task<List<Seat>> GetSeatsByIdsAsync(List<Guid> seatIds); // THÊM PHƯƠNG THỨC NÀY
        Task<List<Guid>> GetPendingSeatIdsForShowTimeAsync(Guid showTimeId); // THÊM PHƯƠNG THỨC NÀY
    }
}