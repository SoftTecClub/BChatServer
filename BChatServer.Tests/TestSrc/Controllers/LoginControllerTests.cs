using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using BChatServer.Src.Controllers;
using BChatServer.Src.DB.Rdb;
using BChatServer.Src.DB.Rdb.Entity;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using BChatServer.Tests.Common;
using BChatServer.Src.Service;

namespace BChatServer.Tests.TestSrc.Controllers;
    /// <summary>
    /// ログインAPIのテストクラス
    /// </summary>
    public class LoginControllerTests
    {
        private readonly Mock<MyContext> _mockContext;
        private readonly Mock<IConnectionMultiplexer> _mockRedis;
        private readonly LoginController _controller;
        private readonly Mock<TokenManageService> _mockTokenService;
        private readonly Mock<IDatabase> _mockDatabase;

        private readonly Mock<IDatabase> _mockDb;

        /// <summary>
        /// ユーザ名とプレーンパスワードのペア
        /// </summary>
        private readonly Dictionary<string, string> _userPlainPass= new Dictionary<string, string>();
        private readonly List<UserEntity> _users = new List<UserEntity>();  
        /// <summary>
        /// ログインAPIのテストコンストラクタ
        /// </summary>
        public LoginControllerTests()
        {
            //モックデータベースに対しての操作を行うためのデータストア
            var mockDataStore = new Dictionary<string, RedisValue>();

            // モックの設定
            _mockContext = new Mock<MyContext>();
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();
            _mockDb = new Mock<IDatabase>();

            // RedisからDBを返す時モックDBを返すようにする
            _mockRedis.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDb.Object);

            //StringSetメソッドがよびだれた時の処理
            // モックの設定
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

            _mockDb.Setup(db => db.StringGet(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .Returns((RedisKey key, CommandFlags flags) =>
                {
                    return mockDataStore.TryGetValue(key.ToString(), out var value) ? value : RedisValue.Null;
                });


            // TokenManageServiceのモックを設定
            _mockTokenService = new Mock<TokenManageService>(_mockRedis.Object);
            _mockTokenService.SetupProperty(_ => _.ExpiryDurationSec, CommonFunc.Token_ExpireTimeForSec);

            // テストデータの追加
            _users = UserCommonFunc.CreateUserEntity(10);
            _users.ForEach(u => 
            {
                _userPlainPass.Add(u.UserId, u.Password);
                u.Password = Src.Common.UserCommonFunc.HashPassword(u.Password);
            });

            // DbSet<UserEntity> のモックを作成
            var mockDbSet = new Mock<DbSet<UserEntity>>();
            var usersQueryable = _users.AsQueryable();
            mockDbSet.As<IQueryable<UserEntity>>().Setup(m => m.Provider).Returns(usersQueryable.Provider);
            mockDbSet.As<IQueryable<UserEntity>>().Setup(m => m.Expression).Returns(usersQueryable.Expression);
            mockDbSet.As<IQueryable<UserEntity>>().Setup(m => m.ElementType).Returns(usersQueryable.ElementType);
            mockDbSet.As<IQueryable<UserEntity>>().Setup(m => m.GetEnumerator()).Returns(usersQueryable.GetEnumerator());

            _mockContext.Setup(c => c.Users).Returns(mockDbSet.Object);

            // LoginControllerのインスタンスを作成
            _controller = new LoginController(_mockContext.Object, _mockRedis.Object, _mockTokenService.Object);
        }
        /// <summary>
        /// テストログイン成功パターン
        /// </summary>
        [Fact]
        public void Post_LoginSuccessful_ReturnsOk()
        {
            // Arrange
            var loginModel = new LoginModel { Name = _users[0].UserId, Password = _userPlainPass[_users[0].UserId] };
            // Act
            var result = _controller.Post(loginModel);
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Assert
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
        }

        /// <summary>
        /// 失敗パターン
        /// 直接ハッシュ値を送信した時
        /// </summary>
        [Fact]
        public void Post_LoginFailed_SendHasPassword_ReturnsBadRequest()
        {
            // Arrange
            var loginModel = new LoginModel { Name = _users[0].UserId, Password = _users[0].Password };

            // Act
            var result = _controller.Post(loginModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        /// <summary>
        /// 際ログインが実行された時、アクセストークンが再発行されることを確認する
        /// </summary>
        [Fact]
        public void Post_Login_ReGenerate_Token(){
            var loginModel = new LoginModel { Name = _users[0].UserId, Password = _userPlainPass[_users[0].UserId] };
            // Act
            var result = _controller.Post(loginModel);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseBody = Assert.IsType<LoginResponse>(okResult.Value);

            var loginModelSec = new LoginModel { Name = _users[0].UserId, Password = _userPlainPass[_users[0].UserId] };

            var resultSec = _controller.Post(loginModelSec);
            var okResultSec = Assert.IsType<OkObjectResult>(resultSec);
            var responseBodySec = Assert.IsType<LoginResponse>(okResultSec.Value);
            // Assert
            Assert.NotEqual(responseBody.Token, responseBodySec.Token);
        }
    }
