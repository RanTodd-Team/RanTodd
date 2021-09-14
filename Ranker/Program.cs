using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SQLite;

public class JSON
{
    public string Token { get; set; }
    public List<string> Prefixes { get; set; }
    public string GitHub { get; set; }
    public ulong GuildId { get; set; }
    public Dictionary<string, ulong> Roles { get; set; }
    public bool Dm { get; set; }
}

public class Rank
{
    [PrimaryKey]
    public string Id { get; set; }
    public string TimeR { get; set; }
    public string Messasges { get; set; }
    public string LevelXp { get; set; }
    public string Level { get; set; }
    public string Xp { get; set; }
    public string NeededXp { get; set; }
    public string LastNeededXp { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public string Avatar { get; set; }
}

namespace Ranker
{
    public class Program
    {
        public static JSON JSON = JsonConvert.DeserializeObject<JSON>(System.IO.File.ReadAllText("config.json"));
        public static SQLiteConnection db = new SQLiteConnection(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Ranker.db"));


        public static void Main(string[] args)
        {
            db.CreateTable<Rank>();
            Bot.MainAsync(args).GetAwaiter().GetResult();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
