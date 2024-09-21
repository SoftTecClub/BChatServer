namespace BChatServer.Tests.Common;
using System.Security.Cryptography;
using System.Text;
using Moq;
using StackExchange.Redis;

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
}