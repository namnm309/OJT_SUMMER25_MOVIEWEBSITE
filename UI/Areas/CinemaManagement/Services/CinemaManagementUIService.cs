using UI.Models;
using UI.Areas.CinemaManagement.Models;
using UI.Services;
using System.Text.Json;

namespace UI.Areas.CinemaManagement.Services
{
    public interface ICinemaManagementUIService
    {
        Task<ApiResponse<JsonElement>> GetCinemaRoomsAsync();
        Task<ApiResponse<JsonElement>> GetCinemaRoomsPaginationAsync(int page = 1, int pageSize = 10);
        Task<ApiResponse<JsonElement>> SearchCinemaRoomsAsync(string searchTerm);
        Task<ApiResponse<JsonElement>> AddCinemaRoomAsync(CinemaRoomCreateViewModel model);
        Task<ApiResponse<JsonElement>> UpdateCinemaRoomAsync(Guid id, CinemaRoomUpdateViewModel model);
        Task<ApiResponse<JsonElement>> DeleteCinemaRoomAsync(Guid id);
        
        // Seat Management
        Task<ApiResponse<JsonElement>> GetCinemaRoomDetailAsync(Guid roomId);
        Task<ApiResponse<JsonElement>> UpdateRoomSeatsAsync(Guid roomId, List<SeatUpdateViewModel> seats);
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

        public async Task<ApiResponse<JsonElement>> GetCinemaRoomsAsync()
        {
            try
            {
                _logger.LogInformation("Getting cinema rooms list");
                return await _apiService.GetAsync<JsonElement>("api/v1/cinemaroom/ViewRoom");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cinema rooms list");
                return new ApiResponse<JsonElement>
                {
                    Success = false,
                    Message = "Không thể tải danh sách phòng chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<JsonElement>> GetCinemaRoomsPaginationAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting cinema rooms with pagination - Page: {Page}, PageSize: {PageSize}", page, pageSize);
                return await _apiService.GetAsync<JsonElement>($"api/v1/cinemaroom/ViewRoomPagination?Page={page}&PageSize={pageSize}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cinema rooms with pagination");
                return new ApiResponse<JsonElement>
                {
                    Success = false,
                    Message = "Không thể tải danh sách phòng chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<JsonElement>> SearchCinemaRoomsAsync(string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching cinema rooms with term: {SearchTerm}", searchTerm);
                return await _apiService.GetAsync<JsonElement>($"api/v1/cinemaroom/search?keyword={Uri.EscapeDataString(searchTerm)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching cinema rooms");
                return new ApiResponse<JsonElement>
                {
                    Success = false,
                    Message = "Không thể tìm kiếm phòng chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<JsonElement>> AddCinemaRoomAsync(CinemaRoomCreateViewModel model)
        {
            try
            {
                _logger.LogInformation("Adding new cinema room: {RoomName}, Rows: {Rows}, Columns: {Columns}, TotalSeats: {TotalSeats}", 
                    model.RoomName, model.NumberOfRows, model.NumberOfColumns, model.TotalSeats);
                
                // Map ViewModel to backend DTO format
                var dto = new
                {
                    RoomName = model.RoomName,
                    TotalSeats = model.TotalSeats,
                    NumberOfRows = model.NumberOfRows,
                    NumberOfColumns = model.NumberOfColumns,
                    DefaultSeatPrice = 100000 // Default price
                };
                
                return await _apiService.PostAsync<JsonElement>("api/v1/cinemaroom/Add", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding cinema room");
                return new ApiResponse<JsonElement>
                {
                    Success = false,
                    Message = "Không thể thêm phòng chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<JsonElement>> UpdateCinemaRoomAsync(Guid id, CinemaRoomUpdateViewModel model)
        {
            try
            {
                _logger.LogInformation("Updating cinema room: {Id} with data: {@Model}", id, model);
                
                // Construct the correct URL format
                var url = $"api/v1/cinemaroom/Update?Id={id}";
                _logger.LogInformation("Sending PATCH request to URL: {Url}", url);
                
                var response = await _apiService.PatchAsync<JsonElement>(url, model);
                
                if (response.Success)
                {
                    _logger.LogInformation("Successfully updated cinema room {Id}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to update cinema room {Id}: {Message}", id, response.Message);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cinema room {Id}", id);
                return new ApiResponse<JsonElement>
                {
                    Success = false,
                    Message = $"Không thể cập nhật phòng chiếu: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<JsonElement>> DeleteCinemaRoomAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting cinema room: {Id}", id);
                var result = await _apiService.DeleteAsync($"api/v1/cinemaroom/Delete/{id}");
                return new ApiResponse<JsonElement>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Success ? JsonDocument.Parse("{}").RootElement : default
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cinema room");
                return new ApiResponse<JsonElement>
                {
                    Success = false,
                    Message = "Không thể xóa phòng chiếu. Vui lòng thử lại."
                };
            }
        }

        public async Task<ApiResponse<JsonElement>> GetCinemaRoomDetailAsync(Guid roomId)
        {
            try
            {
                // Use correct URL format with Id parameter
                var url = $"api/v1/cinemaroom/ViewSeat?Id={roomId}";
                _logger.LogInformation("Getting cinema room detail from URL: {Url}", url);
                
                var response = await _apiService.GetAsync<JsonElement>(url);
                
                _logger.LogInformation("API response received - Success: {Success}, Has Data: {HasData}, ValueKind: {ValueKind}", 
                    response.Success, 
                    response.Data.ValueKind != JsonValueKind.Undefined,
                    response.Data.ValueKind);
                
                if (response.Success)
                {
                    _logger.LogInformation("API response data raw: {RawData}", 
                        response.Data.ValueKind == JsonValueKind.Undefined ? "undefined" : 
                        response.Data.ToString().Length > 100 ? response.Data.ToString().Substring(0, 100) + "..." : 
                        response.Data.ToString());
                    
                    // Kiểm tra và bổ sung thông tin về số hàng và số cột nếu chưa có
                    if (response.Data.ValueKind == JsonValueKind.Object)
                    {
                        var data = response.Data;
                        
                        // Nếu không có thông tin về số hàng và số cột, tính toán từ danh sách ghế
                        if (!data.TryGetProperty("numberOfRows", out _) && !data.TryGetProperty("NumberOfRows", out _))
                        {
                            if (data.TryGetProperty("seats", out var seatsProperty) || data.TryGetProperty("Seats", out seatsProperty))
                            {
                                if (seatsProperty.ValueKind == JsonValueKind.Array && seatsProperty.GetArrayLength() > 0)
                                {
                                    // Tìm số hàng và số cột lớn nhất từ danh sách ghế
                                    int maxRow = 0;
                                    int maxCol = 0;
                                    
                                    foreach (var seat in seatsProperty.EnumerateArray())
                                    {
                                        int row = 0;
                                        int col = 0;
                                        
                                        if (seat.TryGetProperty("rowIndex", out var rowProp))
                                            row = rowProp.GetInt32();
                                        else if (seat.TryGetProperty("RowIndex", out rowProp))
                                            row = rowProp.GetInt32();
                                            
                                        if (seat.TryGetProperty("columnIndex", out var colProp))
                                            col = colProp.GetInt32();
                                        else if (seat.TryGetProperty("ColumnIndex", out colProp))
                                            col = colProp.GetInt32();
                                            
                                        maxRow = Math.Max(maxRow, row);
                                        maxCol = Math.Max(maxCol, col);
                                    }
                                    
                                    // Ghi log thông tin tính toán được
                                    _logger.LogInformation("Calculated room dimensions: Rows={Rows}, Columns={Columns}", 
                                        maxRow, maxCol);
                                }
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("API returned error: {Message}", response.Message);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cinema room detail for ID: {RoomId}", roomId);
                return new ApiResponse<JsonElement>
                {
                    Success = false,
                    Message = $"Không thể tải thông tin phòng chiếu: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<JsonElement>> UpdateRoomSeatsAsync(Guid roomId, List<SeatUpdateViewModel> seats)
        {
            try
            {
                _logger.LogInformation("Updating seats for room: {RoomId} with {Count} seats", roomId, seats.Count);
                
                // Chuẩn bị dữ liệu để gửi đến API
                var updateData = new
                {
                    RoomId = roomId,
                    Seats = seats.Select(s => new
                    {
                        Id = s.SeatId == Guid.Empty ? (Guid?)null : s.SeatId,
                        SeatCode = s.SeatCode,
                        SeatType = s.SeatType,
                        RowIndex = s.RowIndex,
                        ColumnIndex = s.ColumnIndex,
                        IsActive = s.IsActive
                    }).ToList()
                };
                
                // Ghi log dữ liệu gửi đi
                _logger.LogInformation("Sending seat update data: {@UpdateData}", 
                    new { RoomId = updateData.RoomId, SeatCount = updateData.Seats.Count });
                
                // Gửi request đến API endpoint
                return await _apiService.PutAsync<JsonElement>($"api/v1/cinemaroom/UpdateSeats", updateData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room seats for room {RoomId}", roomId);
                return new ApiResponse<JsonElement>
                {
                    Success = false,
                    Message = $"Không thể cập nhật ghế ngồi: {ex.Message}"
                };
            }
        }
    }
} 