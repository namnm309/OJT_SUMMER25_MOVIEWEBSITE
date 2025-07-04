using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class CinemasController : Controller
    {
        private readonly ILogger<CinemasController> _logger;

        public CinemasController(ILogger<CinemasController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Rạp Chiếu Phim";
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewData["Title"] = "Chi tiết rạp";
            ViewData["CinemaId"] = id;
            return View();
        }

        public IActionResult Rooms(int id)
        {
            ViewData["Title"] = "Phòng chiếu";
            ViewData["CinemaId"] = id;
            return View();
        }

        public IActionResult Location()
        {
            ViewData["Title"] = "Vị trí rạp";
            return View();
        }
    }
}
