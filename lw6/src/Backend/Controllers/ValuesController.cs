using System;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using RabbitMQ.Client;
using System.Text;
using System.Threading;

namespace Backend.Controllers
{
	[Route("api/[controller]")]
	public class ValuesController : Controller
	{
		private static ConnectionMultiplexer RedisConnection => ConnectionMultiplexer.Connect("localhost");

		[HttpGet("{id}")]
		public IActionResult Get(string id)
		{
			IDatabase database = RedisConnection.GetDatabase();
			for (short i = 0; i < 5; ++i)
			{
				string rank = database.StringGet("TextRank:" + id);
				if (rank == null)
				{
					Thread.Sleep(200);
				}
				else
				{
					return Ok(rank);
				}
			}
			return new NotFoundResult();
		}

		[HttpPost]
		public string Post([FromForm]Models.StringHolder holder)
		{
			string id = Guid.NewGuid().ToString();
			try
			{
				SaveKeyValuePairToRedis("TextContent:" + id, holder.Data);
				SendMessageToRabbitMQ("TextCreated:" + id);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return id;
		}

		private void SendMessageToRabbitMQ(string message)
		{
			var factory = new ConnectionFactory();
			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					channel.ExchangeDeclare(
						exchange: "backend-api",
						type: "fanout");
					
					channel.BasicPublish(
						exchange: "backend-api",
						routingKey: "",
						basicProperties: null,
						body: Encoding.UTF8.GetBytes(message));
				}
			}
		}

		private void SaveKeyValuePairToRedis(string key, string value)
		{
			IDatabase database = RedisConnection.GetDatabase();
			database.StringSet(key, value);
		}
	}
}
