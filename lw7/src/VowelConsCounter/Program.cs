using System;
using System.Collections.Generic;
using StackExchange.Redis;
using MessageQueueLib;

namespace VowelConsCounter
{
	class Program
	{
		private static readonly IMessageBroker m_broker = new MessageBroker();

		private static readonly HashSet<char> VOWELS = new HashSet<char>
		{
			'a', 'e', 'i', 'o', 'u', 'y'
		};

		private static readonly HashSet<char> CONSONANTS = new HashSet<char>
		{
			'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n',
			'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z'
		};

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

		private static void CountVowelsAndConsonants(string text, out int vowels, out int consonants)
		{
			int vowelsCount = 0;
			int consonantsCount = 0;

			foreach (char ch in text)
			{
				if (VOWELS.Contains(Char.ToLower(ch)))
				{
					++vowelsCount;
				}
				else if (CONSONANTS.Contains(Char.ToLower(ch)))
				{
					++consonantsCount;
				}
			}

			vowels = vowelsCount;
			consonants = consonantsCount;
		}

		public static void Main(string[] args)
		{
			m_broker.DeclareExchange("vowels-cons-counter", ExchangeType.Direct); // для отправки
			m_broker.DeclareExchange("text-rank-tasks", ExchangeType.Direct); // для получения

			m_broker.DeclareQueue("count-task");
			m_broker.BindQueue(
				queueName: "count-task",
				exchangeName: "text-rank-tasks",
				routingKey: "text-rank-task");

			m_broker.BeginConsume("count-task", (string message) =>
			{
				Console.WriteLine(message);
				var tokens = message.Split(":");
				if (tokens.Length == 2 && tokens[0] == "TextRankTask")
				{
					int databaseId = CalculateDatabaseId(tokens[1]);
					IDatabase database = RedisConnection.GetDatabase(databaseId);
					string text = database.StringGet("TextContent:" + tokens[1]);
					Console.WriteLine("Redis database get #" + databaseId + ": " + tokens[1]);

					CountVowelsAndConsonants(text ?? "", out int vowels, out int consonants);
					m_broker.Publish(
						message: "VowelConsCounted:" + tokens[1] + ":" + vowels + ":" + consonants,
						exchangeName: "vowel-cons-counter",
						routingKey: "vowel-cons-task");
				}
			});

			Console.ReadLine();
		}
	}
}
