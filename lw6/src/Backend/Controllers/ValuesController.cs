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

		private static int CalculateDatabaseId(string key)
		{
			int hash = 0;
			foreach (char ch in key)
			{
				hash += ch;
			}
			return hash % 16;
		}

		private void SaveKeyValuePairToRedis(string key, string value, int databaseId = 0)
		{
			IDatabase database = RedisConnection.GetDatabase(databaseId);
			database.StringSet(key, value);
		}

		[HttpGet("{id}")]
		public IActionResult Get(string id)
		{
			int databaseId = CalculateDatabaseId(id);
			Console.WriteLine("Redis database get #" + databaseId + ": " + id);
			IDatabase database = RedisConnection.GetDatabase(databaseId);
			for (short i = 0; i < 5; ++i)
			{
				string rank = database.StringGet("TextRank:" + id);
				if (rank == null)
				{
					Thread.Sleep(500);
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
				int databaseId = CalculateDatabaseId(id);
				Console.WriteLine("Redis database set #" + databaseId + ": " + id);
				SaveKeyValuePairToRedis("TextContent:" + id, holder.Data, databaseId);
				SendMessageToRabbitMQ("TextCreated:" + id);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return id;
		}
	}
}
