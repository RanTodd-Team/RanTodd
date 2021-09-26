using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
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
            string folder = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Ranker");
            else
                folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".ranker");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(File.ReadAllText("config.json"));
            IDatabase database = new SQLiteDatabase(Path.Combine(folder, "Ranker.db"));

            DiscordConfiguration configuration = new()
            {
                // We may drop JSON token.
#pragma warning disable CS0612 // Type or member is obsolete
                Token = Environment.GetEnvironmentVariable("RANKER_TOKEN") ?? configJson.Token,
#pragma warning restore CS0612 // Type or member is obsolete
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.GuildMembers | DiscordIntents.GuildMessages | DiscordIntents.Guilds
            };

            DiscordClient client = new(configuration);

            client.AddExtension(new MessageEvent(database));

            ServiceCollection servCollection = new();
            servCollection.AddSingleton(database);

            var slashCommands = client.UseSlashCommands(new()
            {
                Services = servCollection.BuildServiceProvider()
            });

            slashCommands.SlashCommandErrored += (s, e) =>
            {
                s.Client.Logger.LogError(e.Exception.ToString());
                return Task.CompletedTask;
            };

            slashCommands.RegisterCommands<Commands>(configJson.GuildId);

            client.Ready += (s, e) =>
            {
                Console.WriteLine("Ready!");
                return Task.CompletedTask;
            };

            await client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
