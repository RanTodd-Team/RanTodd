using Microsoft.AspNetCore.Mvc;

namespace Ranker.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
