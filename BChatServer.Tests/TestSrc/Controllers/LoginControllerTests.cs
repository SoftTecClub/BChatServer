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

    public class LoginControllerTests
    {
        private readonly Mock<MyContext> _mockContext;
        private readonly Mock<IConnectionMultiplexer> _mockRedis;
        private readonly LoginController _controller;
        private readonly Mock<TokenManageService> _mockTokenService;
        private readonly Mock<IDatabase> _mockDatabase;

        /// <summary>
        /// ユーザ名とプレーンパスワードのペア
        /// </summary>
        private readonly Dictionary<string, string> _userPlainPass= new Dictionary<string, string>();
        private readonly List<UserEntity> _users = new List<UserEntity>();  

        public LoginControllerTests()
        {
            // モックの設定
            _mockContext = new Mock<MyContext>();
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();

            _mockRedis.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDatabase.Object);

            // TokenManageServiceのモックを設定
            _mockTokenService = new Mock<TokenManageService>(_mockRedis.Object);
            _mockTokenService.SetupProperty(_ => _.ExpiryDurationSec, 10);

            // テストデータの追加
            _users = UserCommonFunc.CreateUserEntity(10);
            _users.ForEach(u => 
            {
                _userPlainPass.Add(u.UserId, u.Password);
                u.Password = LoginModel.HashPassword(u.Password);
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
