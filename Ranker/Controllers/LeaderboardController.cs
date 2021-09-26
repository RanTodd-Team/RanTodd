using Microsoft.AspNetCore.Mvc;

namespace Ranker.Controllers
{
    public class LeaderboardController : Controller
    {
        [HttpGet("leaderboard/{id}")]
        public IActionResult Index(string id)
        {
            ViewBag.id = id;
            return View();
        }
    }
}
