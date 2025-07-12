using UI.Models;
using UI.Services;

namespace UI.Areas.PromotionManagement.Services
{
    public interface IPromotionManagementUIService
    {
        // T31: View Promotion List
        Task<ApiResponse<dynamic>> GetPromotionsAsync();
        Task<ApiResponse<dynamic>> SearchPromotionsAsync(string searchTerm);

        //        // T32: Add Promotion
        //        Task<ApiResponse<dynamic>> AddPromotionAsync(PromotionCreateViewModel model);

        //        // T33: Edit Promotion
        //        Task<ApiResponse<dynamic>> GetPromotionAsync(Guid promotionId);
        //        Task<ApiResponse<dynamic>> UpdatePromotionAsync(Guid promotionId, PromotionUpdateViewModel model);

        //        // T34: Delete Promotion
        //        Task<ApiResponse<dynamic>> DeletePromotionAsync(Guid promotionId);
    }

    public class PromotionManagementUIService : IPromotionManagementUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<PromotionManagementUIService> _logger;

        public PromotionManagementUIService(IApiService apiService, ILogger<PromotionManagementUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<ApiResponse<dynamic>> GetPromotionsAsync()
        {
            try
            {
                _logger.LogInformation("Getting promotions list");
                return await _apiService.GetAsync<dynamic>("api/v1/promotions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting promotions list");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải danh sách khuyến mãi. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> SearchPromotionsAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching promotions with term: {SearchTerm}", searchTerm);
                return await _apiService.GetAsync<dynamic>($"admin/promotions/search?term={Uri.EscapeDataString(searchTerm)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching promotions");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tìm kiếm khuyến mãi. Vui lòng thử lại."
                };
            }
        }

        //        public async Task<ApiResponse<dynamic>> AddPromotionAsync(PromotionCreateViewModel model)
        //        {
        //            try
        //            {
        //                _logger.LogInformation("Adding new promotion: {Title}", model.Title);
        //                return await _apiService.PostAsync<dynamic>("admin/promotions", model);
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, "Error adding promotion");
        //                return new ApiResponse<dynamic>
        //                {
        //                    Success = false,
        //                    Message = "Không thể thêm khuyến mãi. Vui lòng thử lại."
        //                };
        //            }
        //        }

        //        public async Task<ApiResponse<dynamic>> GetPromotionAsync(Guid promotionId)
        //        {
        //            try
        //            {
        //                _logger.LogInformation("Getting promotion detail: {PromotionId}", promotionId);
        //                return await _apiService.GetAsync<dynamic>($"admin/promotions/{promotionId}");
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, "Error getting promotion detail");
        //                return new ApiResponse<dynamic>
        //                {
        //                    Success = false,
        //                    Message = "Không thể tải thông tin khuyến mãi. Vui lòng thử lại."
        //                };
        //            }
        //        }

        //        public async Task<ApiResponse<dynamic>> UpdatePromotionAsync(Guid promotionId, PromotionUpdateViewModel model)
        //        {
        //            try
        //            {
        //                _logger.LogInformation("Updating promotion: {PromotionId}", promotionId);
        //                return await _apiService.PutAsync<dynamic>($"admin/promotions/{promotionId}", model);
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, "Error updating promotion");
        //                return new ApiResponse<dynamic>
        //                {
        //                    Success = false,
        //                    Message = "Không thể cập nhật khuyến mãi. Vui lòng thử lại."
        //                };
        //            }
        //        }

        //        public async Task<ApiResponse<dynamic>> DeletePromotionAsync(Guid promotionId)
        //        {
        //            try
        //            {
        //                _logger.LogInformation("Deleting promotion: {PromotionId}", promotionId);
        //                var result = await _apiService.DeleteAsync($"admin/promotions/{promotionId}");
        //                return new ApiResponse<dynamic> 
        //                { 
        //                    Success = result.Success, 
        //                    Message = result.Message,
        //                    Data = result.Data 
        //                };
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, "Error deleting promotion");
        //                return new ApiResponse<dynamic>
        //                {
        //                    Success = false,
        //                    Message = "Không thể xóa khuyến mãi. Vui lòng thử lại."
        //                };
        //            }
        //        }
    }
}