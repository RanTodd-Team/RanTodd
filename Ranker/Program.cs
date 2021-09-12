using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SQLite;

public class Rank
{
    [PrimaryKey]
    public string Id { get; set; }
    public string UserId { get; set; }
    public string GuildId { get; set; }
    public string TimeR { get; set; }
    public string Messasges { get; set; }
    public string LevelXp { get; set; }
    public string Level { get; set; }
    public string Xp { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public string Avatar { get; set; }
}

namespace Ranker
{
    public class Program
    {
        public static Dictionary<string, List<string>> JSON = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(System.IO.File.ReadAllText("config.json"));
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
