using System;
using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace VowelConsRater
{
	class Program
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

		public static void Main(string[] args)
		{
			Console.WriteLine("VowelConsRater");
			var factory = new ConnectionFactory() { HostName = "localhost" };
			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					channel.ExchangeDeclare("vowel-cons-counter", ExchangeType.Direct);

					channel.QueueDeclare(
						queue: "rank-task",
						durable: false,
						exclusive: false,
						autoDelete: false,
						arguments: null);
					channel.QueueBind(
						queue: "rank-task",
						exchange: "vowel-cons-counter",
						routingKey: "vowel-cons-task");

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, ea) =>
					{
						var body = ea.Body;
						var message = Encoding.UTF8.GetString(body);
						var splitted = message.Split(':');

						Console.WriteLine(message);

						if (splitted.Length == 4 && splitted[0] == "VowelConsCounted")
						{
							int vowels = Int32.Parse(splitted[2]);
							int consonants = Int32.Parse(splitted[3]);
							float rank = (consonants != 0) ? (float)vowels / (float)consonants : float.MaxValue;

							int databaseId = CalculateDatabaseId(splitted[1]);
							IDatabase database = RedisConnection.GetDatabase(databaseId);
							database.StringSet("TextRank:" + splitted[1], rank.ToString());
							Console.WriteLine("Redis database set #" + databaseId + ": " + splitted[1]);
						}
					};

					channel.BasicConsume(
						queue: "rank-task",
						autoAck: true,
						consumer: consumer);

					Console.ReadLine();
				}
			}
		}
	}
}
