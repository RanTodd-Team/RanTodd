namespace RanTodd
{
    [Route("[controller]")]
    public class CommonsController : Controller
    {
        ConfigJson configJson;

        public CommonsController(ConfigJson config)
        {
            configJson = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                if (!string.IsNullOrEmpty(Request.Headers["Code"])) { 
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
                                    Environment.GetEnvironmentVariable("RANTODD_TOKEN"));
                            response = await client2.GetAsync("https://discord.com/api/v9/users/@me/guilds");
                            responseJson = await response.Content.ReadAsStringAsync();
                            JArray jsonParsed2 = JArray.Parse(responseJson);

                            JArray ready = jsonParsed;

                            jsonParsed.ToList().Select(x => x["id"].Value<string>()).ToList().Except(jsonParsed2.ToList().Select(x => x["id"].Value<string>()).ToList()).ToList().ForEach(nony =>
                            {
                                int index = ready.ToList().FindIndex(x => x["id"].Value<string>() == nony);
                                if (index > -1)
                                {
                                    ready.RemoveAt(index);
                                }
                            });

                            return Ok(JsonConvert.SerializeObject(ready));
                        }
                    }
            } else
                {
                    using (HttpClient client = new())
                    {
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue(
                                "Bot",
                                    Environment.GetEnvironmentVariable("RANTODD_TOKEN"));
                        var response = await client.GetAsync("https://discord.com/api/v9/guilds/" + configJson.GuildId.ToString());
                        string responseJson = await response.Content.ReadAsStringAsync();
                        JObject jsonParsed = JObject.Parse(responseJson);

                        return Ok(new Dictionary<string, string>(){
                            { "id", jsonParsed["id"].Value<string>() },
                            { "name", jsonParsed["name"].Value<string>() },
                            { "icon", jsonParsed["icon"].Value<string>() },
                        });

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
