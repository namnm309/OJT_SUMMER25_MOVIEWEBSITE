using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(ILogger<MoviesController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Phim";
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewData["Title"] = "Chi tiết phim";
            ViewData["MovieId"] = id;
            return View();
        }

        public IActionResult Showtimes(int id)
        {
            ViewData["Title"] = "Lịch chiếu";
            ViewData["MovieId"] = id;
            return View();
        }
    }
}
