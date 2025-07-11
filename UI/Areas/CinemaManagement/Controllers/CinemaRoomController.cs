using Microsoft.AspNetCore.Mvc;
using UI.Areas.CinemaManagement.Models;
using UI.Areas.CinemaManagement.Services;
using UI.Models;
using System.Text.Json;

namespace UI.Areas.CinemaManagement.Controllers
{
    [Area("CinemaManagement")]
    public class CinemaRoomController : Controller
    {
        private readonly ICinemaManagementUIService _cinemaService;
        private readonly ILogger<CinemaRoomController> _logger;

        public CinemaRoomController(ICinemaManagementUIService cinemaService, ILogger<CinemaRoomController> logger)
        {
            _cinemaService = cinemaService;
            _logger = logger;
        }


        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = "")
        {
            try
            {
                _logger.LogInformation("Loading cinema rooms - Page: {Page}, Size: {PageSize}, Search: {Search}", page, pageSize, search);

                var response = string.IsNullOrWhiteSpace(search) 
                    ? await _cinemaService.GetCinemaRoomsPaginationAsync(page, pageSize)
                    : await _cinemaService.SearchCinemaRoomsAsync(search);

                _logger.LogInformation("API Response - Success: {Success}, Message: {Message}", response.Success, response.Message);

                if (!response.Success)
                {
                    _logger.LogWarning("API call failed: {Message}", response.Message);
                    TempData["Error"] = response.Message;
                    ViewBag.HasData = false;
                    ViewBag.DataCount = 0;
                    ViewBag.Total = 0;
                    return View(JsonDocument.Parse("[]").RootElement);
                }

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;


                _logger.LogInformation("API Response Data Type: {Type}", response.Data.GetType().Name);
                _logger.LogInformation("API Response Data ValueKind: {ValueKind}", response.Data.ValueKind);
                _logger.LogInformation("API Response Raw Data: {Data}", response.Data.GetRawText());


                if (response.Data.ValueKind == JsonValueKind.Object)
                {

                    if (response.Data.TryGetProperty("data", out var dataProperty))
                    {
                        _logger.LogInformation("Found 'data' property with {Count} items", 
                            dataProperty.ValueKind == JsonValueKind.Array ? dataProperty.GetArrayLength() : 0);
                        

                        ViewBag.Total = response.Data.TryGetProperty("total", out var totalProp) ? totalProp.GetInt32() : 
                                       (dataProperty.ValueKind == JsonValueKind.Array ? dataProperty.GetArrayLength() : 0);
                        ViewBag.CurrentPage = response.Data.TryGetProperty("page", out var pageProp) ? pageProp.GetInt32() : page;
                        ViewBag.PageSize = response.Data.TryGetProperty("pageSize", out var sizeProp) ? sizeProp.GetInt32() : pageSize;
                        

                        ViewBag.HasData = dataProperty.ValueKind == JsonValueKind.Array && dataProperty.GetArrayLength() > 0;
                        ViewBag.DataCount = dataProperty.ValueKind == JsonValueKind.Array ? dataProperty.GetArrayLength() : 0;
                        
                        return View(dataProperty);
                    }
                    else if (response.Data.TryGetProperty("Data", out var DataProperty))
                    {
                        _logger.LogInformation("Found 'Data' property with {Count} items", 
                            DataProperty.ValueKind == JsonValueKind.Array ? DataProperty.GetArrayLength() : 0);
                        
                        ViewBag.Total = response.Data.TryGetProperty("Total", out var TotalProp) ? TotalProp.GetInt32() : 
                                       (DataProperty.ValueKind == JsonValueKind.Array ? DataProperty.GetArrayLength() : 0);
                        ViewBag.CurrentPage = response.Data.TryGetProperty("Page", out var PageProp) ? PageProp.GetInt32() : page;
                        ViewBag.PageSize = response.Data.TryGetProperty("PageSize", out var SizeProp) ? SizeProp.GetInt32() : pageSize;
                        

                        ViewBag.HasData = DataProperty.ValueKind == JsonValueKind.Array && DataProperty.GetArrayLength() > 0;
                        ViewBag.DataCount = DataProperty.ValueKind == JsonValueKind.Array ? DataProperty.GetArrayLength() : 0;
                        
                        return View(DataProperty);
                    }
                    else if (response.Data.TryGetProperty("items", out var itemsProperty))
                    {
                        _logger.LogInformation("Found 'items' property with {Count} items", 
                            itemsProperty.ValueKind == JsonValueKind.Array ? itemsProperty.GetArrayLength() : 0);
                        
                        ViewBag.Total = response.Data.TryGetProperty("total", out var totalProp) ? totalProp.GetInt32() : 
                                       (itemsProperty.ValueKind == JsonValueKind.Array ? itemsProperty.GetArrayLength() : 0);
                        

                        ViewBag.HasData = itemsProperty.ValueKind == JsonValueKind.Array && itemsProperty.GetArrayLength() > 0;
                        ViewBag.DataCount = itemsProperty.ValueKind == JsonValueKind.Array ? itemsProperty.GetArrayLength() : 0;
                        
                        return View(itemsProperty);
                    }
                    else
                    {

                        _logger.LogInformation("No nested data property found, checking object properties");
                        

                        if (response.Data.TryGetProperty("id", out _) && response.Data.TryGetProperty("roomName", out _))
                        {

                            var singleRoomArray = JsonDocument.Parse($"[{response.Data.GetRawText()}]").RootElement;
                            ViewBag.Total = 1;
                            ViewBag.HasData = true;
                            ViewBag.DataCount = 1;
                            return View(singleRoomArray);
                        }
                        

                        _logger.LogWarning("Unknown object structure, returning raw object");
                        ViewBag.Total = 0;
                        ViewBag.HasData = false;
                        ViewBag.DataCount = 0;
                        return View(response.Data);
                    }
                }


                if (response.Data.ValueKind == JsonValueKind.Array)
                {
                    _logger.LogInformation("Direct array with {Count} items", response.Data.GetArrayLength());
                    ViewBag.Total = response.Data.GetArrayLength();
                    ViewBag.HasData = response.Data.GetArrayLength() > 0;
                    ViewBag.DataCount = response.Data.GetArrayLength();
                    return View(response.Data);
                }


                _logger.LogWarning("No valid data structure found in API response, ValueKind: {ValueKind}", response.Data.ValueKind);
                ViewBag.Total = 0;
                ViewBag.HasData = false;
                ViewBag.DataCount = 0;
                return View(JsonDocument.Parse("[]").RootElement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cinema rooms");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách phòng chiếu: " + ex.Message;
                ViewBag.HasData = false;
                ViewBag.DataCount = 0;
                ViewBag.Total = 0;
                return View(JsonDocument.Parse("[]").RootElement);
            }
        }


        public IActionResult Create()
        {
            return View(new CinemaRoomCreateViewModel());
        }


        [HttpPost]
        // [ValidateAntiForgeryToken] // Tạm thời disable để test
        public async Task<IActionResult> Create(CinemaRoomCreateViewModel model)
        {
            _logger.LogInformation("Create cinema room called. ModelState.IsValid: {IsValid}", ModelState.IsValid);
            _logger.LogInformation("Model data: RoomName={RoomName}, TotalSeats={TotalSeats}, NumberOfRows={NumberOfRows}, NumberOfColumns={NumberOfColumns}", 
                model.RoomName, model.TotalSeats, model.NumberOfRows, model.NumberOfColumns);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                _logger.LogWarning("ModelState validation failed: {@Errors}", errors);


                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("application/json") == true)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
                }
                return View(model);
            }

            try
            {
                var response = await _cinemaService.AddCinemaRoomAsync(model);

                if (response.Success)
                {

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                        Request.ContentType?.Contains("application/json") == true)
                    {
                        return Json(new { success = true, message = "Thêm phòng chiếu thành công!" });
                    }
                    
                    TempData["Success"] = "Thêm phòng chiếu thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                        Request.ContentType?.Contains("application/json") == true)
                    {
                        return Json(new { success = false, message = response.Message });
                    }
                    
                    TempData["Error"] = response.Message;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cinema room");
                

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("application/json") == true)
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi thêm phòng chiếu." });
                }
                
                TempData["Error"] = "Có lỗi xảy ra khi thêm phòng chiếu.";
                return View(model);
            }
        }


        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var response = await _cinemaService.GetCinemaRoomDetailAsync(id);

                if (!response.Success)
                {
                    TempData["Error"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }

                var roomData = response.Data;
                
                var model = new CinemaRoomUpdateViewModel
                {
                    RoomName = roomData.GetProperty("roomName").GetString(),
                    TotalSeats = roomData.GetProperty("totalSeats").GetInt32(),
                    IsActive = true // Default to true since we're editing existing room
                };

                ViewBag.RoomId = id;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cinema room for edit");
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin phòng chiếu.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CinemaRoomUpdateViewModel model)
        {
            _logger.LogInformation("Edit request received for room ID: {Id}, Model: {@Model}", id, model);
            
            // Đảm bảo số hàng và số cột phải được cung cấp
            if (model.NumberOfRows == null || model.NumberOfColumns == null)
            {
                ModelState.AddModelError("", "Số hàng và số cột là bắt buộc");
            }
            
            if (!ModelState.IsValid)
            {

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("application/json") == true)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
                }
                
                ViewBag.RoomId = id;
                return View(model);
            }

            try
            {
                // Đặt IsActive mặc định là true
                model.IsActive = true;
                
                // Xử lý cấu hình ghế ngồi
                var seatConfig = Request.Form["SeatConfig"];
                List<SeatUpdateViewModel> seatUpdates = new List<SeatUpdateViewModel>();
                
                if (!string.IsNullOrEmpty(seatConfig))
                {
                    try
                    {
                        // Parse cấu hình ghế từ JSON
                        var seatData = System.Text.Json.JsonSerializer.Deserialize<List<SeatConfigItem>>(seatConfig);
                        
                        if (seatData != null && seatData.Count > 0)
                        {
                            _logger.LogInformation("Processing {Count} seats in configuration", seatData.Count);
                            
                            foreach (var seat in seatData)
                            {
                                // Chuyển đổi từ dữ liệu form sang model
                                var seatUpdate = new SeatUpdateViewModel
                                {
                                    SeatId = seat.id != null ? Guid.Parse(seat.id) : Guid.Empty,
                                    SeatCode = seat.code,
                                    RowIndex = seat.row,
                                    ColumnIndex = seat.column,
                                    SeatType = GetSeatTypeName(seat.type),
                                    IsActive = true
                                };
                                
                                seatUpdates.Add(seatUpdate);
                            }
                            
                            // Cập nhật cấu hình ghế
                            var seatResponse = await _cinemaService.UpdateRoomSeatsAsync(id, seatUpdates);
                            
                            if (!seatResponse.Success)
                            {
                                _logger.LogWarning("Failed to update seat configuration: {Message}", seatResponse.Message);
                                // Vẫn tiếp tục cập nhật thông tin phòng
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing seat configuration");
                        // Vẫn tiếp tục cập nhật thông tin phòng
                    }
                }
                
                _logger.LogInformation("Updating cinema room with ID: {Id}, Model: {@Model}", id, model);
                var response = await _cinemaService.UpdateCinemaRoomAsync(id, model);

                if (response.Success)
                {

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                        Request.ContentType?.Contains("application/json") == true)
                    {
                        return Json(new { success = true, message = "Cập nhật phòng chiếu thành công!" });
                    }
                    
                    TempData["Success"] = "Cập nhật phòng chiếu thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                        Request.ContentType?.Contains("application/json") == true)
                    {
                        return Json(new { success = false, message = response.Message });
                    }
                    
                    TempData["Error"] = response.Message;
                    ViewBag.RoomId = id;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cinema room with ID: {Id}", id);
                

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.ContentType?.Contains("application/json") == true)
                {
                    return Json(new { success = false, message = $"Có lỗi xảy ra khi cập nhật phòng chiếu: {ex.Message}" });
                }
                
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật phòng chiếu.";
                ViewBag.RoomId = id;
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var response = await _cinemaService.DeleteCinemaRoomAsync(id);

                if (response.Success)
                {
                    TempData["Success"] = "Xóa phòng chiếu thành công!";
                }
                else
                {
                    TempData["Error"] = response.Message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cinema room");
                TempData["Error"] = "Có lỗi xảy ra khi xóa phòng chiếu.";
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                _logger.LogInformation("Details action called for room ID: {RoomId}", id);
                var response = await _cinemaService.GetCinemaRoomDetailAsync(id);
                _logger.LogInformation("API response success: {Success}, Message: {Message}", response.Success, response.Message);

                if (!response.Success)
                {

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                        Request.Headers["Accept"].ToString().Contains("application/json"))
                    {
                        _logger.LogWarning("Returning JSON error response for AJAX request");
                        return Json(new { Code = 400, Message = response.Message, Data = (object?)null });
                    }
                    
                    TempData["Error"] = response.Message;
                    return RedirectToAction(nameof(Index));
                }


                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    _logger.LogInformation("Returning JSON success response for AJAX request");

                    return Json(new { Code = 200, Message = "OK", Data = response.Data });
                }

                ViewBag.RoomId = id;
                return View(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cinema room details for ID: {RoomId}", id);
                

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    var errorMessage = $"Có lỗi xảy ra khi tải chi tiết phòng chiếu: {ex.Message}";
                    _logger.LogWarning("Returning JSON error response: {Message}", errorMessage);
                    return Json(new { Code = 500, Message = errorMessage, Data = (object?)null });
                }
                
                TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết phòng chiếu.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // Helper class để parse dữ liệu từ form
        private class SeatConfigItem
        {
            public int row { get; set; }
            public int column { get; set; }
            public int type { get; set; }
            public string? id { get; set; }
            public string code { get; set; } = string.Empty;
        }

        // Helper method để chuyển đổi từ số sang tên loại ghế
        private string GetSeatTypeName(int type)
        {
            return type switch
            {
                1 => "VIP",
                2 => "Couple",
                _ => "Normal"
            };
        }
    }
} 