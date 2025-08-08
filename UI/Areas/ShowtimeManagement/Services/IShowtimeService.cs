using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI.Areas.ShowtimeManagement.Models;

namespace UI.Areas.ShowtimeManagement.Services
{
    public interface IShowtimeService
    {
        Task<List<ShowtimeDto>> GetShowtimesForWeekAsync(DateTime startDate);
        Task<List<ShowtimeDto>> GetShowtimesForMonthAsync(int month, int year);
        Task<ShowtimeDto> GetShowtimeByIdAsync(Guid id);
        Task<List<MovieDto>> GetActiveMoviesAsync();
        Task<List<CinemaRoomDto>> GetCinemaRoomsAsync();
        Task<object> CreateShowtimeAsync(CreateShowtimeViewModel model);
        Task<object> UpdateShowtimeAsync(EditShowtimeViewModel model);
        Task<object> DeleteShowtimeAsync(Guid id);
        Task<object> CheckScheduleConflictAsync(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, int duration, Guid? excludeId = null, Guid? movieId = null);
    }
} 