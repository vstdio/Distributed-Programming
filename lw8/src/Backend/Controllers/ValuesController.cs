using System;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using MessageQueueLib;
using KeyValuePairStorageLib;
using StackExchange.Redis;

namespace Backend.Controllers
{
	[Route("api/[controller]")]
	public class ValuesController : Controller
	{
		private static readonly IMessageBroker m_broker = new MessageBroker();
		private static readonly IKeyValuePairStorage m_storage = new KeyValuePairStorage();

		public ValuesController()
		{
			m_broker.DeclareExchange("backend-api", ExchangeType.Fanout);
		}

		[HttpGet("statistics")]
		public IActionResult GetStatistics()
		{
			IDatabase database = m_storage.GetDatabase();
			var value = database.StringGet("statistics");
			if (value.HasValue)
			{
				Console.WriteLine("Redis database get #1: statistics");
				return Ok(value.ToString());
			}
			return new NotFoundResult();
		}

		[HttpGet("{id}")]
		public IActionResult Get(string id)
		{
			IDatabase database = m_storage.GetDatabase(id, out int databaseId);
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

			return Ok("Превышен лимит проверок успешного текста");
		}

		[HttpPost]
		public string Post([FromForm]string data)
		{
			string contextId = Guid.NewGuid().ToString();
			try
			{
				IDatabase database = m_storage.GetDatabase(contextId, out int databaseId);
				database.StringSet("TextContent:" + contextId, data);
				Console.WriteLine("Redis database set #" + databaseId + ": " + contextId);

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
