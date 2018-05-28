using System;
using StackExchange.Redis;
using MessageQueueLib;
using KeyValuePairStorageLib;

namespace TextStatistics
{
	public class Statistics
	{
		public int TextNum { get; set; } = 0;
		public int HighRankPart { get; set; } = 0;
		public float AvgRank { get; set; } = 0.0f;
		public float TotalRank { get; set; } = 0.0f;
	}

	class Program
	{
		private static readonly IMessageBroker m_broker = new MessageBroker();
		private static readonly IKeyValuePairStorage m_storage = new KeyValuePairStorage();

		private static void InitializeStatistics(Statistics statistics)
		{
			IDatabase database = m_storage.GetDatabase();
			var value = database.StringGet("statistics");
			if (value.HasValue)
			{
				Console.WriteLine("Redis database get #1: statistics");
				var tokens = value.ToString().Split(":");
				if (tokens.Length == 4)
				{
					statistics.TextNum = int.Parse(tokens[0]);
					statistics.HighRankPart = int.Parse(tokens[1]);
					statistics.AvgRank = float.Parse(tokens[2]);
					statistics.TotalRank = float.Parse(tokens[3]);
				}
			}
		}

		private static void UpdateStatistics(float newTextRank, Statistics statistics)
		{
			++statistics.TextNum;
			if (newTextRank >= 0.5f)
			{
				++statistics.HighRankPart;
			}
			statistics.TotalRank += newTextRank;
			statistics.AvgRank = statistics.TotalRank / statistics.TextNum;
		}

		public static void Main(string[] args)
		{
			Statistics statistics = new Statistics();
			InitializeStatistics(statistics);

			m_broker.DeclareExchange("text-rank-calc", ExchangeType.Fanout);
			m_broker.DeclareQueue("text-rank-calc");
			m_broker.BindQueue(
				queueName: "text-rank-calc",
				exchangeName: "text-rank-calc",
				routingKey: "");

			m_broker.BeginConsume("text-rank-calc", (string message) =>
			{
				Console.WriteLine(message);
				string[] tokens = message.Split(":");
				if (tokens.Length == 3 && tokens[0] == "TextRankCalculated")
				{
					UpdateStatistics(float.Parse(tokens[2]), statistics);
					IDatabase database = m_storage.GetDatabase();
					database.StringSet("statistics",
						statistics.TextNum + ":" + statistics.HighRankPart + ":" +
						statistics.AvgRank + ":" + statistics.TotalRank);
					Console.WriteLine("Redis database set #1: " + tokens[1]);
				}
			});

			Console.ReadLine();
		}
	}
}
