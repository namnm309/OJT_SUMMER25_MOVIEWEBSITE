using UI.Models;
using UI.Areas.BookingManagement.Models;
using UI.Services;

namespace UI.Areas.BookingManagement.Services
{
    public interface IBookingManagementUIService
    {
        // T7: Search Movies
        Task<ApiResponse<dynamic>> GetMoviesAsync();
        Task<ApiResponse<dynamic>> SearchMoviesAsync(string searchTerm);

        // T8: Select Movie and Showtime  
        Task<ApiResponse<List<MovieDropdownDto>>> GetMoviesDropdownAsync();
        Task<ApiResponse<List<DateTime>>> GetShowDatesAsync(Guid movieId); // Modified to return List<DateTime>
        Task<ApiResponse<List<ShowtimeDropdownDto>>> GetShowTimesAsync(Guid movieId, DateTime showDate); // Modified to return List<ShowtimeDropdownDto>

        // T9: Select Seats
        Task<ApiResponse<SeatSelectionViewModel>> GetAvailableSeatsAsync(Guid showTimeId);
        Task<ApiResponse<SeatValidationResponse>> ValidateSeatsAsync(Guid showTimeId, List<Guid> seatIds);

        // T10: Confirm Booking
        Task<ApiResponse<dynamic>> ConfirmBookingAsync(BookingConfirmViewModel model);
        
        // T11: Ticket Information
        Task<ApiResponse<dynamic>> GetBookingDetailAsync(Guid bookingId);
    }

    public class BookingManagementUIService : IBookingManagementUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<BookingManagementUIService> _logger;

        public BookingManagementUIService(IApiService apiService, ILogger<BookingManagementUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<ApiResponse<dynamic>> GetMoviesAsync()
        {
            try
            {
                _logger.LogInformation("Getting available movies for booking");
                return await _apiService.GetAsync<dynamic>("movies");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies list");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải danh sách phim. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> SearchMoviesAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching movies with term: {SearchTerm}", searchTerm);
                return await _apiService.GetAsync<dynamic>($"movies/search?term={Uri.EscapeDataString(searchTerm)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tìm kiếm phim. Vui lòng thử lại."
                };
            }
        }

        //T8: Select Movie and Showtime ------------------------------------------------------------
        public async Task<ApiResponse<List<MovieDropdownDto>>> GetMoviesDropdownAsync() // Renamed for clarity
        {
            try
            {
                _logger.LogInformation("Getting available movies for booking dropdown");
                // Specify the full path as per your API structure (e.g., /api/v1/booking-ticket/dropdown/movies)
                var result = await _apiService.GetAsync<ApiResponseData<List<MovieDropdownDto>>>("/api/v1/booking-ticket/dropdown/movies");

                if (result.Success && result.Data != null)
                {
                    return new ApiResponse<List<MovieDropdownDto>> { Success = true, Data = result.Data.Data, Message = result.Data.Message };
                }
                return new ApiResponse<List<MovieDropdownDto>> { Success = false, Message = result.Message ?? "Failed to retrieve movies." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies list for dropdown");
                return new ApiResponse<List<MovieDropdownDto>>
                {
                    Success = false,
                    Message = "Không thể tải danh sách phim. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<List<DateTime>>> GetShowDatesAsync(Guid movieId)
        {
            try
            {
                _logger.LogInformation("Getting show dates for movie: {MovieId}", movieId);
                var result = await _apiService.GetAsync<ApiResponseData<List<DateTime>>>($"/api/v1/booking-ticket/dropdown/movies/{movieId}/dates");

                if (result.Success && result.Data != null)
                {
                    return new ApiResponse<List<DateTime>> { Success = true, Data = result.Data.Data, Message = result.Data.Message };
                }
                return new ApiResponse<List<DateTime>> { Success = false, Message = result.Message ?? "Failed to retrieve show dates." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting show dates");
                return new ApiResponse<List<DateTime>>
                {
                    Success = false,
                    Message = "Không thể tải lịch chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<List<ShowtimeDropdownDto>>> GetShowTimesAsync(Guid movieId, DateTime showDate)
        {
            try
            {
                _logger.LogInformation("Getting show times for movie: {MovieId} on {ShowDate}", movieId, showDate);
                // Ensure date format matches API expectation (ISO 8601, typically "yyyy-MM-ddTHH:mm:ssZ" for UTC or "yyyy-MM-dd" for date only)
                // Based on your API, it expects "2023-04-30T00:00:00Z" for the 'date' query parameter.
                var formattedDate = showDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

                var result = await _apiService.GetAsync<ApiResponseData<List<ShowtimeDropdownDto>>>($"/api/v1/booking-ticket/dropdown/movies/{movieId}/times?date={Uri.EscapeDataString(formattedDate)}");

                if (result.Success && result.Data != null)
                {
                    return new ApiResponse<List<ShowtimeDropdownDto>> { Success = true, Data = result.Data.Data, Message = result.Data.Message };
                }
                return new ApiResponse<List<ShowtimeDropdownDto>> { Success = false, Message = result.Message ?? "Failed to retrieve showtimes." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting show times");
                return new ApiResponse<List<ShowtimeDropdownDto>>
                {
                    Success = false,
                    Message = "Không thể tải suất chiếu. Vui lòng thử lại."
                };
            }
        }
        // T9 ----------------------------------------------------------------------------------------------------- 

        public async Task<ApiResponse<SeatSelectionViewModel>> GetAvailableSeatsAsync(Guid showTimeId)
        {
            try
            {
                _logger.LogInformation("Getting available seats for showtime: {ShowTimeId}", showTimeId);

                // Get seats
                var seatsResult = await _apiService.GetAsync<ApiResponseData<SeatSelectionViewModel>>(
                    $"/api/v1/booking-ticket/available?showTimeId={showTimeId}");

                // Get showtime details
                var detailsResult = await _apiService.GetAsync<ApiResponseData<ShowtimeDetailsDto>>(
                    $"/api/v1/booking-ticket/{showTimeId}/details");

                if (seatsResult.Success && seatsResult.Data != null &&
                    detailsResult.Success && detailsResult.Data != null)
                {
                    var model = seatsResult.Data.Data;
                    model.ShowtimeId = showTimeId;
                    model.ShowDate = detailsResult.Data.Data.ShowDate;

                    // Lấy thông tin chi tiết phim
                    var movieResult = await _apiService.GetAsync<ApiResponseData<MovieDetailDto>>(
                        $"/api/v1/movie/GetById?movieId={detailsResult.Data.Data.MovieId}");

                    if (movieResult.Success && movieResult.Data != null)
                    {
                        var movie = movieResult.Data.Data;
                        model.MovieTitle = movie.Title;
                        model.MovieDescription = movie.Content;
                        model.MovieDirector = movie.Director;
                        model.MovieActors = movie.Actors;
                        model.MovieRunningTime = movie.RunningTime;
                        model.MoviePrimaryImageUrl = movie.PrimaryImageUrl;
                        model.MovieGenres = movie.Genres?.Select(g => g.Name).ToList();
                    }

                    return new ApiResponse<SeatSelectionViewModel>
                    {
                        Success = true,
                        Data = model,
                        Message = seatsResult.Data.Message
                    };
                }

                return new ApiResponse<SeatSelectionViewModel>
                {
                    Success = false,
                    Message = seatsResult.Message ?? "Failed to retrieve seat information."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available seats");
                return new ApiResponse<SeatSelectionViewModel>
                {
                    Success = false,
                    Message = "Không thể tải thông tin ghế. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<SeatValidationResponse>> ValidateSeatsAsync(Guid showTimeId, List<Guid> seatIds)
        {
            try
            {
                _logger.LogInformation("Validating {SeatCount} seats for showtime: {ShowTimeId}", seatIds.Count, showTimeId);

                var result = await _apiService.PostAsync<ApiResponseData<SeatValidationResponse>>(
                    $"/api/v1/booking-ticket/validate?showTimeId={showTimeId}",
                    seatIds);

                if (result.Success && result.Data != null)
                {
                    return new ApiResponse<SeatValidationResponse>
                    {
                        Success = true,
                        Data = result.Data.Data,
                        Message = result.Data.Message
                    };
                }

                return new ApiResponse<SeatValidationResponse>
                {
                    Success = false,
                    Message = result.Message ?? "Failed to validate seats."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating seats");
                return new ApiResponse<SeatValidationResponse>
                {
                    Success = false,
                    Message = "Không thể xác thực ghế. Vui lòng thử lại."
                };
            }
        }





        public async Task<ApiResponse<dynamic>> ConfirmBookingAsync(BookingConfirmViewModel model)
        {
            try
            {
                _logger.LogInformation("Confirming booking for user: {UserId}", model.UserId);
                return await _apiService.PostAsync<dynamic>("booking/confirm", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể xác nhận đặt vé. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetBookingDetailAsync(Guid bookingId)
        {
            try
            {
                _logger.LogInformation("Getting booking detail: {BookingId}", bookingId);
                return await _apiService.GetAsync<dynamic>($"booking/{bookingId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking detail");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải thông tin đặt vé. Vui lòng thử lại."
                };
            }
        }

       
    }
} 