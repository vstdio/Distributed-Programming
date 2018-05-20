using System;
using MessageQueueLib;
using KeyValuePairStorageLib;
using StackExchange.Redis;

namespace VowelConsRater
{
	class Program
	{
		private static readonly IMessageBroker m_broker = new MessageBroker();
		private static readonly IKeyValuePairStorage m_storage = new KeyValuePairStorage();

		public static void Main(string[] args)
		{
			m_broker.DeclareExchange("text-rank-calc", ExchangeType.Fanout);
			m_broker.DeclareExchange("vowel-cons-counter", ExchangeType.Direct);

			m_broker.DeclareQueue("rank-task");
			m_broker.BindQueue(
				queueName: "rank-task",
				exchangeName: "vowel-cons-counter",
				routingKey: "vowel-cons-task");

			m_broker.BeginConsume("rank-task", (string message) =>
			{
				Console.WriteLine(message);
				var tokens = message.Split(':');
				if (tokens.Length == 4 && tokens[0] == "VowelConsCounted")
				{
					int vowels = Int32.Parse(tokens[2]);
					int consonants = Int32.Parse(tokens[3]);
					float rank = (consonants != 0) ? (float)vowels / (float)consonants : float.MaxValue;

					IDatabase database = m_storage.GetDatabase(tokens[1], out int databaseId);
					database.StringSet("TextRank:" + tokens[1], rank.ToString());
					Console.WriteLine("Redis database set #" + databaseId + ": " + tokens[1]);

					// Оповещаем компонент TextStatistics
					m_broker.Publish(
						message: "TextRankCalculated:" + tokens[1] + ":" + rank,
						exchangeName: "text-rank-calc");
				}
			});

			Console.ReadLine();
		}
	}
}
