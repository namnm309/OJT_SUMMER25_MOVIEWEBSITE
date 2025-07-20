using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using UI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;

namespace UI.Controllers
{
    [Authorize]
    public class VouchersController : Controller
    {
        private readonly IApiService _apiService;
        public VouchersController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            // Call the API to get all active vouchers (promotions)
            var allVouchersResp = await _apiService.GetAsync<List<JsonElement>>("/api/v1/promotions");
            ViewBag.AllVouchers = allVouchersResp.Success ? allVouchersResp.Data : null;

            // Call the API to get user's saved vouchers
            var userVouchersResp = await _apiService.GetAsync<List<JsonElement>>("/api/v1/vouchers/my");
            ViewBag.UserVouchers = userVouchersResp.Success ? userVouchersResp.Data : new List<JsonElement>();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveUserPromotion([FromBody] SaveUserPromotionRequest request)
        {
            try
            {
                if (request?.PromotionId == null)
                {
                    return Json(new { success = false, message = "PromotionId không được để trống" });
                }

                // Call the API to save user promotion
                var response = await _apiService.PostAsync<bool>("/api/v1/promotions/save-user-promotion", new
                {
                    promotionId = request.PromotionId
                });

                if (response.Success)
                {
                    return Json(new { success = true, message = "Đã lưu voucher thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = response.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi lưu voucher: " + ex.Message });
            }
        }

        public class SaveUserPromotionRequest
        {
            [JsonPropertyName("promotionId")]
            public string? PromotionId { get; set; }
        }
    }
} 