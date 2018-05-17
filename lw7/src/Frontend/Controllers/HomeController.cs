using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http;

namespace Frontend.Controllers
{
	public class HomeController : Controller
	{
		private static readonly HttpClient httpClient = new HttpClient();

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult TextDetails(string id)
		{
			string details = SendGetRequest("http://127.0.0.1:5000/api/values/" + id).Result;
			ViewData["Message"] = details != float.MaxValue.ToString() ? details : "В тексте нет согласных";
			return View();
		}

		[HttpGet]
		public IActionResult Upload()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Upload(string data)
		{
			var dictionary = new Dictionary<string, string>()
			{
				{ "data", data ?? "" },
			};

			var content = new FormUrlEncodedContent(dictionary);
			var response = await httpClient.PostAsync("http://127.0.0.1:5000/api/values", content);
			var result = await response.Content.ReadAsStringAsync();

			return new RedirectResult("http://127.0.0.1:5001/Home/TextDetails/" + result);
		}

		private async Task<string> SendGetRequest(string requestUri)
		{
			var response = await httpClient.GetAsync(requestUri);
			string value = await response.Content.ReadAsStringAsync();
			if (response.IsSuccessStatusCode && value != null)
			{
				return value;
			}
			return response.StatusCode.ToString();
		}
	}
}
