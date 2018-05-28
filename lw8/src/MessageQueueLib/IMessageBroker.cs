using System;

namespace MessageQueueLib
{
	public interface IMessageBroker
	{
		void DeclareExchange(string name, string type);
		string DeclareQueue(string name = "");
		void BindQueue(string queueName, string exchangeName, string routingKey);
		void Publish(string message, string exchangeName, string routingKey = "");
		void BeginConsume(string queueName, Action<string> onRecieveCallback);
	}
}
