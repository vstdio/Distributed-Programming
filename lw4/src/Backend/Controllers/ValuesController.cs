using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Configuration;
using StackExchange.Redis;
using RabbitMQ.Client;
using System.Text;

namespace Backend.Controllers
{
	[Route("api/[controller]")]
	public class ValuesController : Controller
	{
		static readonly string _queueExchangeName = "backend-api";

		// GET api/values/<id>
		[HttpGet("{id}")]
		public string Get(string id)
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
			IDatabase database = redis.GetDatabase();
			return database.StringGet(id);
		}

		// POST api/values
		[HttpPost]
		public string Post([FromForm]Models.StringHolder holder)
		{
			string id = Guid.NewGuid().ToString();
			try
			{
				SaveKeyValuePairToRedis(id, holder.Data);
				SendMessageToRabbitMQ(id);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return id;
		}

		private void SendMessageToRabbitMQ(string message)
		{
			ConnectionFactory factory = new ConnectionFactory();
			using (IConnection connection = factory.CreateConnection())
			{
				using (IModel channel = connection.CreateModel())
				{
					channel.QueueDeclare(_queueExchangeName, false, false, false, null);
					var body = Encoding.UTF8.GetBytes(message);
					channel.BasicPublish("", _queueExchangeName, null, body);
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

		private static string GetValueFromRedis(string id)
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
			IDatabase database = redis.GetDatabase();
			return database.StringGet(id);
		}
	}
}
