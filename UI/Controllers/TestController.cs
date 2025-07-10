using Microsoft.AspNetCore.Mvc;
using UI.Services;

namespace UI.Controllers
{
    public class TestController : Controller
    {
        private readonly IApiService _apiService;

        public TestController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Migration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RunMigration()
        {
            try
            {
                var result = await _apiService.PostAsync<object>("api/v1/cinemaroom/migration/add-layout-columns", null);
                ViewBag.Result = "Migration completed successfully!";
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Result = $"Migration failed: {ex.Message}";
                ViewBag.Success = false;
            }

            return View("Migration");
        }
    }
} 