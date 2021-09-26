using DSharpPlus;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Ranker
{
    public class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(File.ReadAllText("config.json"));
            SQLiteDatabase database = new SQLiteDatabase(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Ranker.db"));

            DiscordConfiguration configuration = new()
            {
                // We may drop JSON token.
                Token = Environment.GetEnvironmentVariable("RANKER_TOKEN") ?? configJson.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.GuildMembers | DiscordIntents.GuildMessages | DiscordIntents.Guilds
            };

            DiscordClient client = new(configuration);

            client.AddExtension(new MessageEvent(database));

            var slashCommands = client.UseSlashCommands();

            slashCommands.RegisterCommands<Commands>(configJson.GuildId);

            await client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
