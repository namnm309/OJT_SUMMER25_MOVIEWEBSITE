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
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Services.ShowtimeManagement
{
    public interface IShowtimeService
    {
        Task<IActionResult> GetAllShowtimes(int page = 1, int pageSize = 20);
        Task<IActionResult> GetShowtimesByMonth(int month, int year);
        Task<IActionResult> GetShowtimeById(Guid id);
        Task<IActionResult> CreateShowtime(ShowtimeCreateDto dto);
        Task<IActionResult> CreateNewShowtime(ShowtimeCreateNewDto dto);
        Task<IActionResult> UpdateShowtime(Guid id, ShowtimeUpdateDto dto);
        Task<IActionResult> DeleteShowtime(Guid id);
        Task<IActionResult> CheckScheduleConflict(  Guid cinemaRoomId, 
                                                    DateTime showDate,
                                                    TimeSpan startTime,
                                                    TimeSpan endTime,
                                                    Guid? excludeId = null,
                                                    Guid? movieId = null);
        Task<bool> HasScheduleConflict(     Guid cinemaRoomId,
                                            DateTime showDate,
                                            TimeSpan startTime,
                                            TimeSpan endTime,
                                            Guid? excludeId = null,
                                            Guid? movieId = null);
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
        private readonly ILogger<ShowtimeService> _logger;

        // Convert a local date (date portion only) to UTC midnight for storage
        private static DateTime ToUtcDate(DateTime localDate)
        {
            // Ensure only the date part is used and mark it as Local, then convert to UTC
            var localMidnight = DateTime.SpecifyKind(localDate.Date, DateTimeKind.Local);
            return localMidnight.ToUniversalTime();
        }

        public ShowtimeService(
            IGenericRepository<ShowTime> showtimeRepo,
            IGenericRepository<Movie> movieRepo,
            IGenericRepository<CinemaRoom> cinemaRoomRepo,
            IGenericRepository<Booking> bookingRepo,
            IMapper mapper,
            ILogger<ShowtimeService> logger)
        {
            _showtimeRepo = showtimeRepo;
            _movieRepo = movieRepo;
            _cinemaRoomRepo = cinemaRoomRepo;
            _bookingRepo = bookingRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> GetAllShowtimes(int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = await _showtimeRepo.ListAsync("Movie", "Movie.MovieImages", "Room");
            // Order by ShowDate desc, StartTime
            var ordered = query.OrderByDescending(s => s.ShowDate).ThenBy(s => s.StartTime).ToList();

            var totalItems = ordered.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var paged = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var items = _mapper.Map<List<ShowtimeListDto>>(paged);

            var respData = new {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };

            return SuccessResp.Ok(respData);
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
                    s.ShowDate.Value.ToLocalTime().Month == month && 
                    s.ShowDate.Value.ToLocalTime().Year == year).ToList();

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

            // Normalize show date (store as UTC midnight)
            var normalizedShowDate = DateTime.SpecifyKind(dto.ShowDate.Date, DateTimeKind.Utc);

            // Check for schedule conflicts (BACKEND CHẶN TRÙNG)
            var hasConflict = await HasScheduleConflict(dto.CinemaRoomId, normalizedShowDate, dto.StartTime, endTime, null, dto.MovieId);
            if (hasConflict)
            {
                return ErrorResp.BadRequest("Lịch chiếu bị trùng! Không thể tạo mới.");
            }

            var showtime = _mapper.Map<ShowTime>(dto);
            showtime.EndTime = endTime;
            showtime.RoomId = dto.CinemaRoomId;
            showtime.ShowDate = normalizedShowDate;

            await _showtimeRepo.CreateAsync(showtime);
            return SuccessResp.Ok("Showtime created successfully");
        }

        public async Task<IActionResult> UpdateShowtime(Guid id, ShowtimeUpdateDto dto)
        {
            var showtime = await _showtimeRepo.FindByIdAsync(id, "Bookings");
            if (showtime == null)
                return ErrorResp.NotFound("Không tìm thấy lịch chiếu");

            // Kiểm tra xem lịch chiếu đã có booking chưa
            if (showtime.Bookings != null && showtime.Bookings.Any())
            {
                return ErrorResp.BadRequest("Không thể chỉnh sửa lịch chiếu đã có vé được đặt. Vui lòng xóa lịch chiếu và tạo mới.");
            }

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

            // Calculate end time based on movie duration
            var endTime = dto.StartTime.Add(TimeSpan.FromMinutes(movie.RunningTime));

            //// Validate start time is before end time
            //if (dto.StartTime >= endTime)
            //    return ErrorResp.BadRequest("Giờ bắt đầu phải nhỏ hơn giờ kết thúc");

            // Normalize show date (store as UTC midnight)
            var normalizedShowDate = DateTime.SpecifyKind(dto.ShowDate.Date, DateTimeKind.Utc);

            // Check for schedule conflicts (exclude current showtime)
            var hasConflict = await HasScheduleConflict(dto.CinemaRoomId, normalizedShowDate, dto.StartTime, endTime, id);
            if (hasConflict)
            {
                return ErrorResp.BadRequest("Đã có lịch chiếu khác trong khoảng thời gian này tại phòng chiếu này.");
            }

            // Update showtime
            showtime.MovieId = dto.MovieId;
            showtime.RoomId = dto.CinemaRoomId;
            showtime.ShowDate = normalizedShowDate;
            showtime.StartTime = dto.StartTime;
            showtime.EndTime = endTime;
            showtime.Price = dto.Price;
            
            if (dto.IsActive.HasValue)
                showtime.IsActive = dto.IsActive.Value;

            showtime.UpdatedAt = DateTime.UtcNow;
            await _showtimeRepo.UpdateAsync(showtime);

            return SuccessResp.Ok("Cập nhật lịch chiếu thành công");
        }

        public async Task<IActionResult> DeleteShowtime(Guid id)
        {
            var showtime = await _showtimeRepo.FindByIdAsync(id, "Bookings");
            if (showtime == null)
                return ErrorResp.NotFound("Showtime not found");

            // Check if there are any bookings for this showtime
            if (showtime.Bookings != null && showtime.Bookings.Any())
            {
                // Xóa tất cả bookings của showtime này (cascade delete sẽ tự động xóa các bảng con)
                await _bookingRepo.DeleteRangeAsync(showtime.Bookings);
            }

            await _showtimeRepo.DeleteAsync(showtime);
            return SuccessResp.Ok("Showtime deleted successfully");
        }

        public async Task<IActionResult> CheckScheduleConflict(
            Guid cinemaRoomId,
            DateTime showDate,
            TimeSpan startTime,
            TimeSpan endTime,
            Guid? excludeId = null,
            Guid? movieId = null)
{
    var hasConflict = await HasScheduleConflict(cinemaRoomId, showDate, startTime, endTime, excludeId, movieId);
    return SuccessResp.Ok(!hasConflict);
}

        public async Task<bool> HasScheduleConflict(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, TimeSpan endTime, Guid? excludeId = null, Guid? movieId = null)
        {
            try
            {
                _logger.LogInformation($"[CONFLICT CHECK] INPUT: showDate={showDate:yyyy-MM-dd}, startTime={startTime}, endTime={endTime}, movieId={movieId}, roomId={cinemaRoomId}");
                var existingShowtimes = await _showtimeRepo.WhereAsync(s => 
                    s.RoomId == cinemaRoomId && s.IsActive);

                var inputDateStr = showDate.ToString("yyyy-MM-dd");
                foreach (var existing in existingShowtimes)
                {
                    if (!existing.ShowDate.HasValue) continue;
                    var dbDateStr = existing.ShowDate.Value.ToString("yyyy-MM-dd");
                    _logger.LogInformation($"[CHECKTRUNGLICHHHHHHHHHHHHHHHHHHHHHHH] DB: showDate={dbDateStr}, startTime={existing.StartTime}, endTime={existing.EndTime}, movieId={existing.MovieId}, roomId={existing.RoomId}, existingId={existing.Id}");
                    if (dbDateStr != inputDateStr) continue;

                    _logger.LogInformation($"[CHECKTRUNGLICHHHHHHHHHHHHHHHHHHHHHHH] So sánh: input (movieId={movieId}, startTime={startTime}) với DB (movieId={existing.MovieId}, startTime={existing.StartTime}, endTime={existing.EndTime})");
                    // 1. Không cho phép giờ bắt đầu trùng với giờ bắt đầu của cùng 1 bộ phim, cùng phòng, cùng ngày
                    if (movieId.HasValue && existing.MovieId == movieId.Value && existing.StartTime == startTime)
                    {
                        _logger.LogWarning($"[CHECKTRUNGLICHHHHHHHHHHHHHHHHHHHHHHH] Trùng giờ bắt đầu cùng phim: movieId={movieId}, startTime={startTime}");
                        return true;
                    }
                    // 2. Không cho phép giờ bắt đầu trùng với giờ kết thúc của bất kỳ lịch chiếu nào khác cùng phòng, cùng ngày
                    if (existing.EndTime == startTime)
                    {
                        _logger.LogWarning($"[CHECKTRUNGLICHHHHHHHHHHHHHHHHHHHHHHH] Trùng giờ bắt đầu với giờ kết thúc phim khác: startTime={startTime}, endTime={existing.EndTime}");
                        return true;
                    }
                }
                _logger.LogInformation($"[CHECKTRUNGLICHHHHHHHHHHHHHHHHHHHHHHH] Không phát hiện conflict cho movieId={movieId}, startTime={startTime}, endTime={endTime}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CHECKTRUNGLICHHHHHHHHHHHHHHHHHHHHHHH] Lỗi khi kiểm tra conflict");
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
            //if (dto.StartTime >= dto.EndTime)
            //    return ErrorResp.BadRequest("Giờ bắt đầu phải nhỏ hơn giờ kết thúc");

            var showDateUtc = DateTime.SpecifyKind(dto.ShowDate.Date, DateTimeKind.Utc);

            // Check for schedule conflicts (BACKEND CHẶN TRÙNG)
            var hasConflict = await HasScheduleConflict(dto.CinemaRoomId, showDateUtc, dto.StartTime, dto.EndTime, null, dto.MovieId);
            if (hasConflict)
            {
                return ErrorResp.BadRequest("Lịch chiếu bị trùng! Không thể tạo mới.");
            }

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
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
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