using Application.ResponseCode;
using ApplicationLayer.DTO;
using ApplicationLayer.DTO.ShowtimeManagement;
using AutoMapper;
using DomainLayer.Entities;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.ShowtimeManagement
{
    public interface IShowtimeService
    {
        Task<IActionResult> GetAllShowtimes();
        Task<IActionResult> GetShowtimesByDateRange(DateTime startDate, DateTime endDate);
        Task<IActionResult> GetShowtimeById(Guid id);
        Task<IActionResult> CreateShowtime(ShowtimeCreateDto dto);
        Task<IActionResult> UpdateShowtime(Guid id, ShowtimeUpdateDto dto);
        Task<IActionResult> DeleteShowtime(Guid id);
        Task<IActionResult> CheckScheduleConflict(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, TimeSpan endTime, Guid? excludeId = null);
        Task<bool> HasScheduleConflict(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, TimeSpan endTime, Guid? excludeId = null);
        Task<IActionResult> GetShowtimesByMovie(Guid movieId);
        Task<IActionResult> GetShowtimesByRoom(Guid roomId);
    }

    public class ShowtimeService : IShowtimeService
    {
        private readonly IGenericRepository<ShowTime> _showtimeRepo;
        private readonly IGenericRepository<Movie> _movieRepo;
        private readonly IGenericRepository<CinemaRoom> _cinemaRoomRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IMapper _mapper;

        public ShowtimeService(
            IGenericRepository<ShowTime> showtimeRepo,
            IGenericRepository<Movie> movieRepo,
            IGenericRepository<CinemaRoom> cinemaRoomRepo,
            IGenericRepository<Booking> bookingRepo,
            IMapper mapper)
        {
            _showtimeRepo = showtimeRepo;
            _movieRepo = movieRepo;
            _cinemaRoomRepo = cinemaRoomRepo;
            _bookingRepo = bookingRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> GetAllShowtimes()
        {
            var showtimes = await _showtimeRepo.ListAsync("Movie,Room");
            var result = _mapper.Map<List<ShowtimeListDto>>(showtimes);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetShowtimesByDateRange(DateTime startDate, DateTime endDate)
        {
            var showtimes = await _showtimeRepo.WhereAsync(
                s => s.ShowDate >= startDate && s.ShowDate <= endDate,
                "Movie,Room");
            
            var result = _mapper.Map<List<ShowtimeListDto>>(showtimes);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetShowtimeById(Guid id)
        {
            var showtime = await _showtimeRepo.FindByIdAsync(id, "Movie,Room");
            if (showtime == null)
                return ErrorResp.NotFound("Showtime not found");

            var result = _mapper.Map<ShowtimeListDto>(showtime);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> CreateShowtime(ShowtimeCreateDto dto)
        {
            // Validate movie exists
            var movie = await _movieRepo.FindByIdAsync(dto.MovieId);
            if (movie == null)
                return ErrorResp.NotFound("Movie not found");

            // Validate cinema room exists
            var room = await _cinemaRoomRepo.FindByIdAsync(dto.CinemaRoomId);
            if (room == null)
                return ErrorResp.NotFound("Cinema room not found");

            // Calculate end time based on movie duration
            var endTime = dto.StartTime.Add(TimeSpan.FromMinutes(movie.RunningTime));

            // Check for schedule conflicts
            var hasConflict = await HasScheduleConflict(dto.CinemaRoomId, dto.ShowDate, dto.StartTime, endTime);
            if (hasConflict)
            {
                return ErrorResp.BadRequest("Schedule conflict detected. There is already a showtime at this time in this room.");
            }

            var showtime = _mapper.Map<ShowTime>(dto);
            showtime.EndTime = endTime;
            showtime.RoomId = dto.CinemaRoomId;

            await _showtimeRepo.CreateAsync(showtime);
            return SuccessResp.Ok("Showtime created successfully");
        }

        public async Task<IActionResult> UpdateShowtime(Guid id, ShowtimeUpdateDto dto)
        {
            var showtime = await _showtimeRepo.FindByIdAsync(id);
            if (showtime == null)
                return ErrorResp.NotFound("Showtime not found");

            // Validate movie exists
            var movie = await _movieRepo.FindByIdAsync(dto.MovieId);
            if (movie == null)
                return ErrorResp.NotFound("Movie not found");

            // Validate cinema room exists
            var room = await _cinemaRoomRepo.FindByIdAsync(dto.CinemaRoomId);
            if (room == null)
                return ErrorResp.NotFound("Cinema room not found");

            // Calculate end time based on movie duration
            var endTime = dto.StartTime.Add(TimeSpan.FromMinutes(movie.RunningTime));

            // Check for schedule conflicts (exclude current showtime)
            var hasConflict = await HasScheduleConflict(dto.CinemaRoomId, dto.ShowDate, dto.StartTime, endTime, id);
            if (hasConflict)
            {
                return ErrorResp.BadRequest("Schedule conflict detected. There is already a showtime at this time in this room.");
            }

            // Update showtime
            showtime.MovieId = dto.MovieId;
            showtime.RoomId = dto.CinemaRoomId;
            showtime.ShowDate = dto.ShowDate;
            showtime.StartTime = dto.StartTime;
            showtime.EndTime = endTime;
            showtime.Price = dto.Price;
            
            if (dto.IsActive.HasValue)
                showtime.IsActive = dto.IsActive.Value;

            showtime.UpdatedAt = DateTime.UtcNow;
            await _showtimeRepo.UpdateAsync(showtime);

            return SuccessResp.Ok("Showtime updated successfully");
        }

        public async Task<IActionResult> DeleteShowtime(Guid id)
        {
            var showtime = await _showtimeRepo.FindByIdAsync(id, "Bookings");
            if (showtime == null)
                return ErrorResp.NotFound("Showtime not found");

            // Check if there are any bookings for this showtime
            if (showtime.Bookings != null && showtime.Bookings.Any())
                return ErrorResp.BadRequest("Cannot delete showtime with existing bookings");

            await _showtimeRepo.DeleteAsync(showtime);
            return SuccessResp.Ok("Showtime deleted successfully");
        }

        public async Task<IActionResult> CheckScheduleConflict(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, TimeSpan endTime, Guid? excludeId = null)
        {
            var hasConflict = await HasScheduleConflict(cinemaRoomId, showDate, startTime, endTime, excludeId);
            return SuccessResp.Ok(!hasConflict); // Return opposite of hasConflict
        }

        public async Task<bool> HasScheduleConflict(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, TimeSpan endTime, Guid? excludeId = null)
        {
            var existingShowtimes = (await _showtimeRepo.WhereAsync(s => 
                s.RoomId == cinemaRoomId && 
                s.ShowDate.Value.Date == showDate.Date &&
                s.IsActive)).ToList();

            if (excludeId.HasValue)
            {
                existingShowtimes = existingShowtimes.Where(s => s.Id != excludeId.Value).ToList();
            }

            foreach (var existing in existingShowtimes)
            {
                // Check if there's an overlap
                if ((startTime < existing.EndTime && endTime > existing.StartTime))
                {
                    return true; // Conflict found
                }
            }

            return false; // No conflict
        }

        public async Task<IActionResult> GetShowtimesByMovie(Guid movieId)
        {
            var movie = await _movieRepo.FindByIdAsync(movieId);
            if (movie == null)
                return ErrorResp.NotFound("Movie not found");

            var showtimes = await _showtimeRepo.WhereAsync(s => s.MovieId == movieId, "Movie,Room");
            var result = _mapper.Map<List<ShowtimeListDto>>(showtimes);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetShowtimesByRoom(Guid roomId)
        {
            var room = await _cinemaRoomRepo.FindByIdAsync(roomId);
            if (room == null)
                return ErrorResp.NotFound("Cinema room not found");

            var showtimes = await _showtimeRepo.WhereAsync(s => s.RoomId == roomId, "Movie,Room");
            var result = _mapper.Map<List<ShowtimeListDto>>(showtimes);
            return SuccessResp.Ok(result);
        }
    }
} 