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
		[HttpGet("{id}")]
		public IActionResult Get(string id)
		{
			IDatabase database = GetRedisDatabaseConnection();
			for (short i = 0; i < 5; ++i)
			{
				string rank = database.StringGet("TextRankGuid_" + id);
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
				SaveKeyValuePairToRedis("TextContentGuid_" + id, holder.Data);
				SendMessageToRabbitMQ("TextCreated_" + id);
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
					channel.QueueDeclare(
						queue: "backend-api",
						durable: false,
						exclusive: false,
						autoDelete: false,
						arguments: null);

					var bytes = Encoding.UTF8.GetBytes(message);
					channel.BasicPublish(
						exchange: "",
						routingKey: "backend-api",
						basicProperties: null,
						body: bytes);

					Console.WriteLine("Message: '" + message + "' sent to RabbitMQ");
				}
			}
		}

		private void SaveKeyValuePairToRedis(string key, string value)
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
			IDatabase database = redis.GetDatabase();
			database.StringSet(key, value);
			Console.WriteLine("Pair (" + key + ", " + value + ") saved to redis");
		}

		private static IDatabase GetRedisDatabaseConnection()
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
			return redis.GetDatabase();
		}
	}
}
