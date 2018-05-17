using System;
using System.Text;
using System.Collections.Generic;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace VowelConsCounter
{
	class Program
	{
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
			Console.WriteLine("VowelConsCounter");

			var factory = new ConnectionFactory() { HostName = "localhost" };
			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					channel.ExchangeDeclare("text-rank-tasks", ExchangeType.Direct);

					channel.QueueDeclare(
						queue: "count-task",
						durable: false,
						exclusive: false,
						autoDelete: false,
						arguments: null);
					channel.QueueBind(
						queue: "count-task",
						exchange: "text-rank-tasks",
						routingKey: "text-rank-task");

					channel.ExchangeDeclare("vowel-cons-counter", ExchangeType.Direct);

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, ea) => {
						var body = ea.Body;
						var message = Encoding.UTF8.GetString(body);
						var tokens = message.Split(':');

						Console.WriteLine(message);

						if (tokens.Length == 2 && tokens[0] == "TextRankTask")
						{
							int databaseId = CalculateDatabaseId(tokens[1]);
							IDatabase database = RedisConnection.GetDatabase(databaseId);
							string text = database.StringGet("TextContent:" + tokens[1]);
							Console.WriteLine("Redis database get #" + databaseId + ": " + tokens[1]);

							CountVowelsAndConsonants(text ?? "", out int vowels, out int consonants);

							channel.BasicPublish(
								exchange: "vowel-cons-counter",
								routingKey: "vowel-cons-task",
								basicProperties: null,
								body: Encoding.UTF8.GetBytes("VowelConsCounted:" + tokens[1] + ":" + vowels + ":" + consonants));
						}
					};

					channel.BasicConsume(
						queue: "count-task",
						autoAck: true,
						consumer: consumer);

					Console.ReadLine();
				}
			}
		}
	}
}
