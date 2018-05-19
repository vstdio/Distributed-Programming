using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageQueueLib
{
	public class MessageBroker : IMessageBroker
	{
		private readonly ConnectionFactory m_factory;
		private readonly IConnection m_connection;
		private readonly IModel m_channel;

		public MessageBroker()
		{
			m_factory = new ConnectionFactory();
			m_connection = m_factory.CreateConnection();
			m_channel = m_connection.CreateModel();
		}

		public void BeginConsume(string queueName, Action<string> onRecieveCallback)
		{
			var consumer = new EventingBasicConsumer(m_channel);

			consumer.Received += (model, ea) =>
			{
				onRecieveCallback(Encoding.UTF8.GetString(ea.Body));
			};

			m_channel.BasicConsume(
				queue: queueName,
				autoAck: true,
				consumer: consumer);
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
