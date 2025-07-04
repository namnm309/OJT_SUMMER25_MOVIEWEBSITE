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
                var endDate = startDate.AddDays(6);
                var result = await _apiService.GetAsync<JsonElement>($"/api/v1/showtime/GetByDateRange?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataElement))
                {
                    var showtimes = JsonSerializer.Deserialize<List<ShowtimeDto>>(dataElement.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return showtimes ?? new List<ShowtimeDto>();
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            // Return mock data for now
            return GetMockShowtimes(startDate);
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
                // Log error
            }
            
            return null;
        }

        public async Task<List<MovieDto>> GetActiveMoviesAsync()
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/movie/GetActive");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataElement))
                {
                    var movies = JsonSerializer.Deserialize<List<MovieDto>>(dataElement.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return movies ?? new List<MovieDto>();
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            // Return mock data
            return new List<MovieDto>
            {
                new MovieDto { Id = Guid.NewGuid(), Title = "Oppenheimer", RunningTime = 180, Status = "NowShowing" },
                new MovieDto { Id = Guid.NewGuid(), Title = "Barbie", RunningTime = 114, Status = "NowShowing" },
                new MovieDto { Id = Guid.NewGuid(), Title = "The Nun II", RunningTime = 110, Status = "NowShowing" }
            };
        }

        public async Task<List<CinemaRoomDto>> GetCinemaRoomsAsync()
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/cinemaroom");
                
                if (result.Success && result.Data.TryGetProperty("data", out var dataElement))
                {
                    var rooms = JsonSerializer.Deserialize<List<CinemaRoomDto>>(dataElement.GetRawText(), 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return rooms ?? new List<CinemaRoomDto>();
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            // Return mock data
            return new List<CinemaRoomDto>
            {
                new CinemaRoomDto { Id = Guid.NewGuid(), Name = "Phòng 1", TotalSeats = 100, RoomType = "Standard" },
                new CinemaRoomDto { Id = Guid.NewGuid(), Name = "Phòng 2", TotalSeats = 120, RoomType = "VIP" },
                new CinemaRoomDto { Id = Guid.NewGuid(), Name = "Phòng 3", TotalSeats = 80, RoomType = "Standard" },
                new CinemaRoomDto { Id = Guid.NewGuid(), Name = "Phòng IMAX", TotalSeats = 200, RoomType = "IMAX" }
            };
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

                var result = await _apiService.PostAsync<JsonElement>("/api/v1/showtime", data);
                
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

        public async Task<bool> CheckScheduleConflictAsync(Guid cinemaRoomId, DateTime showDate, TimeSpan startTime, int duration, Guid? excludeId = null)
        {
            try
            {
                var endTime = startTime.Add(TimeSpan.FromMinutes(duration));
                var url = $"/api/v1/showtime/CheckConflict?cinemaRoomId={cinemaRoomId}&showDate={showDate:yyyy-MM-dd}&startTime={startTime:hh\\:mm}&endTime={endTime:hh\\:mm}";
                
                if (excludeId.HasValue)
                {
                    url += $"&excludeId={excludeId}";
                }

                var result = await _apiService.GetAsync<JsonElement>(url);
                
                if (result.Success && result.Data.TryGetProperty("hasConflict", out var hasConflictElement))
                {
                    return hasConflictElement.GetBoolean();
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            
            return false;
        }

        private List<ShowtimeDto> GetMockShowtimes(DateTime startDate)
        {
            var showtimes = new List<ShowtimeDto>();
            var movies = GetActiveMoviesAsync().Result;
            var rooms = GetCinemaRoomsAsync().Result;
            var random = new Random();

            for (int day = 0; day < 7; day++)
            {
                var date = startDate.AddDays(day);
                
                foreach (var room in rooms)
                {
                    var showtimesPerDay = random.Next(3, 6);
                    var currentTime = new TimeSpan(9, 0, 0); // Start at 9 AM

                    for (int i = 0; i < showtimesPerDay; i++)
                    {
                        var movie = movies[random.Next(movies.Count)];
                        var showtime = new ShowtimeDto
                        {
                            Id = Guid.NewGuid(),
                            MovieId = movie.Id,
                            MovieTitle = movie.Title,
                            MovieDuration = movie.RunningTime,
                            CinemaRoomId = room.Id,
                            CinemaRoomName = room.Name,
                            TotalSeats = room.TotalSeats,
                            BookedSeats = random.Next(0, room.TotalSeats),
                            ShowDate = date,
                            StartTime = currentTime,
                            EndTime = currentTime.Add(TimeSpan.FromMinutes(movie.RunningTime + 15)), // Add 15 min for cleaning
                            Price = room.RoomType == "VIP" ? 150000 : (room.RoomType == "IMAX" ? 200000 : 100000),
                            Status = "Active",
                            IsActive = true
                        };

                        showtimes.Add(showtime);
                        currentTime = showtime.EndTime.Add(TimeSpan.FromMinutes(30)); // 30 min break between shows
                        
                        if (currentTime > new TimeSpan(22, 0, 0)) // Stop after 10 PM
                            break;
                    }
                }
            }

            return showtimes.OrderBy(s => s.ShowDate).ThenBy(s => s.StartTime).ToList();
        }
    }
} 