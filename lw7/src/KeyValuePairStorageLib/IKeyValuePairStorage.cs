using StackExchange.Redis;

namespace KeyValuePairStorageLib
{
	public interface IKeyValuePairStorage
	{
		IDatabase GetDatabase(string key, out int databaseId);
	}
}
