namespace RanTodd
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
                        { "client_secret", Environment.GetEnvironmentVariable("RANTODD_CLIENT_SECRET") },
                        { "grant_type", "authorization_code" },
                        { "code", Request.Headers["Code"] },
                        { "redirect_uri", configJson.Domain + "/callback" },
                        { "scope", "identify guilds" }
                    };

                    using (FormUrlEncodedContent requestContent = new FormUrlEncodedContent(dictionary)) {
                        var response = await client.PostAsync($"https://discord.com/api/v9/oauth2/token", requestContent);
                        string responseJson = await response.Content.ReadAsStringAsync();
                        return Ok(responseJson);
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
