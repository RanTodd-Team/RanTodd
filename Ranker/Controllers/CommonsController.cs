using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ranker
{
    [Route("[controller]")]
    public class CommonsController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            Request.Headers["Code"]);
                    var response = await client.GetAsync("https://discord.com/api/v9/users/@me/guilds");
                    string responseJson = await response.Content.ReadAsStringAsync();
                    JArray jsonParsed = JArray.Parse(responseJson);

                    using (HttpClient client2 = new())
                    {
                        client2.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue(
                                "Bot",
                                Environment.GetEnvironmentVariable("RANKER_TOKEN"));
                        response = await client2.GetAsync("https://discord.com/api/v9/users/@me/guilds");
                        responseJson = await response.Content.ReadAsStringAsync();
                        JArray jsonParsed2 = JArray.Parse(responseJson);

                        var ready = jsonParsed.Intersect(jsonParsed2);

                        return Ok(JsonConvert.SerializeObject(ready));
                    }

                }
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
