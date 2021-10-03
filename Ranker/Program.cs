using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Ranker
{
    public class Program
    {
        static void Main(string[] args)
        {
            BuildWebHost(args);
        }

        static void BuildWebHost(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
