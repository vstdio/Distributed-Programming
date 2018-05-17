using System;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

using StackExchange.Redis;
using RabbitMQ.Client;

namespace Backend.Controllers
{
	[Route("api/[controller]")]
	public class ValuesController : Controller
	{
		private static ConnectionMultiplexer RedisConnection => ConnectionMultiplexer.Connect("localhost");
		private static readonly int DATABASE_COUNT = 16;

		private static int CalculateDatabaseId(string key)
		{
			int hash = 0;
			foreach (char ch in key)
			{
				hash += ch;
			}
			return hash % DATABASE_COUNT;
		}

		[HttpGet("{id}")]
		public IActionResult Get(string id)
		{
			int databaseId = CalculateDatabaseId(id);
			IDatabase database = RedisConnection.GetDatabase(databaseId);
			Console.WriteLine("Redis database get #" + databaseId + ": " + id);

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
			string contextId = Guid.NewGuid().ToString();
			try
			{
				int databaseId = CalculateDatabaseId(contextId);
				Console.WriteLine("Redis database set #" + databaseId + ": " + contextId);

				IDatabase database = RedisConnection.GetDatabase(databaseId);
				database.StringSet("TextContent:" + contextId, holder.Data);

				SendMessageToRabbitMQ("TextCreated:" + contextId);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return contextId;
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
	}
}
