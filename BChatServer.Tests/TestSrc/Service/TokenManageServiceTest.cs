using System;
using Xunit;
using Moq;
using StackExchange.Redis;
using BChatServer.Src.Service;
using BChatServer.Tests.Common;

namespace BChatServer.Tests.TestSrc.Service
{
    /// <summary>
    /// トークン管理サービスのテストクラス
    /// </summary>
    public class TokenManageServiceTest
    {
        private readonly RedisService _redis;

        private readonly TokenManageService _tokenManageService;

        /// <summary>
        /// Redisのエントリ
        /// </summary>
        public class MockRedisEntry
        {
            /// <summary>
            /// Redisの値
            /// </summary>
            public RedisValue Value { get; set; }
            /// <summary>
            /// 有効期限
            /// </summary>
            public DateTime? ExpiryTime { get; set; }
        }
        /// <summary>
        /// トークン管理サービスのテストコンストラクタ
        /// </summary>
        public TokenManageServiceTest()
        {

            _redis = CommonFunc.GenerateRedis();
            // モックを使ってTokenManageServiceのインスタンスを作成
            _tokenManageService = new TokenManageService(_redis);
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
            Thread.Sleep((CommonFunc.Token_ExpireTimeForSec+1)*1000);
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
            Thread.Sleep((CommonFunc.Token_ExpireTimeForSec+1)*1000); 
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