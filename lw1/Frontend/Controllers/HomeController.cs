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

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string data)
        {
            // TODO: send data in POST request to backend and read returned id value from response
            var dictionary = new Dictionary<string, string>()
            {
                // data can be null but it's better to have empty string i think
                { "data", (data == null) ? "" : data },
            };
            var content = new FormUrlEncodedContent(dictionary);
            var response = await httpClient.PostAsync("http://127.0.0.1:5000/api/values", content);
            var result = await response.Content.ReadAsStringAsync();
            return Ok(result);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
