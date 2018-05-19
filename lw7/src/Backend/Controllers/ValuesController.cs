using System;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

using StackExchange.Redis;

using MessageQueueLib;

namespace Backend.Controllers
{
	[Route("api/[controller]")]
	public class ValuesController : Controller
	{
		private static readonly IMessageBroker m_broker = new MessageBroker();

		private static ConnectionMultiplexer RedisConnection => ConnectionMultiplexer.Connect("localhost");
		private static readonly int DATABASE_COUNT = 16;

		public ValuesController()
		{
			m_broker.DeclareExchange("backend-api", ExchangeType.Fanout);
		}

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
		public string Post([FromForm]string data)
		{
			string contextId = Guid.NewGuid().ToString();
			try
			{
				int databaseId = CalculateDatabaseId(contextId);
				Console.WriteLine("Redis database set #" + databaseId + ": " + contextId);

				IDatabase database = RedisConnection.GetDatabase(databaseId);
				database.StringSet("TextContent:" + contextId, data);

				m_broker.Publish("TextCreated:" + contextId, "backend-api");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return contextId;
		}
	}
}
