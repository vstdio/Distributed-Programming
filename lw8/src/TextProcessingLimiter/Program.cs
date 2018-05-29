using System;
using MessageQueueLib;
using KeyValuePairStorageLib;
using StackExchange.Redis;

namespace TextProcessingLimiter
{
	class Program
	{
		private static readonly IMessageBroker m_broker = new MessageBroker();
		private static readonly IKeyValuePairStorage m_storage = new KeyValuePairStorage();

		private static void Main(string[] args)
		{
			int availableTextCount = 2;
			Console.WriteLine("Available text count: " + availableTextCount);

			m_broker.DeclareExchange("backend-api", ExchangeType.Fanout);
			m_broker.DeclareExchange("text-success-marker", ExchangeType.Fanout);
			m_broker.DeclareExchange("processing-limiter", ExchangeType.Fanout);

			string inputQueue = m_broker.DeclareQueue();
			m_broker.BindQueue(
				queueName: inputQueue,
				exchangeName: "backend-api",
				routingKey: "");

			string successQueue = m_broker.DeclareQueue();
			m_broker.BindQueue(
				queueName: successQueue,
				exchangeName: "text-success-marker",
				routingKey: "");

			m_broker.BeginConsume(inputQueue, (string message) => {
				var tokens = message.Split(":");
				if (tokens.Length == 2 && tokens[0] == "TextCreated")
				{
					IDatabase database = m_storage.GetDatabase(tokens[1], out int databaseId);
					if (availableTextCount > 0)
					{
						--availableTextCount;
						Console.WriteLine("Available text count: " + availableTextCount);
						m_broker.Publish(
							message: "ProcessingAccepted:" + tokens[1] + ":true",
							exchangeName: "processing-limiter");
					}
					else
					{
						Console.WriteLine("Limit reached");
						m_broker.Publish(
							message: "ProcessingAccepted:" + tokens[1] + ":false",
							exchangeName: "processing-limiter");
						database.StringSet("TextStatus:" + tokens[1], "rejected");
					}
				}
			});

			m_broker.BeginConsume(successQueue, (string message) => {
				var tokens = message.Split(":");
				if (tokens.Length == 3 && tokens[0] == "TextSuccessMarked")
				{
					if (tokens[2] == "true")
					{
						Console.WriteLine("Text marked as successful: " + tokens[1]);
					}
					else if (tokens[2] == "false")
					{
						Console.WriteLine("Text is unsuccessfull, rollback: " + tokens[1]);
						++availableTextCount;
					}
				}
			});
		}
	}
}
