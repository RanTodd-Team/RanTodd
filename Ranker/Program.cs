using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                Token = Environment.GetEnvironmentVariable("RANKER_TOKEN"),
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

            slashCommands.SlashCommandErrored += async (s, e) =>
            {
                s.Client.Logger.LogError(e.Exception.ToString());

                bool botOwner = s.Client.CurrentApplication.Owners.Any(x => x.Id == e.Context.User.Id);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithTitle("Error")
                    .WithColor(DiscordColor.Red)
                    .WithDescription(botOwner ? $"```{e.Exception}```" : "Please contact the bot owner to get more information.");

                await e.Context.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("Something went wrong!")
                    .AddEmbed(embed));
            };

            slashCommands.RegisterCommands<SlashCommands>(configJson.GuildId);

            var commands = client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = configJson.Prefixes,
                Services = servCollection.BuildServiceProvider()
            });

            commands.RegisterCommands<NormalCommands>();

            client.Ready += (s, e) =>
            {
                s.Logger.LogInformation("Bot is ready!");
                return Task.CompletedTask;
            };

            await client.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
