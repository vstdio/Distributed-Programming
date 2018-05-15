using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TextRankCalc
{
	class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("TextRankCalc");

			var factory = new ConnectionFactory() { HostName = "localhost" };
			using (var connection = factory.CreateConnection())
			{
				using (var channel = connection.CreateModel())
				{
					channel.ExchangeDeclare("backend-api", ExchangeType.Fanout);
					channel.ExchangeDeclare("text-rank-tasks", ExchangeType.Direct);
					
					string queueName = channel.QueueDeclare().QueueName;
					channel.QueueBind(
						queue: queueName,
						exchange: "backend-api",
						routingKey: "");

					var consumer = new EventingBasicConsumer(channel);
					consumer.Received += (model, ea) => {
						var body = ea.Body;
						var message = Encoding.UTF8.GetString(body);

						Console.WriteLine(message);

						var splitted = message.Split(":");
						if (splitted.Length == 2 && splitted[0] == "TextCreated")
						{
							channel.BasicPublish(
								exchange: "text-rank-tasks",
								routingKey: "text-rank-task",
								basicProperties: null,
								body: Encoding.UTF8.GetBytes("TextRankTask:" + splitted[1]));
						}
					};

					channel.BasicConsume(
						queue: queueName,
						autoAck: true,
						consumer: consumer);

					Console.ReadLine();
				}
			}
		}
	}
}
