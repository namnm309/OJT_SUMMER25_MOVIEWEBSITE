using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class EventsController : Controller
    {
        private readonly ILogger<EventsController> _logger;

        public EventsController(ILogger<EventsController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Sự Kiện";
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewData["Title"] = "Chi tiết sự kiện";
            ViewData["EventId"] = id;
            return View();
        }

        public IActionResult Calendar()
        {
            ViewData["Title"] = "Lịch sự kiện";
            return View();
        }
    }
}
