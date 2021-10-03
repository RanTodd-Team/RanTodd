using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ranker
{
    public class Startup
    {
        const string Cors = "_cors";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: Cors,
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin();
                                  });
            });

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

            services.AddSingleton(database);
            services.AddSingleton(configJson);

            services.AddHostedService<BotService>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(Cors);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
