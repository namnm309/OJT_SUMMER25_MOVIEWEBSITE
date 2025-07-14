using Application.ResponseCode;
using ApplicationLayer.DTO;
using ApplicationLayer.DTO.CinemaRoomManagement;
using AutoMapper;
using DomainLayer.Entities;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.CinemaRoomManagement
{
    public interface ICinemaRoomService
    {
        public Task<IActionResult> GetAllCinemaRoom();
        public Task<IActionResult> GetAllCinemaRoomPagination(PaginationReq query);
        public Task<IActionResult> GetSeatDetail(Guid Id);
        public Task<IActionResult> AddCinemaRoom(CinemaRoomCreateDto Dto);
        public Task<IActionResult> UpdateCinemaRoom(Guid id, CinemaRoomUpdateDto dto);
        public Task<IActionResult> DeleteCinemaRoom(Guid id);
        public Task<IActionResult> SearchCinemaRoom(string? keyword);
        public Task<IActionResult> ViewSeatTrue(Guid roomId);
        public Task<IActionResult> ViewSeatFalse(Guid roomId);
        public Task<IActionResult> UpdateSeatTypes(UpdateSeatTypesRequest request);

    }
    
    public class CinemaRoomService : ICinemaRoomService
    {
        private readonly IGenericRepository<CinemaRoom> _cinemaRoomRepo;
        private readonly IGenericRepository<Seat> _seatRepo;
        private readonly IMapper _mapper;

        public CinemaRoomService(IGenericRepository<CinemaRoom> cinemaRoomRepo, IGenericRepository<Seat> seatRepo, IMapper mapper)
        {
            _cinemaRoomRepo = cinemaRoomRepo;
            _seatRepo = seatRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> GetAllCinemaRoom()
        {
            var room = await _cinemaRoomRepo.ListAsync();

            var result = _mapper.Map<List<CinemaRoomListDto>>(room);

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetAllCinemaRoomPagination(PaginationReq query)
        {
            int page = query.Page <= 0 ? 1 : query.Page;
            int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var allRooms = await _cinemaRoomRepo.ListAsync();
            int totalItems = allRooms.Count;

            var pagedRooms = allRooms
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .ToList();

            var result = _mapper.Map<List<CinemaRoomListDto>>(pagedRooms);

            var response = new
            {
                Data = result,
                Total = totalItems,
                Page = page,
                PageSize = pageSize,
            };

            return SuccessResp.Ok(response);
        }

        public async Task<IActionResult> GetSeatDetail(Guid Id)
        {
            var room = await _cinemaRoomRepo.FindByIdAsync(Id, "Seats");
            if (room == null)
                return ErrorResp.NotFound("Not Found");

            var seats = await _seatRepo.WhereAsync(s => s.RoomId == Id);

            var result = _mapper.Map<List<SeatDetailDto>>(room.Seats);

            return SuccessResp.Ok(new
            {
                RoomId = room.Id,
                RoomName = room.RoomName,
                TotalSeats = room.TotalSeats,
                Seats = result
            });
        }

        public async Task<IActionResult> AddCinemaRoom(CinemaRoomCreateDto Dto)
        {
            var exist = await _cinemaRoomRepo.FirstOrDefaultAsync(r => r.RoomName.Trim().ToLower() == Dto.RoomName.Trim().ToLower());

            if (exist != null)
                return ErrorResp.BadRequest("Cinema room with this name already exists");

            // Validate layout
            var calculatedSeats = Dto.NumberOfRows * Dto.NumberOfColumns;
            if (calculatedSeats != Dto.TotalSeats)
            {
                return ErrorResp.BadRequest($"Số ghế không khớp: {Dto.NumberOfRows} hàng x {Dto.NumberOfColumns} ghế = {calculatedSeats}, nhưng TotalSeats = {Dto.TotalSeats}");
            }

            var room = _mapper.Map<CinemaRoom>(Dto);
            await _cinemaRoomRepo.CreateAsync(room);

            // Tự động tạo ghế theo layout
            await CreateSeatsForRoom(room.Id, Dto.NumberOfRows, Dto.NumberOfColumns, Dto.DefaultSeatPrice);

            return SuccessResp.Ok("Add CinemaRoom and create seats successfully");
        }

        private async Task CreateSeatsForRoom(Guid roomId, int numberOfRows, int numberOfColumns, decimal defaultPrice)
        {
            var seats = new List<Seat>();

            for (int row = 1; row <= numberOfRows; row++)
            {
                // Chuyển số hàng thành chữ cái (1=A, 2=B, ...)
                char rowLetter = (char)('A' + row - 1);
                
                for (int col = 1; col <= numberOfColumns; col++)
                {
                    var seat = new Seat
                    {
                        RoomId = roomId,
                        SeatCode = $"{rowLetter}{col:D2}", // A01, A02, B01, B02, ...
                        RowIndex = row,
                        ColumnIndex = col,
                        SeatType = DomainLayer.Enum.SeatType.Normal, // Mặc định là ghế thường
                        PriceSeat = defaultPrice,
                        Status = DomainLayer.Enum.SeatStatus.Available,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    seats.Add(seat);
                }
            }

            // Tạo tất cả ghế cùng lúc
            foreach (var seat in seats)
            {
                await _seatRepo.CreateAsync(seat);
            }
        }

        public async Task<IActionResult> SearchCinemaRoom(string? keyword)
        {
            var room = string.IsNullOrWhiteSpace(keyword)
                ? await _cinemaRoomRepo.ListAsync()
                : await _cinemaRoomRepo.WhereAsync(r => r.RoomName.ToLower().Contains(keyword.ToLower()));

            var result = _mapper.Map<List<CinemaRoomListDto>>(room);

            return SuccessResp.Ok(result);
        }

        // Ghế đặt rồi (true)
        public async Task<IActionResult> ViewSeatTrue(Guid roomId)
        {
            var room = await _cinemaRoomRepo.FindByIdAsync(roomId);
            if (room == null)
                return ErrorResp.NotFound("Cinema room not found");

            var seat = await _seatRepo.WhereAsync(s => s.RoomId == roomId && s.Status == DomainLayer.Enum.SeatStatus.Available);

            var result = _mapper.Map<List<SeatViewDto>>(seat);

            return SuccessResp.Ok(new
            {
                RoomName = room.RoomName,
                Seat = result
            });
        }

        // Ghế đặt rồi (False)
        public async Task<IActionResult> ViewSeatFalse(Guid roomId)
        {
            var room = await _cinemaRoomRepo.FindByIdAsync(roomId);
            if (room == null)
                return ErrorResp.NotFound("Cinema room not found");

            var seat = await _seatRepo.WhereAsync(s => s.RoomId == roomId && s.Status == DomainLayer.Enum.SeatStatus.Selected);

            var result = _mapper.Map<List<SeatViewDto>>(seat);

            return SuccessResp.Ok(new
            {
                RoomName = room.RoomName,
                Seat = result
            });
        }

        public async Task<IActionResult> UpdateCinemaRoom(Guid id, CinemaRoomUpdateDto dto)
        {
            var room = await _cinemaRoomRepo.FindByIdAsync(id);
            if (room == null)
                return ErrorResp.NotFound("Cinema room not found");

            // Check if room name already exists (exclude current room)
            if (!string.IsNullOrWhiteSpace(dto.RoomName))
            {
                var existingRoom = await _cinemaRoomRepo.FirstOrDefaultAsync(r => 
                    r.RoomName.Trim().ToLower() == dto.RoomName.Trim().ToLower() && r.Id != id);
                
                if (existingRoom != null)
                    return ErrorResp.BadRequest("Cinema room with this name already exists");
                
                room.RoomName = dto.RoomName.Trim();
            }

            // Handle seat regeneration
            if (dto.RegenerateSeats && dto.NumberOfRows.HasValue && dto.NumberOfColumns.HasValue)
            {
                // Validate layout
                var calculatedSeats = dto.NumberOfRows.Value * dto.NumberOfColumns.Value;
                if (dto.TotalSeats.HasValue && calculatedSeats != dto.TotalSeats.Value)
                {
                    return ErrorResp.BadRequest($"Số ghế không khớp: {dto.NumberOfRows} hàng x {dto.NumberOfColumns} ghế = {calculatedSeats}, nhưng TotalSeats = {dto.TotalSeats}");
                }

                // Check if room has any active bookings
                var existingSeats = await _seatRepo.WhereAsync(s => s.RoomId == id, "BookingDetails.Booking");
                var hasActiveBookings = existingSeats.Any(s => s.BookingDetails.Any(bd => 
                    bd.Booking.Status == DomainLayer.Enum.BookingStatus.Confirmed || 
                    bd.Booking.Status == DomainLayer.Enum.BookingStatus.Pending));

                if (hasActiveBookings)
                {
                    return ErrorResp.BadRequest("Cannot regenerate seats for room with active bookings");
                }

                // Delete old seats
                foreach (var seat in existingSeats)
                {
                    await _seatRepo.DeleteAsync(seat);
                }

                // Update total seats
                room.TotalSeats = calculatedSeats;

                // Create new seats
                await CreateSeatsForRoom(id, dto.NumberOfRows.Value, dto.NumberOfColumns.Value, dto.DefaultSeatPrice ?? 100000);
            }
            else
            {
                // Update fields only if provided
                if (dto.TotalSeats.HasValue)
                    room.TotalSeats = dto.TotalSeats.Value;
            }

            if (dto.IsActive.HasValue)
                room.IsActive = dto.IsActive.Value;

            room.UpdatedAt = DateTime.UtcNow;
            await _cinemaRoomRepo.UpdateAsync(room);

            return SuccessResp.Ok("Cinema room updated successfully");
        }

        public async Task<IActionResult> DeleteCinemaRoom(Guid id)
        {
            var room = await _cinemaRoomRepo.FindByIdAsync(id, "ShowTimes,Seats");
            if (room == null)
                return ErrorResp.NotFound("Cinema room not found");

            // Check if room has active showtimes
            if (room.ShowTimes != null && room.ShowTimes.Any())
                return ErrorResp.BadRequest("Cannot delete cinema room with existing showtimes");

            // Soft delete related seats first
            if (room.Seats != null && room.Seats.Any())
            {
                foreach (var seat in room.Seats)
                {
                    await _seatRepo.DeleteAsync(seat);
                }
            }

            // Delete the room
            await _cinemaRoomRepo.DeleteAsync(room);

            return SuccessResp.Ok("Cinema room deleted successfully");
        }

        public async Task<IActionResult> UpdateSeatTypes(UpdateSeatTypesRequest request)
        {
            var room = await _cinemaRoomRepo.FindByIdAsync(request.RoomId);
            if (room == null)
                return ErrorResp.NotFound("Cinema room not found");

            foreach (var update in request.Updates)
            {
                var seat = await _seatRepo.FindByIdAsync(update.SeatId);
                if (seat == null || seat.RoomId != request.RoomId)
                    continue;

                seat.SeatType = update.NewSeatType;
                seat.UpdatedAt = DateTime.UtcNow;

                await _seatRepo.UpdateAsync(seat);
            }

            return SuccessResp.Ok("Seat types updated successfully");
        }
    }
}
