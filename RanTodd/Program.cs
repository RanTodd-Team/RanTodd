using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RanTodd;
using System;
using System.IO;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

const string Cors = "_cors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: Cors,
        builder =>
        {
            builder
            .AllowAnyOrigin()
            .AllowAnyHeader();
        });
});

string folder = "";

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RanTodd");
else
    folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".rantodd");

if (!Directory.Exists(folder))
{
    Directory.CreateDirectory(folder);
}

ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(File.ReadAllText("config.json"));
IRanToddRepository database = new SQLiteRanToddRepository(Path.Combine(folder, "RanTodd.db"));

builder.Services.AddSingleton(database);
builder.Services.AddSingleton(configJson);

builder.Services.AddHostedService<BotService>();

builder.Services.AddControllers();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
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