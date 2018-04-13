using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Frontend
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\config\")))
				.AddJsonFile("frontend.json", optional: false)
				.Build();

			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.UseConfiguration(config)
				.Build()
				.Run();
		}
	}
}
