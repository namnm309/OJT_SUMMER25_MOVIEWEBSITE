using Microsoft.AspNetCore.Mvc;

namespace UI.Controllers
{
    public class TestController : Controller
    {
        public IActionResult TestMovieCard()
        {
            return View();
        }
    }
} 