using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ranker
{
    [Route("[controller]")]
    public class LevelsController : Controller
    {
        private readonly IRankerRepository _database;

        public LevelsController(IRankerRepository database)
        {
            _database = database;
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(ulong id, [FromQuery]int page = 0)
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bot",
                            Environment.GetEnvironmentVariable("RANKER_TOKEN"));
                    var response = await client.GetAsync($"https://discord.com/api/v9/guilds/{id}");
                    string responseJson = await response.Content.ReadAsStringAsync();
                    JToken jsonParsed = JToken.Parse(responseJson);
                    IEnumerable<Rank> ranks = (await _database.Ranks.GetAsync(id))
                        .OrderByDescending(f => f.TotalXp)
                        .Skip(page * 100)
                        .Take(100);

                    List<Role> roles = await _database.Roles.GetAsync(id); 

                    return Ok(new Dictionary<string, object>()
                    {
                        {
                            "guild",
                            new Dictionary<string, object>()
                            {
                                { "name", jsonParsed["name"].Value<string>() },
                                { "description", jsonParsed["description"].Value<string>() },
                                { "icon", jsonParsed["icon"].Value<string>() },
                                { "is_joinable", jsonParsed["rules_channel_id"] != null }
                            }
                        },
                        { "roles", roles.OrderBy(x => x.Level) },
                        { "players", ranks }
                    });
                }
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
