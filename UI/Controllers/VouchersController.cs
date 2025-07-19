using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using UI.Services;
using Microsoft.AspNetCore.Authorization;

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
            var resp = await _apiService.GetAsync<List<JsonElement>>("/api/v1/promotions");
            ViewBag.AllVouchers = resp.Success ? resp.Data : null;
            return View();
        }
    }
} 