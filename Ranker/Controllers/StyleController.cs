using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ranker
{
    [Route("[controller]")]
    public class StyleController : Controller
    {
        private readonly IDatabase _database;

        public StyleController(IDatabase database)
        {
            _database = database;
        }

        [HttpGet("{guildId}")]
        public async Task<IActionResult> Get(ulong guildId)
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            Request.Headers["Code"]);
                    var response = await client.GetAsync("https://discord.com/api/v9/users/@me");
                    string responseJson = await response.Content.ReadAsStringAsync();
                    JObject jsonParsed = JObject.Parse(responseJson);
                    ulong userId = ulong.Parse(jsonParsed["id"].Value<string>());
                    Rank rankCard = await _database.GetAsync(userId, guildId);
                    return Ok(new Dictionary<string, bool>(){
                        { "fleuron", rankCard.Fleuron }
                    });
                }
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpPost("{guildId}")]
        public async Task<IActionResult> Post(ulong guildId)
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            Request.Headers["Code"]);
                    var response = await client.GetAsync("https://discord.com/api/v9/users/@me");
                    string responseJson = await response.Content.ReadAsStringAsync();
                    JObject jsonParsed = JObject.Parse(responseJson);
                    ulong userId = ulong.Parse(jsonParsed["id"].Value<string>());
                    Rank rankCard = await _database.GetAsync(userId, guildId);
                    using (StreamReader stream = new StreamReader(Request.Body))
                    {
                        rankCard.Fleuron = JObject.Parse(await stream.ReadToEndAsync())["fleuron"].Value<bool>();
                        await _database.UpsertAsync(userId, guildId, rankCard);
                        return Ok(new Dictionary<string, bool>(){
                        { "fleuron", rankCard.Fleuron }
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
