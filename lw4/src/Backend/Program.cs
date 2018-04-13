using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Backend
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\Config\")))
				.AddJsonFile("backend.json", optional: false)
				.Build();

			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.UseConfiguration(config)
				.Build()
				.Run();
		}
	}
}
