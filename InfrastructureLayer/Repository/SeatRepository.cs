using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Entities;
using DomainLayer.Enum;
using DomainLayer.Exceptions;
using InfrastructureLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repository
{
    // tôi là PhucAn,
    // tôi đã tạo và code file này đừng gộp code của tôi với code của người khác nhé
    // vì tôi sẽ không hiểu và không đọc được đâu (I'am a beginner)
    public class SeatRepository : ISeatRepository
    {
        private readonly MovieContext _context;

        public SeatRepository(MovieContext context)
        {
            _context = context;
        }

        public async Task<(string RoomName, List<Seat> Seats, List<Guid> BookedSeatIds)> GetSeatInfoAsync(Guid showTimeId)
        {
            var showTime = await _context.ShowTimes
                .Include(st => st.Room)
                .FirstOrDefaultAsync(st => st.Id == showTimeId);

            if (showTime?.Room == null)
                throw new NotFoundException("ShowTime or its Room not found", showTimeId.ToString());

            var seats = await _context.Seats
                .Where(s => s.RoomId == showTime.RoomId && s.Status != SeatStatus.Available)
                .OrderBy(s => s.RowIndex)
                .ThenBy(s => s.ColumnIndex)
                .ToListAsync();

            var bookedSeatIds = await _context.BookingDetails
                .Where(bd => bd.Booking.ShowTimeId == showTimeId)
                .Select(bd => bd.SeatId)
                .ToListAsync();

            return (showTime.Room.RoomName, seats, bookedSeatIds);
        }

        public async Task<bool> ValidateSeatsAsync(Guid showTimeId, List<Guid> seatIds)
        {
            // Kiểm tra tất cả ghế có thuộc cùng 1 phòng với suất chiếu không
            var showTime = await _context.ShowTimes
                .Include(st => st.Room)
                .FirstOrDefaultAsync(st => st.Id == showTimeId);

            if (showTime == null) return false;

            var validSeatsCount = await _context.Seats
                .CountAsync(s => s.RoomId == showTime.RoomId &&
                                 s.Status == SeatStatus.Available && // Đảm bảo ghế còn active
                                 seatIds.Contains(s.Id));

            return validSeatsCount == seatIds.Count;
        }

        public async Task<Seat?> GetByIdAsync(Guid seatId)
        {
            // Thay đổi logic: chỉ cần tìm ghế theo ID, không cần kiểm tra IsActive ở đây
            // Việc kiểm tra IsActive sẽ được thực hiện ở tầng Service hoặc tại thời điểm đặt vé
            return await _context.Seats.FirstOrDefaultAsync(s => s.Id == seatId);
        }

        // Trong SeatRepository
        public async Task UpdateSeatAsync(Seat seat)
        {
            _context.Seats.Update(seat);
            await _context.SaveChangesAsync();
        }

        // THÊM PHƯƠNG THỨC NÀY: Update nhiều ghế cùng lúc
        public async Task UpdateSeatsAsync(IEnumerable<Seat> seats)
        {
            _context.Seats.UpdateRange(seats);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Seat>> GetSeatsByRoomIdAsync(Guid roomId)
        {
            return await _context.Seats
                .Where(s => s.RoomId == roomId)
                .OrderBy(s => s.RowIndex)
                .ThenBy(s => s.ColumnIndex)
                .ToListAsync();
        }

        public async Task<List<Guid>> GetBookedSeatIdsForShowTimeAsync(Guid showTimeId)
        {
            return await _context.BookingDetails
                .Where(bd => bd.Booking.ShowTimeId == showTimeId)
                .Select(bd => bd.SeatId)
                .ToListAsync();
        }

        // THÊM PHƯƠNG THỨC NÀY: Lấy danh sách ghế theo nhiều ID
        public async Task<List<Seat>> GetSeatsByIdsAsync(List<Guid> seatIds)
        {
            return await _context.Seats
                .Where(s => seatIds.Contains(s.Id))
                .ToListAsync();
        }
    }
}