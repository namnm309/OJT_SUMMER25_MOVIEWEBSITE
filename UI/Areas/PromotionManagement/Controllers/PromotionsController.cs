using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UI.Models;
using UI.Services;

namespace UI.Areas.PromotionManagement.Controllers
{
    [Area("PromotionManagement")]
    [Authorize(Roles = "Admin,Staff")]
    public class PromotionsController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<PromotionsController> _logger;
        //Khai báo service rồi mới sài được
        private readonly IImageService _imageService;


        // Thêm vào constructor
        public PromotionsController(
            IApiService apiService,
            ILogger<PromotionsController> logger,
            // thêm vào constructor
            IImageService imageService)
        {
            _apiService = apiService;
            _logger = logger;
            _imageService = imageService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý khuyến mãi";

            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/promotions");

                if (result.Success && result.Data.ValueKind != JsonValueKind.Undefined)
                {
                    if (result.Data.TryGetProperty("data", out var dataProp))
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var promotions = JsonSerializer.Deserialize<List<PromotionViewModel>>(
                            dataProp.GetRawText(), options);

                        _logger.LogInformation("Received {Count} promotions", promotions?.Count);
                        return View(promotions);
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

            return View(new List<PromotionViewModel>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm khuyến mãi mới";
            return View();
        }

        // Sửa action Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionViewModel model, IFormFile imageFile) // khai báo biến để sài 
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
                    description = model.Description,
                    imageUrl = model.ImageUrl,
                };

                var result = await _apiService.PostAsync<PromotionViewModel>("/api/v1/promotions", promotionsData);

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
        public async Task<IActionResult> Edit(string id)
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

                        var promotion = JsonSerializer.Deserialize<PromotionViewModel>(
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
        public async Task<IActionResult> Edit(PromotionViewModel model, IFormFile imageFile)
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
        public async Task<IActionResult> Delete(string id)
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
    }
} 