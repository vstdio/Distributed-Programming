using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageQueueLib
{
	public class MessageBroker : IMessageBroker
	{
		private readonly ConnectionFactory m_factory = null;
		private readonly IConnection m_connection = null;
		private readonly IModel m_channel = null;
		private readonly EventingBasicConsumer m_consumer = null;

		public MessageBroker()
		{
			m_factory = new ConnectionFactory();
			m_connection = m_factory.CreateConnection();
			m_channel = m_connection.CreateModel();
			m_consumer = new EventingBasicConsumer(m_channel);
		}

		public void BeginConsume(string queueName, Action<string> onRecieveCallback)
		{
			m_consumer.Received += (model, ea) =>
			{
				onRecieveCallback(Encoding.UTF8.GetString(ea.Body));
			};

			m_channel.BasicConsume(
				queue: queueName,
				autoAck: true,
				consumer: m_consumer);
		}

		public void BindQueue(string queueName, string exchangeName, string routingKey)
		{
			m_channel.QueueBind(
				queue: queueName,
				exchange: exchangeName,
				routingKey: routingKey);
		}

		public void DeclareExchange(string name, string type)
		{
			m_channel.ExchangeDeclare(
				exchange: name,
				type: type);
		}

		public string DeclareQueue(string name = "")
		{
			return m_channel.QueueDeclare(
				queue: name,
				exclusive: false,
				autoDelete: false,
				arguments: null).QueueName;
		}

		public void Publish(string message, string exchangeName, string routingKey = "")
		{
			m_channel.BasicPublish(
				exchange: exchangeName,
				routingKey: routingKey,
				basicProperties: null,
				body: Encoding.UTF8.GetBytes(message));
		}
	}
}
