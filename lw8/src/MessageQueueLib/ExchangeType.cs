namespace MessageQueueLib
{
	public static class ExchangeType
	{
		public const string Fanout = RabbitMQ.Client.ExchangeType.Fanout;
		public const string Direct = RabbitMQ.Client.ExchangeType.Direct;
	}
}
