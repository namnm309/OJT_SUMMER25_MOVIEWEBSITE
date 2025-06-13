using UI.Models;
using UI.Services;

namespace UI.Areas.BookingManagement.Services
{
    public interface IBookingManagementUIService
    {
        // T7: Search Movies
        Task<ApiResponse<dynamic>> GetMoviesAsync();
        Task<ApiResponse<dynamic>> SearchMoviesAsync(string searchTerm);
        
        // T8: Select Movie and Showtime  
        Task<ApiResponse<dynamic>> GetShowDatesAsync(Guid movieId);
        Task<ApiResponse<dynamic>> GetShowTimesAsync(Guid movieId, DateTime showDate);
        
        // T9: Select Seats
        Task<ApiResponse<dynamic>> GetSeatsAsync(Guid showTimeId);
        Task<ApiResponse<dynamic>> SelectSeatsAsync(Guid showTimeId, List<Guid> seatIds);
        
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

        public async Task<ApiResponse<dynamic>> GetShowDatesAsync(Guid movieId)
        {
            try
            {
                _logger.LogInformation("Getting show dates for movie: {MovieId}", movieId);
                return await _apiService.GetAsync<dynamic>($"movies/{movieId}/showdates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting show dates");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải lịch chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetShowTimesAsync(Guid movieId, DateTime showDate)
        {
            try
            {
                _logger.LogInformation("Getting show times for movie: {MovieId} on {ShowDate}", movieId, showDate);
                return await _apiService.GetAsync<dynamic>($"movies/{movieId}/showtimes?date={showDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting show times");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải suất chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetSeatsAsync(Guid showTimeId)
        {
            try
            {
                _logger.LogInformation("Getting seats for showtime: {ShowTimeId}", showTimeId);
                return await _apiService.GetAsync<dynamic>($"showtimes/{showTimeId}/seats");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seats");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải sơ đồ ghế. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> SelectSeatsAsync(Guid showTimeId, List<Guid> seatIds)
        {
            try
            {
                _logger.LogInformation("Selecting {SeatCount} seats for showtime: {ShowTimeId}", seatIds.Count, showTimeId);
                var request = new { ShowTimeId = showTimeId, SeatIds = seatIds };
                return await _apiService.PostAsync<dynamic>("booking/select-seats", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting seats");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể chọn ghế. Vui lòng thử lại."
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