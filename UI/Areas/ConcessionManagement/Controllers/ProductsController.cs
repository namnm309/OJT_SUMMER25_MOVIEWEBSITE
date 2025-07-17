using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UI.Areas.ConcessionManagement.Controllers
{
    [Area("ConcessionManagement")]
    public class ProductsController : Controller
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly UI.Areas.ConcessionManagement.Services.IConcessionManagementUIService _service;

        public ProductsController(UI.Areas.ConcessionManagement.Services.IConcessionManagementUIService service, ILogger<ProductsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var items = await _service.GetAllConcessionItemsAsync();
                return Json(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting concession items");
                return StatusCode(500, "Đã xảy ra lỗi khi tải dữ liệu");
            }
        }
    }
}
