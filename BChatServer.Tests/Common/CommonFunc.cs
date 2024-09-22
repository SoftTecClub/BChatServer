namespace BChatServer.Tests.Common;
using System.Security.Cryptography;
using System.Text;
using Moq;
using StackExchange.Redis;
/// <summary>
/// テスト用の共通関数クラス
/// <author>Ryokugyoku</author>
/// <date>2024/09/22</date>
/// </summary>
public static class CommonFunc{

    private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <summary>
    /// 文字列をランダム生成するメソッド
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GenerateRandomString(int length)
    {
        if (length <= 0)
            throw new ArgumentException("Length must be a positive number", nameof(length));

        var randomBytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var result = new StringBuilder(length);
        foreach (var b in randomBytes)
        {
            result.Append(AllowedChars[b % AllowedChars.Length]);
        }

        return result.ToString();
    }

    /// <summary>
    /// トークンの有効期限を秒で設定
    /// </summary>
    public static int Token_ExpireTimeForSec = 5;

    /// <summary>
    /// モックRedisを生成するメソッド
    /// </summary>
    /// <returns></returns>
    public static Mock<IConnectionMultiplexer> GenerateMockRedis()
    {
        //モックデータベースに対しての操作を行うためのデータストア
        var mockDataStore = new Dictionary<string, RedisValue>();
        Mock<IDatabase>_mockDb = new Mock<IDatabase>();
        Mock<IConnectionMultiplexer> _mockRedis = new Mock<IConnectionMultiplexer>();

        _mockRedis.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDb.Object);

                    
        // モックの設定 StringSetメソッドがよびだれた時の処理
        _mockDb.Setup(db => db.StringSet( It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()))
            .Returns((RedisKey key, RedisValue value, TimeSpan? expiry, bool keepTtl, When when, CommandFlags flags) =>
            {
                mockDataStore[key.ToString()] = value;
                return true;
            });
            
            // モックの設定 StringGetメソッドがよびだれた時の処理
            _mockDb.Setup(db => db.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .Returns((RedisKey key, CommandFlags flags) =>
            {
                return mockDataStore.TryGetValue(key.ToString(), out var value) ? value : RedisValue.Null;
            });
        
        return new Mock<IConnectionMultiplexer>();
    }
    private static Random random = new Random();
    /// <summary>
    /// ランダムなE164形式の電話番号を生成する
    /// </summary>
    /// <returns></returns>
    public static string GenerateRandomE164PhoneNumber()
    {
        // 国番号をランダムに選択（例: 1 = US, 44 = UK, 81 = Japan）
        int[] countryCodes = { 1, 44, 81 };
        int countryCode = countryCodes[random.Next(countryCodes.Length)];

        // 残りの桁数をランダムに生成（最大15桁 - 国番号の桁数）
        int remainingDigits = 15 - countryCode.ToString().Length;
        string localNumber = GenerateRandomDigits(remainingDigits);

        return $"+{countryCode}{localNumber}";
    }

    /// <summary>
    /// 指定された桁数のランダムな数字列を生成する
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    private static string GenerateRandomDigits(int length)
    {
        char[] digits = new char[length];
        for (int i = 0; i < length; i++)
        {
            digits[i] = (char)('0' + random.Next(10));
        }
        return new string(digits);
    }
}