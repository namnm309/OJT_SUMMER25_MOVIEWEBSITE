using UI.Models;
using UI.Services;

namespace UI.Areas.MovieManagement.Services
{
    public interface IMovieManagementUIService
    {
        // T27: View Movie List
        Task<ApiResponse<dynamic>> GetMoviesAsync();
        
        // T28: Add Movie
        Task<ApiResponse<dynamic>> AddMovieAsync(MovieCreateViewModel model);
        
        // T29: Edit Movie
        Task<ApiResponse<dynamic>> GetMovieAsync(Guid movieId);
        Task<ApiResponse<dynamic>> UpdateMovieAsync(Guid movieId, MovieUpdateViewModel model);
        
        // T30: Delete Movie
        Task<ApiResponse<dynamic>> DeleteMovieAsync(Guid movieId);
    }

    public class MovieManagementUIService : IMovieManagementUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<MovieManagementUIService> _logger;

        public MovieManagementUIService(IApiService apiService, ILogger<MovieManagementUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<ApiResponse<dynamic>> GetMoviesAsync()
        {
            try
            {
                _logger.LogInformation("Getting movies list for management");
                return await _apiService.GetAsync<dynamic>("admin/movies");
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

        public async Task<ApiResponse<dynamic>> AddMovieAsync(MovieCreateViewModel model)
        {
            try
            {
                _logger.LogInformation("Adding new movie: {Title}", model.Title);
                return await _apiService.PostAsync<dynamic>("admin/movies", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding movie");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể thêm phim. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetMovieAsync(Guid movieId)
        {
            try
            {
                _logger.LogInformation("Getting movie detail: {MovieId}", movieId);
                return await _apiService.GetAsync<dynamic>($"admin/movies/{movieId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie detail");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải thông tin phim. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> UpdateMovieAsync(Guid movieId, MovieUpdateViewModel model)
        {
            try
            {
                _logger.LogInformation("Updating movie: {MovieId}", movieId);
                return await _apiService.PutAsync<dynamic>($"admin/movies/{movieId}", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating movie");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể cập nhật thông tin phim. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> DeleteMovieAsync(Guid movieId)
        {
            try
            {
                _logger.LogInformation("Deleting movie: {MovieId}", movieId);
                var result = await _apiService.DeleteAsync($"admin/movies/{movieId}");
                return new ApiResponse<dynamic> 
                { 
                    Success = result.Success, 
                    Message = result.Message,
                    Data = result.Data 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting movie");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể xóa phim. Vui lòng thử lại."
                };
            }
        }
    }
} 