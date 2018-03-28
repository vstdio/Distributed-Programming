using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using StackExchange.Redis;

namespace TextListener
{
	class Program
	{
		private static string GetValueFromRedis(string id)
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
			IDatabase database = redis.GetDatabase();
			return database.StringGet(id);
		}

		private static void EnterListeningLoop()
		{
			ConnectionFactory factory = new ConnectionFactory();
			using (IConnection connection = factory.CreateConnection())
			{
				using (IModel channel = connection.CreateModel())
				{
					channel.QueueDeclare("hello", false, false, false, null);
					var consumer = new QueueingBasicConsumer(channel);
					channel.BasicConsume("hello", true, consumer);

					Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");
					while (true)
					{
						var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

						var body = ea.Body;
						var message = Encoding.UTF8.GetString(body);
						Console.WriteLine(" [x] Received {0}", GetValueFromRedis(message));
					}
				}
			}
		}

		static void Main(string[] args)
		{
			try
			{
				EnterListeningLoop();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
