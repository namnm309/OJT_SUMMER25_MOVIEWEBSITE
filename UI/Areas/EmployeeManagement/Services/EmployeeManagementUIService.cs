using UI.Models;
using UI.Areas.EmployeeManagement.Models;
using UI.Services;

namespace UI.Areas.EmployeeManagement.Services
{
    public interface IEmployeeManagementUIService
    {

        Task<ApiResponse<dynamic>> GetEmployeesAsync();
        Task<ApiResponse<dynamic>> SearchEmployeesAsync(string searchTerm);
        

        Task<ApiResponse<dynamic>> AddEmployeeAsync(EmployeeCreateViewModel model);
        

        Task<ApiResponse<dynamic>> GetEmployeeAsync(Guid employeeId);
        Task<ApiResponse<dynamic>> UpdateEmployeeAsync(Guid employeeId, EmployeeUpdateViewModel model);
        

        Task<ApiResponse<dynamic>> DeleteEmployeeAsync(Guid employeeId);
    }

    public class EmployeeManagementUIService : IEmployeeManagementUIService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<EmployeeManagementUIService> _logger;

        public EmployeeManagementUIService(IApiService apiService, ILogger<EmployeeManagementUIService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<ApiResponse<dynamic>> GetEmployeesAsync()
        {
            try
            {
                _logger.LogInformation("Getting employees list");
                return await _apiService.GetAsync<dynamic>("api/v1/employee/GetAll");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employees list");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải danh sách nhân viên. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> SearchEmployeesAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching employees with term: {SearchTerm}", searchTerm);
                return await _apiService.GetAsync<dynamic>($"api/v1/employee/Search?query={Uri.EscapeDataString(searchTerm)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching employees");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tìm kiếm nhân viên. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> AddEmployeeAsync(EmployeeCreateViewModel model)
        {
            try
            {
                _logger.LogInformation("Adding new employee: {Username}", model.Username);

                // Ánh xạ ViewModel -> DTO backend
                var requestBody = new
                {
                    Account = model.Username,
                    Password = model.Password,
                    ConfirmPassword = model.Password, // Tạm dùng luôn Password làm ConfirmPassword (form hiện tại chưa có trường confirm)
                    FullName = model.FullName,
                    Email = model.Email,
                    IdentityCard = model.IdentityCard,
                    Gender = model.Gender ?? "Male", // backend nhận enum, string "Male"/"Female" vẫn được bind
                    PhoneNumber = model.Phone,
                    Address = model.Address,
                    DateOfBirth = model.BirthDate ?? DateTime.UtcNow.AddYears(-18),
                    ProfileImageUrl = (string?)null
                };

                return await _apiService.PostAsync<dynamic>("api/v1/employee/Add", requestBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding employee");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể thêm nhân viên. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> GetEmployeeAsync(Guid employeeId)
        {
            try
            {
                _logger.LogInformation("Getting employee detail: {EmployeeId}", employeeId);
                return await _apiService.GetAsync<dynamic>($"api/v1/employee/GetById?Id={employeeId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee detail");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể tải thông tin nhân viên. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> UpdateEmployeeAsync(Guid employeeId, EmployeeUpdateViewModel model)
        {
            try
            {
                _logger.LogInformation("Updating employee: {EmployeeId}", employeeId);

                var updateBody = new
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    IdentityCard = model.IdentityCard,
                    Gender = model.Gender ?? "Male",
                    PhoneNumber = model.Phone,
                    Address = model.Address,
                    DateOfBirth = model.BirthDate ?? DateTime.UtcNow.AddYears(-18),
                    ProfileImageUrl = (string?)null,
                    Password = model.Password,
                    ConfirmPassword = model.Password
                };

                return await _apiService.PatchAsync<dynamic>($"api/v1/employee/Update?Id={employeeId}", updateBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể cập nhật thông tin nhân viên. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<dynamic>> DeleteEmployeeAsync(Guid employeeId)
        {
            try
            {
                _logger.LogInformation("Deleting employee: {EmployeeId}", employeeId);
                var result = await _apiService.DeleteAsync($"api/v1/employee/Detele?Id={employeeId}");
                return new ApiResponse<dynamic> 
                { 
                    Success = result.Success, 
                    Message = result.Message,
                    Data = result.Data 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee");
                return new ApiResponse<dynamic>
                {
                    Success = false,
                    Message = "Không thể xóa nhân viên. Vui lòng thử lại."
                };
            }
        }
    }
} 