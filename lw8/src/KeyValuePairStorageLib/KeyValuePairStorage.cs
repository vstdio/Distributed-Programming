using StackExchange.Redis;

namespace KeyValuePairStorageLib
{
	public class KeyValuePairStorage : IKeyValuePairStorage
	{
		private static readonly int DATABASE_COUNT = 16;
		private ConnectionMultiplexer RedisConnection => ConnectionMultiplexer.Connect("localhost");

		public IDatabase GetDatabase(string key, out int databaseId)
		{
			databaseId = CalculateDatabaseId(key);
			return RedisConnection.GetDatabase(databaseId);
		}

		public IDatabase GetDatabase(int id = -1)
		{
			return RedisConnection.GetDatabase(id);
		}

		private int CalculateDatabaseId(string segmentationKey)
		{
			int databaseId = 0;
			foreach (char ch in segmentationKey)
			{
				databaseId += ch;
			}
			return databaseId % DATABASE_COUNT;
		}
	}
}
