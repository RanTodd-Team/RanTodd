
using Microsoft.AspNetCore.Mvc;
namespace Ranker.Controllers
{
    public class LeaderboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
