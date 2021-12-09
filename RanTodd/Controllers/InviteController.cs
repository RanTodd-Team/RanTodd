using System.Text;

namespace RanTodd.Controllers
{
    [Route("[controller]")]
    public class InviteController : Controller
    {
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(ulong id)
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bot",
                            Environment.GetEnvironmentVariable("RANTODD_TOKEN"));
                    var response = await client.GetAsync($"https://discord.com/api/v9/guilds/{id}");
                    string responseJson = await response.Content.ReadAsStringAsync();
                    JToken jsonParsed = JToken.Parse(responseJson);

                    string rulesChannel = jsonParsed["rules_channel_id"].Value<string>();

                    if (rulesChannel == null)
                        throw new Exception("No rules channel to create invites for.");

                    using (StringContent requestContent = new("{ \"max_uses\": 1, \"unique\": true }", Encoding.UTF8, "application/json"))
                    {
                        response = await client.PostAsync($"https://discord.com/api/v9/channels/{rulesChannel}/invites", requestContent);
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            // We aren't authorized to create an invite.
                            return Unauthorized();
                        }

                        responseJson = await response.Content.ReadAsStringAsync();

                        var code = JToken.Parse(responseJson)["code"].Value<string>();
                        return Ok(
                            new Dictionary<string, string>()
                            {
                                { "url", $"https://discord.gg/{code}" }
                            }
                        );
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
