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
            // Call the API to get user profile (including points)
            var userProfileResp = await _apiService.GetAsync<JsonElement>("/api/user/profile");
            if (userProfileResp.Success && userProfileResp.Data.ValueKind != JsonValueKind.Undefined)
            {
                var userProfile = userProfileResp.Data;
                double userScore = userProfile.TryGetProperty("score", out var scoreProp) && scoreProp.ValueKind == JsonValueKind.Number ? scoreProp.GetDouble() : 0;
                ViewBag.UserScore = userScore;
            }
            else
            {
                ViewBag.UserScore = 0;
            }

            // Call the API to get all active vouchers (promotions)
            var allVouchersResp = await _apiService.GetAsync<List<JsonElement>>("/api/v1/promotions");
            ViewBag.AllVouchers = allVouchersResp.Success ? allVouchersResp.Data : null;

            // Call the API to get user's saved vouchers
            var userVouchersResp = await _apiService.GetAsync<List<JsonElement>>("/api/v1/promotions/my");
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
                    return Json(new { success = true, message = "Đã đổi voucher thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = response.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi đổi voucher. Vui lòng thử lại." });
            }
        }
    }

    public class SaveUserPromotionRequest
    {
        public Guid? PromotionId { get; set; }
    }
} 