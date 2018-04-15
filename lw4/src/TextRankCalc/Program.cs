using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Collections.Generic;

namespace TextRankCalc
{
	class Program
	{
		private static HashSet<char> VOWELS = new HashSet<char>
		{
			'a', 'e', 'i', 'o', 'u', 'y'
		};

		private static HashSet<char> CONSONANTS = new HashSet<char>
		{
			'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n',
			'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z'
		};

		private static string GetValueFromRedis(string key)
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
			IDatabase database = redis.GetDatabase();
			return database.StringGet(key);
		}

		private static void SaveDataToRedis(string id, string value)
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
			IDatabase database = redis.GetDatabase();
			database.StringSet("TextRankGuid_" + id, value);
		}

		private static void CalculateTextRankAndInsertDataToRedis(string id, string text)
		{
			if (text == null)
			{
				SaveDataToRedis(id, "Сообщение пустое");
				return;
			}

			int vowels = 0;
			int consonants = 0;

			foreach (char ch in text)
			{
				if (VOWELS.Contains(Char.ToLower(ch)))
				{
					++vowels;
				}
				else if (CONSONANTS.Contains(Char.ToLower(ch)))
				{
					++consonants;
				}
			}

			if (consonants != 0)
			{
				float rank = (float)vowels / consonants;
				SaveDataToRedis(id, "Отношение гласных к согласным: " + rank.ToString());
			}
			else
			{
				SaveDataToRedis(id, "В тексте нет согласных");
			}
		}

		private static void OnMessageCallback(object model, BasicDeliverEventArgs eventArgs)
		{
			var body = eventArgs.Body;
			var message = Encoding.UTF8.GetString(body);
			Console.WriteLine("[x] Received {0}", message);

			var tokens = message.Split("_");
			if (tokens.Length == 2 && tokens[0] == "TextCreated")
			{
				string id = tokens[1];
				string text = GetValueFromRedis("TextContentGuid_" + id);
				CalculateTextRankAndInsertDataToRedis(id, text);
			}
		}

		public static void Main(string[] args)
		{
			var factory = new ConnectionFactory() { HostName = "localhost" };
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

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += OnMessageCallback;

					channel.BasicConsume(
						queue: "backend-api",
						autoAck: true,
						consumer: consumer);

					Console.WriteLine("Waiting for messages...");
					Console.WriteLine("Press any key to exit.");
					Console.ReadLine();
				}
			}
		}
	}
}
