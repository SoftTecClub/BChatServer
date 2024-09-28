using BChatServer.Src.DB.Redis;
using StackExchange.Redis;

/// <summary>
/// Redisサービスクラス
/// </summary>
public class RedisService
{
    private readonly ConnectionMultiplexer _redis;
    private IDatabase _db;

    /// <summary>
    /// RedisServiceのコンストラクタ
    /// </summary>
    /// <param name="connectionString"></param>
    public RedisService(string connectionString)
    {
        _redis = ConnectionMultiplexer.Connect(connectionString);
        _db = _redis.GetDatabase();
    }

    /// <summary>
    /// 指定したプレフィックスを持つキーを取得する
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private IEnumerable<RedisKey> GetKeysByPrefix(string prefix, RedisDbTypeEnum type)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: prefix + "*", database: (int)type);

        return keys;
    }
    /// <summary>
    /// 指定したプレフィックスの値を取得する
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="dbType"></param>
    public IEnumerable<HashEntry[]> GetValuesByPrefix(string prefix, RedisDbTypeEnum dbType)
    {
        _db = _redis.GetDatabase((int)dbType);
        var keys = GetKeysByPrefix(prefix, dbType);
        var values = new List<HashEntry[]>();

        foreach (var key in keys)
        {
            var hashValues = _db.HashGetAll(key);
            values.Add(hashValues);
        }

        return values;
    }

    /// <summary>
    /// 指定したDBのキーを削除する
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dbType"></param>
    public void DeleteKey(string key, RedisDbTypeEnum dbType)
    {
        _db = _redis.GetDatabase((int)dbType);
        _db.KeyDelete(key);
    }

    /// <summary>
    /// 指定したキーの値を取得する
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dbType"></param>
    public string? GetVauluesByKey(string key, RedisDbTypeEnum dbType)
    {
        _db = _redis.GetDatabase((int)dbType);
        return _db.StringGet(key);
        
    }

    /// <summary>
    /// 指定したキーに値を設定する
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiry"></param>
    /// <param name="type"></param>
    public bool StringSet(string key, string value,RedisDbTypeEnum type ,TimeSpan? expiry = null)
    {
        _db = _redis.GetDatabase((int)type);
       return _db.StringSet(key, value, expiry);
    }

    /// <summary>
    /// 指定したキーに値を設定する
    /// </summary>
    /// <param name="key"></param>
    /// <param name="entry"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public void HashSet(string key,HashEntry[] entry, RedisDbTypeEnum type)
    {
        _db = _redis.GetDatabase((int)type);
        _db.HashSet(key, entry);
    }

        /// <summary>
    /// 指定したプレフィックスを持つキーを削除する
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="type"></param>
    public void DeleteKeysByPrefix(string prefix, RedisDbTypeEnum type)
    {
        var keys = GetKeysByPrefix(prefix, type);
        _db = _redis.GetDatabase((int)type);

        foreach (var key in keys)
        {
            _db.KeyDelete(key);
            Console.WriteLine($"Deleted key: {key}");
        }
    }
}