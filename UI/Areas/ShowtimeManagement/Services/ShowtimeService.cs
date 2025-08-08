using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using UI.Areas.ShowtimeManagement.Models;
using UI.Services;

namespace UI.Areas.ShowtimeManagement.Services
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly IApiService _apiService;

        public ShowtimeService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<ShowtimeDto>> GetShowtimesForWeekAsync(DateTime startDate)
        {
            try
            {

                var result = await _apiService.GetAsync<JsonElement>($"/api/v1/showtime");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataElement))
                {
                    var allShowtimes = JsonSerializer.Deserialize<List<ShowtimeDto>>(dataElement.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (allShowtimes != null)
                    {

                        var endDate = startDate.AddDays(6);
                        return allShowtimes.Where(s => s.ShowDate.Date >= startDate.Date && s.ShowDate.Date <= endDate.Date).ToList();
                    }
                }
                
                return new List<ShowtimeDto>();
            }
            catch (Exception ex)
            {
                // Lỗi khi tải dữ liệu từ API
                throw new Exception($"Lỗi tải lịch chiếu: {ex.Message}");
            }
        }

        public async Task<List<ShowtimeDto>> GetShowtimesForMonthAsync(int month, int year)
        {
            try
            {

                var result = await _apiService.GetAsync<JsonElement>($"/api/v1/showtime/GetByMonth?month={month}&year={year}");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataElement))
                {
                    var showtimes = JsonSerializer.Deserialize<List<ShowtimeDto>>(dataElement.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return showtimes ?? new List<ShowtimeDto>();
                }
                
                return new List<ShowtimeDto>();
            }
            catch (Exception ex)
            {
                // Lỗi khi tải dữ liệu từ API
                throw new Exception($"Lỗi tải lịch chiếu tháng {month}/{year}: {ex.Message}");
            }
        }

        public async Task<ShowtimeDto> GetShowtimeByIdAsync(Guid id)
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>($"/api/v1/showtime/{id}");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataElement))
                {
                    return JsonSerializer.Deserialize<ShowtimeDto>(dataElement.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch (Exception ex)
            {

            }
            
            return null;
        }

        public async Task<List<MovieDto>> GetActiveMoviesAsync()
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/booking-ticket/dropdown/movies");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataElement))
                {
                    var movies = JsonSerializer.Deserialize<List<MovieDto>>(dataElement.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return movies ?? new List<MovieDto>();
                }
                
                // API trả về thành công nhưng không có dữ liệu
                throw new Exception("Không có phim nào đang chiếu");
            }
            catch (Exception ex)
            {
                // Lỗi khi tải dữ liệu từ API
                throw new Exception($"Lỗi tải danh sách phim: {ex.Message}");
            }
        }

        public async Task<List<CinemaRoomDto>> GetCinemaRoomsAsync()
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/cinemaroom/ViewRoom");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataElement))
                {
                    var rooms = JsonSerializer.Deserialize<List<CinemaRoomDto>>(dataElement.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return rooms ?? new List<CinemaRoomDto>();
                }
                
                // API trả về thành công nhưng không có dữ liệu
                throw new Exception("Không có phòng chiếu nào");
            }
            catch (Exception ex)
            {
                // Lỗi khi tải dữ liệu từ API
                throw new Exception($"Lỗi tải danh sách phòng chiếu: {ex.Message}");
            }
        }

        public async Task<object> CreateShowtimeAsync(CreateShowtimeViewModel model)
        {
            try
            {
                var data = new
                {
                    movieId = model.MovieId,
                    cinemaRoomId = model.CinemaRoomId,
                    showDate = model.ShowDate,
                    startTime = model.StartTime.ToString(@"hh\:mm"),
                    price = model.Price
                };

                var result = await _apiService.PostAsync<JsonElement>("/showtime/create-new", data);
                
                if (result.Success)
                {
                    return new { success = true, message = "Tạo lịch chiếu thành công" };
                }
                
                return new { success = false, message = result.Message ?? "Có lỗi xảy ra" };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> UpdateShowtimeAsync(EditShowtimeViewModel model)
        {
            try
            {
                var data = new
                {
                    id = model.Id,
                    movieId = model.MovieId,
                    cinemaRoomId = model.CinemaRoomId,
                    showDate = model.ShowDate,
                    startTime = model.StartTime.ToString(@"hh\:mm"),
                    price = model.Price
                };

                var result = await _apiService.PutAsync<JsonElement>($"/api/v1/showtime/{model.Id}", data);
                
                if (result.Success)
                {
                    return new { success = true, message = "Cập nhật lịch chiếu thành công" };
                }
                
                return new { success = false, message = result.Message ?? "Có lỗi xảy ra" };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> DeleteShowtimeAsync(Guid id)
        {
            try
            {
                var result = await _apiService.DeleteAsync($"/api/v1/showtime/{id}");
                
                if (result.Success)
                {
                    return new { success = true, message = "Xóa lịch chiếu thành công" };
                }
                
                return new { success = false, message = result.Message ?? "Có lỗi xảy ra" };
            }
            catch (Exception ex)
            {
                return new { success = false, message = "Có lỗi xảy ra: " + ex.Message };
            }
        }

        public async Task<object> CheckScheduleConflictAsync(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, int duration, Guid? excludeId = null, Guid? movieId = null)
        {
            try
            {
                // Calculate end time based on movie duration + 15 minutes for cleaning
                var endTime = startTime.Add(TimeSpan.FromMinutes(duration + 15));
                var movieIdParam = movieId.HasValue ? movieId.Value.ToString() : Guid.Empty.ToString();
                var url = $"/api/v1/showtime/CheckConflict?cinemaRoomId={cinemaRoomId}&showDate={showDate:yyyy-MM-dd}&startTime={startTime:hh\\:mm}&endTime={endTime:hh\\:mm}&movieId={movieIdParam}";
                
                if (excludeId.HasValue)
                {
                    url += $"&excludeId={excludeId}";
                }

                var result = await _apiService.GetAsync<JsonElement>(url);
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataElement))
                {
                    return new
                    {
                        hasConflict = dataElement.TryGetProperty("hasConflict", out var hasConflictElement) ? hasConflictElement.GetBoolean() : false,
                        conflictMessage = dataElement.TryGetProperty("conflictMessage", out var messageElement) ? messageElement.GetString() : string.Empty,
                        conflictingShowtimes = dataElement.TryGetProperty("conflictingShowtimes", out var showtimesElement) ? showtimesElement.ToString() : string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                // Silent error handling
            }
            
            return new { hasConflict = false, conflictMessage = string.Empty, conflictingShowtimes = string.Empty };
        }
    }
}