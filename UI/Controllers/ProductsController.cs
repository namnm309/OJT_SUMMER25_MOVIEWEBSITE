using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sản Phẩm";
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewData["Title"] = "Chi tiết sản phẩm";
            ViewData["ProductId"] = id;
            return View();
        }

        public IActionResult Category(string category)
        {
            ViewData["Title"] = $"Danh mục: {category}";
            ViewData["Category"] = category;
            return View();
        }
    }
}
