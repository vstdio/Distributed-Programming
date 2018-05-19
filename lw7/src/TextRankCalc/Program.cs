using System;
using MessageQueueLib;

namespace TextRankCalc
{
	class Program
	{
		private static readonly IMessageBroker m_broker = new MessageBroker();

		public static void Main(string[] args)
		{
			m_broker.DeclareExchange("backend-api", ExchangeType.Fanout);
			m_broker.DeclareExchange("text-rank-tasks", ExchangeType.Direct);

			string queueName = m_broker.DeclareQueue();
			m_broker.BindQueue(
				queueName: queueName,
				exchangeName: "backend-api",
				routingKey: "");

			m_broker.BeginConsume(queueName, (string message) =>
			{
				Console.WriteLine(message);
				var tokens = message.Split(":");
				if (tokens.Length == 2 && tokens[0] == "TextCreated")
				{
					m_broker.Publish(
						exchangeName: "text-rank-tasks",
						routingKey: "text-rank-task",
						message: "TextRankTask:" + tokens[1]);
				}
			});

			Console.ReadLine();
		}
	}
}
