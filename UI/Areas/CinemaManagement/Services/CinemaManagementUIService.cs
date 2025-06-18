using UI.Models;
using UI.Areas.CinemaManagement.Models;
using UI.Services;

namespace UI.Areas.CinemaManagement.Services
{
    public interface ICinemaManagementUIService
    {
        // T25: Display List of Cinema Rooms
        Task<ApiResponse<dynamic>> GetCinemaRoomsAsync();
        Task<ApiResponse<dynamic>> SearchCinemaRoomsAsync(string searchTerm);
        Task<ApiResponse<dynamic>> AddCinemaRoomAsync(CinemaRoomCreateViewModel model);
        
        // T26: Cinema Room Detail (Seat Management)
        Task<ApiResponse<dynamic>> GetCinemaRoomDetailAsync(Guid roomId);
        Task<ApiResponse<dynamic>> UpdateRoomSeatsAsync(Guid roomId, List<SeatUpdateViewModel> seats);
    }

    public class CinemaManagementUIService : ICinemaManagementUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<CinemaManagementUIService> _logger;

        public CinemaManagementUIService(IApiService apiService, ILogger<CinemaManagementUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<ApiResponse<dynamic>> GetCinemaRoomsAsync()
        {
            try
            {
                _logger.LogInformation("Getting cinema rooms list");
                return await _apiService.GetAsync<dynamic>("admin/cinema-rooms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cinema rooms list");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải danh sách phòng chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> SearchCinemaRoomsAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching cinema rooms with term: {SearchTerm}", searchTerm);
                return await _apiService.GetAsync<dynamic>($"admin/cinema-rooms/search?term={Uri.EscapeDataString(searchTerm)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching cinema rooms");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tìm kiếm phòng chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> AddCinemaRoomAsync(CinemaRoomCreateViewModel model)
        {
            try
            {
                _logger.LogInformation("Adding new cinema room: {RoomName}", model.RoomName);
                return await _apiService.PostAsync<dynamic>("admin/cinema-rooms", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding cinema room");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể thêm phòng chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetCinemaRoomDetailAsync(Guid roomId)
        {
            try
            {
                _logger.LogInformation("Getting cinema room detail: {RoomId}", roomId);
                return await _apiService.GetAsync<dynamic>($"admin/cinema-rooms/{roomId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cinema room detail");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải thông tin phòng chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> UpdateRoomSeatsAsync(Guid roomId, List<SeatUpdateViewModel> seats)
        {
            try
            {
                _logger.LogInformation("Updating seats for room: {RoomId}", roomId);
                return await _apiService.PutAsync<dynamic>($"admin/cinema-rooms/{roomId}/seats", seats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room seats");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể cập nhật ghế ngồi. Vui lòng thử lại."
                };
            }
        }
    }
} 