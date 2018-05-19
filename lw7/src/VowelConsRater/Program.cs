using System;
using System.Text;
using StackExchange.Redis;
using MessageQueueLib;

namespace VowelConsRater
{
	class Program
	{
		private static readonly IMessageBroker m_broker = new MessageBroker();

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

		public static void Main(string[] args)
		{
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

					int databaseId = CalculateDatabaseId(tokens[1]);
					IDatabase database = RedisConnection.GetDatabase(databaseId);
					database.StringSet("TextRank:" + tokens[1], rank.ToString());
					Console.WriteLine("Redis database set #" + databaseId + ": " + tokens[1]);
				}
			});

			Console.ReadLine();
		}
	}
}
