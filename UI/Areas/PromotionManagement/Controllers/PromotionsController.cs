using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using UI.Areas.PromotionManagement.Services;
using UI.Models;
using UI.Services;
using UI.Areas.PromotionManagement.Models;

namespace UI.Areas.PromotionManagement.Controllers
{
    [Area("PromotionManagement")]
    [Authorize(Roles = "Admin,Staff")]
    public class PromotionsController : Controller
    {
        private readonly IApiService _apiService;        private readonly ILogger<PromotionsController> _logger;
        //Khai báo service rồi mới sài được
        private readonly IImageService _imageService;

        private readonly IPromotionManagementUIService _promotionService;

        // Thêm vào constructor
        public PromotionsController(
            IApiService apiService,
            ILogger<PromotionsController> logger,
            IImageService imageService,
            IPromotionManagementUIService promotionService) // Inject the service
        {
            _apiService = apiService;
            _logger = logger;
            _imageService = imageService;
            _promotionService = promotionService; // Assign to the field
        }

        //public async Task<IActionResult> Index()
        //{
        //    ViewData["Title"] = "Khuyến mãi";

        //    try
        //    {
        //        var result = await _apiService.GetAsync<JsonElement>("/api/v1/promotions");

        //        if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
        //        {
        //            if (result.Data.TryGetProperty("data", out var dataProp))
        //            {
        //                var options = new JsonSerializerOptions
        //                {
        //                    PropertyNameCaseInsensitive = true
        //                };

        //                var promotions = JsonSerializer.Deserialize<List<PromotionViewModel>>(
        //                    dataProp.GetRawText(), options);

        //                _logger.LogInformation("Received {Count} promotions", promotions?.Count);
        //                return View(promotions);
        //            }
        //        }

        //        _logger.LogError("Failed to get promotions: {Message}", result.Message);
        //        TempData["ErrorMessage"] = result.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting promotions");
        //        TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách khuyến mãi";
        //    }

        //    return View(new List<PromotionViewModel>());
        //}

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Khuyến mãi";

            // Log user info for debugging
            _logger.LogInformation("User authenticated: {IsAuthenticated}, User: {UserName}, Roles: {Roles}", 
                User.Identity?.IsAuthenticated, 
                User.Identity?.Name,
                string.Join(", ", User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)));

            try
            {
                _logger.LogInformation("Attempting to get promotions from API");
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/promotions");
                
                _logger.LogInformation("API Response - Success: {Success}, StatusCode: {StatusCode}, Message: {Message}", 
                    result.Success, result.StatusCode, result.Message);

                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    _logger.LogInformation("API returned data, attempting to parse");
                    _logger.LogInformation("Raw API response: {Response}", result.Data.ToString());
                    
                    if (result.Data.TryGetProperty("data", out var dataProp))
                    {
                        _logger.LogInformation("Found 'data' property, attempting to deserialize");
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        try
                        {
                            // Deserialize the promotions into a List<PromotionViewModel>
                            var promotions = JsonSerializer.Deserialize<List<PromotionDto>>(dataProp.GetRawText(), options);

                            _logger.LogInformation("Successfully parsed {Count} promotions", promotions?.Count);
                            return View(promotions);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize promotions: {RawData}", dataProp.GetRawText());
                            TempData["ErrorMessage"] = "Lỗi khi xử lý dữ liệu từ server";
                        }
                    }
                    else
                    {
                        _logger.LogWarning("API response does not contain 'data' property. Available properties: {Properties}", 
                            string.Join(", ", result.Data.EnumerateObject().Select(p => p.Name)));
                    }
                }

                _logger.LogError("Failed to get promotions: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting promotions");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách khuyến mãi";
            }

            return View(new List<PromotionDto>());
        }


        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm khuyến mãi mới";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionDto model, IFormFile imageFile)
        {
            try
            {
                // Xử lý upload ảnh nếu có
                if (imageFile != null && imageFile.Length > 0)
                {
                    model.ImageUrl = await _imageService.UploadImageAsync(imageFile);
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var promotionsData = new
                {
                    title = model.Title,
                    startDate = model.StartDate,
                    endDate = model.EndDate,
                    discountPercent = model.DiscountPercent,
                    requiredPoints = model.RequiredPoints,
                    description = model.Description,
                    imageUrl = model.ImageUrl,
                };

                var result = await _apiService.PostAsync<PromotionDto>("/api/v1/promotions", promotionsData);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Thêm khuyến mãi thành công!";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", result.Message ?? "Có lỗi xảy ra khi thêm khuyến mãi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating promotion");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm khuyến mãi");
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var result = await _apiService.GetAsync<JsonElement>($"/api/v1/promotions/{id}");

                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    if (result.Data.TryGetProperty("data", out var dataProp))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var promotion = JsonSerializer.Deserialize<PromotionDto>(
                            dataProp.GetRawText(), options);

                        return View(promotion);
                    }
                }

                TempData["ErrorMessage"] = result.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting promotion details");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin khuyến mãi";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PromotionDto model, IFormFile imageFile)
        {
            try
            {
                // Xử lý upload ảnh nếu có
                if (imageFile != null && imageFile.Length > 0)
                {
                    model.ImageUrl = await _imageService.UploadImageAsync(imageFile);
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var promotionsData = new
                {
                    id = model.Id,
                    title = model.Title,
                    startDate = model.StartDate,
                    endDate = model.EndDate,
                    discountPercent = model.DiscountPercent,
                    requiredPoints = model.RequiredPoints,
                    description = model.Description,
                    imageUrl = model.ImageUrl,
                };
               
                var result = await _apiService.PutAsync<JsonElement>("/api/v1/promotions", promotionsData);
                

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Cập nhật khuyến mãi thành công!";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating promotion");
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _apiService.DeleteAsync($"/api/v1/promotions/{id}");

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Xóa khuyến mãi thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting promotion");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa khuyến mãi";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPromotions()
        {
            try
            {
                _logger.LogInformation("Getting all promotions for dashboard");
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/promotions");

                if (result.Success)
                {
                    return Json(result);
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all promotions");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách khuyến mãi" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] PromotionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();
                    
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors });
                }

                var promotionsData = new
                {
                    title = model.Title,
                    startDate = model.StartDate,
                    endDate = model.EndDate,
                    discountPercent = model.DiscountPercent,
                    requiredPoints = model.RequiredPoints,
                    description = model.Description,
                    imageUrl = model.ImageUrl,
                };

                var result = await _apiService.PostAsync<JsonElement>("/api/v1/promotions", promotionsData);

                if (result.Success)
                {
                    return Json(new { success = true, message = "Thêm khuyến mãi thành công!" });
                }

                return Json(new { success = false, message = result.Message ?? "Có lỗi xảy ra khi thêm khuyến mãi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating promotion via AJAX");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm khuyến mãi" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAjax([FromBody] PromotionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();
                    
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors });
                }

                var promotionsData = new
                {
                    id = model.Id,
                    title = model.Title,
                    startDate = model.StartDate,
                    endDate = model.EndDate,
                    discountPercent = model.DiscountPercent,
                    requiredPoints = model.RequiredPoints,
                    description = model.Description,
                    imageUrl = model.ImageUrl,
                };

                var result = await _apiService.PutAsync<JsonElement>("/api/v1/promotions", promotionsData);

                if (result.Success)
                {
                    return Json(new { success = true, message = "Cập nhật khuyến mãi thành công!" });
                }

                return Json(new { success = false, message = result.Message ?? "Có lỗi xảy ra khi cập nhật khuyến mãi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating promotion via AJAX");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật khuyến mãi" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAjax(string id)
        {
            try
            {
                var result = await _apiService.DeleteAsync($"/api/v1/promotions/{id}");

                if (result.Success)
                {
                    return Json(new { success = true, message = "Xóa khuyến mãi thành công!" });
                }

                return Json(new { success = false, message = result.Message ?? "Có lỗi xảy ra khi xóa khuyến mãi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting promotion via AJAX");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa khuyến mãi" });
            }
        }
    }
} 