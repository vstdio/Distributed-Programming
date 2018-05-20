using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Collections.Generic;

namespace Frontend.Controllers
{
	public class StatisticsController : Controller
	{
		public IActionResult Index()
		{
			Task<string> statistics = SendGetRequest("http://localhost:5000/api/values/statistics");
			var tokens = statistics.Result.Split(":");
			ViewData["Message"] = new List<string>()
			{
				"Кол-во обработанных текстов: " + tokens[0],
				"Кол-во текстов с оценкой от 0.5: " + tokens[1],
				"Средняя оценка: " + tokens[2]
			};
			return View();
		}

		private async Task<string> SendGetRequest(string url)
		{
			using (var httpClient = new HttpClient())
			{
				using (var response = await httpClient.GetAsync(url))
				{
					return await response.Content.ReadAsStringAsync();
				}
			}
		}
	}
}
