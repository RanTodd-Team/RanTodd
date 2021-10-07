using Microsoft.AspNetCore.Mvc;
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
    public class TokenController : Controller
    {

        ConfigJson configJson;

        public TokenController(ConfigJson config)
        {
            configJson = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                using (HttpClient client = new())
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>() {
                        { "client_id", configJson.ClientId },
                        { "client_secret", Environment.GetEnvironmentVariable("RANKER_CLIENT_SECRET") },
                        { "grant_type", "authorization_code" },
                        { "code", Request.Headers["Code"] },
                        { "redirect_uri", configJson.Domain + "/callback" },
                        { "scope", "identify guilds" }
                    };

                    var response = await client.PostAsync($"https://discord.com/api/v9/oauth2/token", new FormUrlEncodedContent(dictionary));
                    string responseJson = await response.Content.ReadAsStringAsync();
                    return Ok(responseJson);
                }
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
