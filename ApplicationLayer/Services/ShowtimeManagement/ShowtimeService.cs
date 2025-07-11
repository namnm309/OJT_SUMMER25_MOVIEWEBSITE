using Application.ResponseCode;
using ApplicationLayer.DTO;
using ApplicationLayer.DTO.ShowtimeManagement;
using ApplicationLayer.DTO.BookingTicketManagement;
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
        Task<IActionResult> GetShowtimesByMonth(int month, int year);
        Task<IActionResult> GetShowtimeById(Guid id);
        Task<IActionResult> CreateShowtime(ShowtimeCreateDto dto);
        Task<IActionResult> CreateNewShowtime(ShowtimeCreateNewDto dto);
        Task<IActionResult> UpdateShowtime(Guid id, ShowtimeUpdateDto dto);
        Task<IActionResult> DeleteShowtime(Guid id);
        Task<IActionResult> CheckScheduleConflict(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, TimeSpan endTime, Guid? excludeId = null);
        Task<bool> HasScheduleConflict(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, TimeSpan endTime, Guid? excludeId = null);
        Task<IActionResult> GetShowtimesByMovie(Guid movieId);
        Task<IActionResult> GetShowtimesByRoom(Guid roomId);
        Task<IActionResult> GetMoviesForDropdown();
        Task<IActionResult> GetCinemaRoomsForDropdown();
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
            var showtimes = await _showtimeRepo.ListAsync("Movie", "Movie.MovieImages", "Room");
            var result = _mapper.Map<List<ShowtimeListDto>>(showtimes);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetShowtimesByMonth(int month, int year)
        {
            // Validate input parameters
            if (month < 1 || month > 12)
                return ErrorResp.BadRequest("Th�ng ph?i t? 1 ??n 12");
            
            if (year < 2020 || year > 2030)
                return ErrorResp.BadRequest("N?m ph?i t? 2020 ??n 2030");

            try
            {
                // Get all active showtimes and filter in memory to avoid PostgreSQL datetime issues
                // Include Movie.MovieImages to get poster images
                var allShowtimes = await _showtimeRepo.WhereAsync(
                    s => s.ShowDate.HasValue && s.IsActive,
                    "Movie", "Movie.MovieImages", "Room");

                // Filter by month and year in memory
                var filteredShowtimes = allShowtimes.Where(s => 
                    s.ShowDate.HasValue &&
                    s.ShowDate.Value.Month == month && 
                    s.ShowDate.Value.Year == year).ToList();

                var result = _mapper.Map<List<ShowtimeListDto>>(filteredShowtimes);
                return SuccessResp.Ok(result);
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                return ErrorResp.InternalServerError($"L?i khi l?y d? li?u l?ch chi?u: {ex.Message}");
            }
        }

        public async Task<IActionResult> GetShowtimeById(Guid id)
        {
            var showtime = await _showtimeRepo.FindByIdAsync(id, "Movie", "Movie.MovieImages", "Room");
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
            try
            {
                // Get showtimes for the room and filter in memory to avoid PostgreSQL datetime issues
                var existingShowtimes = await _showtimeRepo.WhereAsync(s => 
                    s.RoomId == cinemaRoomId && s.IsActive);

                // Filter by date in memory
                var sameDayShowtimes = existingShowtimes.Where(s => 
                    s.ShowDate.HasValue && 
                    s.ShowDate.Value.Date == showDate.Date).ToList();

                if (excludeId.HasValue)
                {
                    sameDayShowtimes = sameDayShowtimes.Where(s => s.Id != excludeId.Value).ToList();
                }

                foreach (var existing in sameDayShowtimes)
                {
                    // Check if there's an overlap
                    if ((startTime < existing.EndTime && endTime > existing.StartTime))
                    {
                        return true; // Conflict found
                    }
                }

                return false; // No conflict
            }
            catch (Exception)
            {
                // In case of error, assume no conflict to allow operation to continue
                return false;
            }
        }

        public async Task<IActionResult> GetShowtimesByMovie(Guid movieId)
        {
            var movie = await _movieRepo.FindByIdAsync(movieId);
            if (movie == null)
                return ErrorResp.NotFound("Movie not found");

            var showtimes = await _showtimeRepo.WhereAsync(s => s.MovieId == movieId, "Movie", "Movie.MovieImages", "Room");
            var result = _mapper.Map<List<ShowtimeListDto>>(showtimes);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetShowtimesByRoom(Guid roomId)
        {
            var room = await _cinemaRoomRepo.FindByIdAsync(roomId);
            if (room == null)
                return ErrorResp.NotFound("Cinema room not found");

            var showtimes = await _showtimeRepo.WhereAsync(s => s.RoomId == roomId, "Movie", "Movie.MovieImages", "Room");
            var result = _mapper.Map<List<ShowtimeListDto>>(showtimes);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> CreateNewShowtime(ShowtimeCreateNewDto dto)
        {
            // Validate movie exists
            var movie = await _movieRepo.FindByIdAsync(dto.MovieId);
            if (movie == null)
                return ErrorResp.NotFound("Phim không tồn tại");

            // Validate cinema room exists
            var room = await _cinemaRoomRepo.FindByIdAsync(dto.CinemaRoomId);
            if (room == null)
                return ErrorResp.NotFound("Phòng chiếu không tồn tại");

            // Validate show date is not in the past
            if (dto.ShowDate.Date < DateTime.Today)
                return ErrorResp.BadRequest("Ngày chiếu không thể là ngày trong quá khứ");

            // Validate start time is before end time
            if (dto.StartTime >= dto.EndTime)
                return ErrorResp.BadRequest("Giờ bắt đầu phải nhỏ hơn giờ kết thúc");

            // Check for schedule conflicts
            var hasConflict = await HasScheduleConflict(dto.CinemaRoomId, dto.ShowDate, dto.StartTime, dto.EndTime);
            if (hasConflict)
            {
                return ErrorResp.BadRequest("Đã có lịch chiếu khác trong khoảng thời gian này tại phòng chiếu này.");
            }

            // Chuyển ShowDate sang UTC để tránh lỗi Npgsql "Cannot write DateTime with Kind=Unspecified" 
            var showDateUtc = dto.ShowDate.Kind == DateTimeKind.Utc 
                ? dto.ShowDate 
                : DateTime.SpecifyKind(dto.ShowDate, DateTimeKind.Utc);

            var showtime = new ShowTime
            {
                Id = Guid.NewGuid(),
                MovieId = dto.MovieId,
                RoomId = dto.CinemaRoomId,
                ShowDate = showDateUtc,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Price = dto.Price,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _showtimeRepo.CreateAsync(showtime);
            return SuccessResp.Ok("Tạo lịch chiếu mới thành công");
        }

        public async Task<IActionResult> GetMoviesForDropdown()
        {
            var movies = await _movieRepo.WhereAsync(m => m.Status == DomainLayer.Enum.MovieStatus.NowShowing || m.Status == DomainLayer.Enum.MovieStatus.ComingSoon, "MovieImages", "MovieGenres", "MovieGenres.Genre");
            var result = movies.Select(m => new MovieDropdownDto
            {
                Id = m.Id,
                Title = m.Title,
                PrimaryImageUrl = m.MovieImages?.FirstOrDefault()?.ImageUrl,
                Genre = m.MovieGenres?.Any() == true ? string.Join(", ", m.MovieGenres.Select(mg => mg.Genre.GenreName)) : null,
                Duration = m.RunningTime
            }).ToList();
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetCinemaRoomsForDropdown()
        {
            var rooms = await _cinemaRoomRepo.WhereAsync(r => r.IsActive);
            var result = rooms.Select(r => new CinemaRoomDropdownDto
            {
                Id = r.Id,
                Name = r.RoomName,
                TotalSeats = r.TotalSeats,
                IsActive = r.IsActive
            }).ToList();
            return SuccessResp.Ok(result);
        }
    }
}