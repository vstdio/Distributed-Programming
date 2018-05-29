using System;
using MessageQueueLib;

namespace TextSuccessMarker
{
	class Program
	{
		private static readonly IMessageBroker m_broker = new MessageBroker();
		private static readonly float MIN_SUCCESS_VALUE = 0.5f;

		private static void Main(string[] args)
		{
			m_broker.DeclareExchange("text-success-marker", ExchangeType.Fanout); // output exchange
			m_broker.DeclareExchange("text-rank-calc", ExchangeType.Fanout); // input exchange

			string queueName = m_broker.DeclareQueue();
			m_broker.BindQueue(
				queueName: queueName,
				exchangeName: "text-rank-calc",
				routingKey: "");

			m_broker.BeginConsume(queueName, (string message) => {
				string[] tokens = message.Split(":");
				if (tokens.Length == 3 && tokens[0] == "TextRankCalculated")
				{
					float rank = float.Parse(tokens[2]);
					if (rank >= MIN_SUCCESS_VALUE)
					{
						Console.WriteLine("Success mark: " + tokens[1]);
						m_broker.Publish(
							message: "TextSuccessMarked:" + tokens[1] + ":true",
							exchangeName: "text-success-marker");
					}
					else
					{
						Console.WriteLine("Unsuccess mark: " + tokens[1]);
						m_broker.Publish(
							message: "TextSuccessMarked:" + tokens[1] + ":false",
							exchangeName: "text-success-marker");
					}
				}
			});
		}
	}
}
