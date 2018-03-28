using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // https://stackoverflow.com/questions/42892173/hosting-json-available-options
            // https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/hosting?tabs=aspnetcore2x
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\config\")))
                .AddJsonFile("BackendConfig.json", optional: false)
                .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
