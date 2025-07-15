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

        // Thêm method bị thiếu cho dropdown
        Task<ApiResponse<dynamic>> GetMoviesDropdownAsync();

        // T8: Select Movie and Showtime  
        Task<ApiResponse<dynamic>> GetShowDatesAsync(Guid movieId);
        Task<ApiResponse<dynamic>> GetShowTimesAsync(Guid movieId, DateTime showDate);

        // T9: Select Seats
        Task<ApiResponse<dynamic>> GetSeatsAsync(Guid showTimeId);
        Task<ApiResponse<dynamic>> SelectSeatsAsync(Guid showTimeId, List<Guid> seatIds);

        // Thêm các method bị thiếu cho seat management
        Task<ApiResponse<dynamic>> GetAvailableSeatsAsync(Guid showtimeId);
        Task<ApiResponse<dynamic>> ValidateSeatsAsync(Guid showtimeId, List<Guid> seatIds);

        // T10: Confirm Booking
        Task<ApiResponse<dynamic>> ConfirmBookingAsync(BookingConfirmViewModel model);

        // T11: Ticket Information
        Task<ApiResponse<dynamic>> GetBookingDetailAsync(Guid bookingId);

        // Search Customer
        Task<ApiResponse<CustomerSearchViewModel>> SearchCustomerAsync(string searchTerm);

        // Create Customer
        Task<ApiResponse<dynamic>> CreateCustomerAsync(CreateCustomerViewModel model);

        // Confirm Admin Booking
        Task<ApiResponse<dynamic>> ConfirmAdminBookingAsync(ConfirmAdminBookingViewModel model);

        // New methods for score conversion booking
        Task<ApiResponse<BookingConfirmationDetailViewModel>> GetBookingConfirmationDetailAsync(Guid showTimeId, List<Guid> seatIds, string memberId);
        Task<ApiResponse<BookingConfirmSuccessViewModel>> ConfirmBookingWithScoreAsync(BookingConfirmWithScoreViewModel model);

        // Booking list management methods
        Task<ApiResponse<dynamic>> GetBookingListAsync(dynamic filter);
        Task<ApiResponse<dynamic>> UpdateBookingStatusAsync(Guid bookingId, string newStatus);
        Task<ApiResponse<dynamic>> CancelBookingAsync(Guid bookingId, string reason);
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
                return await _apiService.GetAsync<dynamic>("api/v1/booking-ticket/dropdown/movies");
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

        // Thêm method này
        public async Task<ApiResponse<dynamic>> GetMoviesDropdownAsync()
        {
            try
            {
                _logger.LogInformation("Getting all movies from /api/v1/movie/View");
                return await _apiService.GetAsync<dynamic>("api/v1/movie/View");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies");
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
                return await _apiService.GetAsync<dynamic>($"api/v1/movie/Search?keyword={Uri.EscapeDataString(searchTerm)}");
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
                return await _apiService.GetAsync<dynamic>($"api/v1/booking-ticket/dropdown/movies/{movieId}/dates");
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
                _logger.LogInformation("Getting show times for movie: {MovieId} on {ShowDate} from /api/v1/booking-ticket/times", movieId, showDate);
                // Format date as yyyy-MM-dd for the API
                var dateParam = showDate.ToString("yyyy-MM-dd");
                return await _apiService.GetAsync<dynamic>($"api/v1/booking-ticket/dropdown/movies/{movieId}/times?date={Uri.EscapeDataString(dateParam)}");
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
                return await _apiService.GetAsync<dynamic>($"api/v1/booking-ticket/available?showTimeId={showTimeId}");
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
                return await _apiService.PostAsync<dynamic>($"api/v1/booking-ticket/validate?showTimeId={showTimeId}", seatIds);
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

        // Thêm implementation cho GetAvailableSeatsAsync
        public async Task<ApiResponse<dynamic>> GetAvailableSeatsAsync(Guid showtimeId)
        {
            try
            {
                _logger.LogInformation("Getting available seats for showtime: {ShowtimeId}", showtimeId);
                return await _apiService.GetAsync<dynamic>($"api/v1/booking-ticket/available?showTimeId={showtimeId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available seats");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải thông tin ghế. Vui lòng thử lại."
                };
            }
        }

        // Thêm implementation cho ValidateSeatsAsync
        public async Task<ApiResponse<dynamic>> ValidateSeatsAsync(Guid showtimeId, List<Guid> seatIds)
        {
            try
            {
                _logger.LogInformation("Validating {SeatCount} seats for showtime: {ShowtimeId}", seatIds.Count, showtimeId);
                return await _apiService.PostAsync<dynamic>($"api/v1/booking-ticket/validate?showTimeId={showtimeId}", seatIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating seats");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể xác thực ghế. Vui lòng thử lại."
                };
            }
        }

        // Search Customer
        public async Task<ApiResponse<CustomerSearchViewModel>> SearchCustomerAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching customer with term: {SearchTerm}", searchTerm);
                return await _apiService.GetAsync<CustomerSearchViewModel>($"api/v1/booking-ticket/SearchCustomer?searchTerm={Uri.EscapeDataString(searchTerm)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customer");
                return new ApiResponse<CustomerSearchViewModel>
                {
                    Success = false,
                    Message = "Không thể tìm kiếm khách hàng. Vui lòng thử lại."
                };
            }
        }

        // Create Customer
        public async Task<ApiResponse<dynamic>> CreateCustomerAsync(CreateCustomerViewModel model)
        {
            try
            {
                _logger.LogInformation("Creating customer");
                return await _apiService.PostAsync<dynamic>("api/v1/booking-ticket/create-member-account", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tạo khách hàng. Vui lòng thử lại."
                };
            }
        }

        // Confirm Admin Booking
        public async Task<ApiResponse<dynamic>> ConfirmAdminBookingAsync(ConfirmAdminBookingViewModel model)
        {
            try
            {
                _logger.LogInformation("Confirming admin booking for showtime: {ShowTimeId}", model.ShowTimeId);
                return await _apiService.PostAsync<dynamic>("api/v1/booking-ticket/confirm-Admin-booking", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming admin booking");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể xác nhận đặt vé. Vui lòng thử lại."
                };
            }
        }

        // New methods for score conversion booking
        public async Task<ApiResponse<BookingConfirmationDetailViewModel>> GetBookingConfirmationDetailAsync(Guid showTimeId, List<Guid> seatIds, string memberId)
        {
            try
            {
                _logger.LogInformation("Getting booking confirmation detail for showtime: {ShowTimeId}, seatIds: {SeatIds}, memberId: {MemberId}", showTimeId, string.Join(",", seatIds), memberId);

                var seatIdsParam = string.Join("&seatIds=", seatIds);
                var url = $"api/v1/booking-ticket/booking-confirmation-detail?showTimeId={showTimeId}&seatIds={seatIdsParam}&memberId={Uri.EscapeDataString(memberId)}";

                return await _apiService.GetAsync<BookingConfirmationDetailViewModel>(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking confirmation detail");
                return new ApiResponse<BookingConfirmationDetailViewModel>
                {
                    Success = false,
                    Message = "Không thể tải thông tin xác nhận đặt vé. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<BookingConfirmSuccessViewModel>> ConfirmBookingWithScoreAsync(BookingConfirmWithScoreViewModel model)
        {
            try
            {
                _logger.LogInformation("Confirming booking with score for member: {MemberId}", model.MemberId);
                return await _apiService.PostAsync<BookingConfirmSuccessViewModel>("api/v1/booking-ticket/confirm-booking-with-score", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking with score");
                return new ApiResponse<BookingConfirmSuccessViewModel>
                {
                    Success = false,
                    Message = "Không thể xác nhận đặt vé. Vui lòng thử lại."
                };
            }
        }

        // Booking list management methods
        public async Task<ApiResponse<dynamic>> GetBookingListAsync(dynamic filter)
        {
            try
            {
                _logger.LogInformation("Getting booking list with filters");

                var queryParams = new List<string>();

                if (filter.FromDate != null)
                    queryParams.Add($"fromDate={((DateTime)filter.FromDate).ToString("yyyy-MM-dd")}");

                if (filter.ToDate != null)
                    queryParams.Add($"toDate={((DateTime)filter.ToDate).ToString("yyyy-MM-dd")}");

                if (!string.IsNullOrEmpty(filter.MovieTitle))
                    queryParams.Add($"movieTitle={Uri.EscapeDataString(filter.MovieTitle)}");

                if (!string.IsNullOrEmpty(filter.BookingStatus))
                    queryParams.Add($"bookingStatus={Uri.EscapeDataString(filter.BookingStatus)}");

                if (!string.IsNullOrEmpty(filter.CustomerSearch))
                    queryParams.Add($"customerSearch={Uri.EscapeDataString(filter.CustomerSearch)}");

                if (!string.IsNullOrEmpty(filter.BookingCode))
                    queryParams.Add($"bookingCode={Uri.EscapeDataString(filter.BookingCode)}");

                queryParams.Add($"page={filter.Page}");
                queryParams.Add($"pageSize={filter.PageSize}");
                queryParams.Add($"sortBy={Uri.EscapeDataString(filter.SortBy)}");
                queryParams.Add($"sortDirection={Uri.EscapeDataString(filter.SortDirection)}");

                var queryString = string.Join("&", queryParams);
                var url = $"api/v1/booking-ticket/bookings?{queryString}";

                return await _apiService.GetAsync<dynamic>(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking list");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải danh sách đặt vé. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> UpdateBookingStatusAsync(Guid bookingId, string newStatus)
        {
            try
            {
                _logger.LogInformation("Updating booking status for {BookingId} to {NewStatus}", bookingId, newStatus);

                var updateData = new { NewStatus = newStatus };
                return await _apiService.PutAsync<dynamic>($"api/v1/booking-ticket/booking/{bookingId}/status", updateData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể cập nhật trạng thái đặt vé. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> CancelBookingAsync(Guid bookingId, string reason)
        {
            try
            {
                _logger.LogInformation("Cancelling booking {BookingId} with reason: {Reason}", bookingId, reason);

                var cancelData = new { Reason = reason };
                return await _apiService.PostAsync<dynamic>($"api/v1/booking-ticket/booking/{bookingId}/cancel", cancelData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể hủy đặt vé. Vui lòng thử lại."
                };
            }
        }
    }
}