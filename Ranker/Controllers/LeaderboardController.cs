using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ranker.Controllers
{
    public class LeaderboardController : Controller
    {
        [Route("leaderboard/{id}")]
        public IActionResult Index(string id)
        {
            ViewBag.id = id;
            return View();
        }
    }
}
