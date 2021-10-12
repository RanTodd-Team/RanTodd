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

                    Settings settings = await _database.Settings.GetAsync(id);

                    List<Role> roles = await _database.Roles.GetAsync(id);

                    using (HttpClient client2 = new())
                    {
                        bool managesGuild = false;

                        if (!String.IsNullOrEmpty(Request.Headers["Code"]))
                        {
                            client2.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue(
                                    "Bearer",
                                    Request.Headers["Code"]);
                            var response2 = await client2.GetAsync("https://discord.com/api/v9/users/@me/guilds");
                            string responseJson2 = await response2.Content.ReadAsStringAsync();
                            JArray jsonParsed2 = JArray.Parse(responseJson2);

                            managesGuild = (ulong.Parse(jsonParsed2.ToList().Find(x => x["id"].Value<string>() == id.ToString())["permissions"].Value<string>()) & 0x0000000020) == 0x0000000020;
                        } 
                        return Ok(new Dictionary<string, object>()
                    {
                            { "managesGuild", managesGuild },
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
                        { "settings", new Dictionary<string, object>() {
                            { "banner", settings.Banner },
                            { "minRange", settings.MinRange },
                            { "maxRange", settings.MaxRange }
                        }},
                        { "roles", roles.OrderBy(x => x.Level) },
                        { "players", ranks }
                    });
                    }
                }
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
