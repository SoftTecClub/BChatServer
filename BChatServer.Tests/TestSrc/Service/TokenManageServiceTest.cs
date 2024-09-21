using System;
using Xunit;
using Moq;
using StackExchange.Redis;
using BChatServer.Src.Service;
using BChatServer.Tests.Common;

namespace BChatServer.Tests.TestSrc.Service
{
    public class TokenManageServiceTest
    {
        private readonly Mock<IConnectionMultiplexer> _mockRedis;
        private readonly Mock<IDatabase> _mockDb;
        private readonly TokenManageService _tokenManageService;

        /// <summary>
        /// Redisのエントリ
        /// </summary>
        public class MockRedisEntry
        {
            public RedisValue Value { get; set; }
            public DateTime? ExpiryTime { get; set; }
        }
        public TokenManageServiceTest()
        {
            var mockDataStore = new Dictionary<string, MockRedisEntry>();

            // モックの作成
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockDb = new Mock<IDatabase>();

            // RedisからDBを返す時モックDBを返すようにする
            _mockRedis.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDb.Object);

            // モックの設定
            _mockDb.Setup(db => db.StringSet( It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
                .Returns((RedisKey key, RedisValue value, TimeSpan? expiry, bool keepTtl, When when, CommandFlags flags) =>
                {
                    MockRedisEntry entry = new MockRedisEntry
                    {
                        Value = value,
                        ExpiryTime = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : (DateTime?)null
                    };

                    mockDataStore[key.ToString()] = entry;
                    return true;
                });

            _mockDb.Setup(db => db.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns((RedisKey key, CommandFlags flags) =>
                {
                    if (mockDataStore.TryGetValue(key.ToString(), out var entry))
                    {
                        if (entry.ExpiryTime.HasValue && entry.ExpiryTime.Value < DateTime.UtcNow)
                        {
                            // 有効期限が過ぎている場合は RedisValue.Null を返す
                            return RedisValue.Null;
                        }
                        return entry.Value;
                    }
                    return RedisValue.Null;
                });

            // モックを使ってTokenManageServiceのインスタンスを作成
            _tokenManageService = new TokenManageService(_mockRedis.Object);
            _tokenManageService.ExpiryDurationSec = CommonFunc.Token_ExpireTimeForSec;
        }

        /// <summary>
        /// トークン生成テスト
        /// </summary>
        [Fact]
        public void GenerateToken_ShouldReturnToken()
        {
            // Arrange
            var userId = "testUser1";
            
            // Act
            var token = _tokenManageService.GenerateToken(userId);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        /// <summary>
        /// ユーザIDからトークン取得テスト
        /// </summary>
        [Fact]
        public void GetToken_ShouldReturnToken()
        {
            // Arrange
            var userId = "testUser2";
            var expectedToken = _tokenManageService.GenerateToken(userId);
            // Act
            var token = _tokenManageService.GetToken(userId);
            // Assert
            Assert.Equal(expectedToken, token);
        }

        /// <summary>
        /// トークンを設定し、期限切れになるまで待機し、トークンを取得するテスト
        /// </summary>
        [Fact]
        public void GetToken_ExpiredFortime(){
            // Arrange
            var userId = "testUser2";
            var expectedToken = _tokenManageService.GenerateToken(userId);
            Thread.Sleep(CommonFunc.Token_ExpireTimeForSec+1);
            // Act
            var token = _tokenManageService.GetToken(userId);
            // Assert
            Assert.NotEqual(expectedToken, token);
            Assert.Null(token);
        }

        /// <summary>
        /// トークンの有効期限テスト
        /// </summary>
        [Fact]
        public void ValidateToken_ShouldReturnTrueForValidToken()
        {
            // Arrange
            var userId = "testUser3";
            var token = _tokenManageService.GenerateToken(userId);

            // Act
            var isValid = _tokenManageService.ValidateToken(token);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// 無効なアクセストークンの検証テスト
        /// </summary>
        [Fact]
        public void ValidateToken_ShouldReturnFalseForInvalidToken()
        {
            // Arrange
            var invalidToken = "invalidToken";

            // Act
            var isValid = _tokenManageService.ValidateToken(invalidToken);

            // Assert
            Assert.False(isValid);
        }

        /// <summary>
        /// トークンからユーザIDを取得するテスト
        /// </summary>
        [Fact]
        public void GetUserIdFromToken_ShouldReturnUserId()
        {
            // Arrange
            var userId = "testUser4";
            var token = _tokenManageService.GenerateToken(userId);

            // Act
            var extractedUserId = _tokenManageService.GetUserIdFromToken(token);

            // Assert
            Assert.Equal(userId, extractedUserId);
        }

        /// <summary>
        /// トークンの有効期限テスト
        /// </summary>
        [Fact]
        public void Token_ExpiredFortime(){
            // Arrange
            var userId = "testUser5";
            var token = _tokenManageService.GenerateToken(userId);
            Thread.Sleep(CommonFunc.Token_ExpireTimeForSec+1); // 11秒待機
            // Act
            var isValid = _tokenManageService.ValidateToken(token);
            // Assert
            Assert.False(isValid);
        }

        /// <summary>
        /// Token再生成テスト
        /// </summary>
        [Fact]
        public void Token_Regenerate(){
            // Arrange
            var userId = "testUser6";
            var token = _tokenManageService.GenerateToken(userId);
            var tokenSec = _tokenManageService.GenerateToken(userId);
            // Act
            var isValid = _tokenManageService.ValidateToken(token);
            var isValidSec = _tokenManageService.ValidateToken(tokenSec);
            // Assert
            Assert.False(isValid);
            Assert.True(isValidSec);
            Assert.NotEqual(token, tokenSec);
        }
    }
}