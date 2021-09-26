using Microsoft.AspNetCore.Mvc;

namespace Ranker.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly IDatabase _database;

        public LeaderboardController(IDatabase database)
        {
            _database = database;
        }

        [HttpGet("leaderboard/{id}")]
        public IActionResult Index(string id)
        {
            ViewBag.id = id;
            ViewBag.database = _database;
            return View();
        }
    }
}
