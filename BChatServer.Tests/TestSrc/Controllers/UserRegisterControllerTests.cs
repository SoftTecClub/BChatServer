using Microsoft.AspNetCore.Mvc;
using Moq;
using StackExchange.Redis;
using Xunit;
using BChatServer.Src.Controllers;
using BChatServer.Src.DB.Rdb;
using BChatServer.Src.DB.Rdb.Entity;
using BChatServer.Src.Model;
using BChatServer.Src.Service;
using Microsoft.EntityFrameworkCore;
using BChatServer.Tests.Common;

namespace BChatServer.Tests.TestSrc.Controllers
{
    /// <summary>
    /// ユーザ登録APIのテストクラス
    /// </summary>
    public class UserRegisterControllerTests
    {
        private readonly Mock<MyContext> _mockContext;
        private readonly Mock<IConnectionMultiplexer> _mockRedis;
        private readonly Mock<IDatabase> _mockDb;
        private readonly Mock<DbSet<UserEntity>> _mockUserSet;
        private readonly UserRegisterController _controller;
        private readonly TokenManageService _tokenManageService;

        private int _currentId;

        /// <summary>
        /// ユーザ登録APIのテストコンストラクタ
        /// </summary>
        public UserRegisterControllerTests()
        {
            _mockContext = new Mock<MyContext>();
            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockDb = new Mock<IDatabase>();
            _mockUserSet = new Mock<DbSet<UserEntity>>();
            _tokenManageService = new TokenManageService(_mockRedis.Object);
            _currentId = 1; // 初期ID
            _mockRedis.Setup(_ => _.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDb.Object);

            var data = new List<UserEntity>().AsQueryable();
            _mockUserSet.As<IQueryable<UserEntity>>().Setup(m => m.Provider).Returns(data.Provider);
            _mockUserSet.As<IQueryable<UserEntity>>().Setup(m => m.Expression).Returns(data.Expression);
            _mockUserSet.As<IQueryable<UserEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
            _mockUserSet.As<IQueryable<UserEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            _mockContext.Setup(c => c.Users).Returns((Microsoft.EntityFrameworkCore.DbSet<UserEntity>)_mockUserSet.Object);
            // DbSetのAddメソッドをオーバーライドしてIDを自動採番
            _mockUserSet.Setup(m => m.Add(It.IsAny<UserEntity>())).Callback<UserEntity>(entity =>
            {
                entity.Id = _currentId++;
            });



            _controller = new UserRegisterController(_mockContext.Object, _mockRedis.Object, _tokenManageService);
        }

        /// <summary>
        /// からのユーザ登録リクエストが来た場合
        /// </summary>
        [Fact]
        public void Post_ShouldReturnBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var model = new UserRegisterReceiveModel
            {
                Name = " ",
                UserId = " ",
                Email = " ",
                PhoneNumber = " ",
                Password = " "
            };

            // Act
            var result = _controller.Post(model);

            // Assert
            if (result is BadRequestObjectResult badRequestResult && badRequestResult.Value is UserRegisterResponseModel response)
            {
                Assert.True(response.NameIsError);
                Assert.True(response.UserIdIsError);
                Assert.True(response.EmailIsError);
                Assert.True(response.PhoneNumberIsError);
                Assert.True(response.PasswordIsError);
            }
            else
            {
                Assert.Fail("Expected BadRequestObjectResult with UserRegisterResponseModel");
            }
        }

        /// <summary>
        /// 入力チェック処理
        /// UserIdが数字、英文字、およびアンダースコアのみを含むかどうかを検証します。
        /// PhoneNumberがE.264形式に準拠しているかどうかを判定します。
        /// </summary>
        [Fact]
        public void Post_InputTextcheck()
        {
            // Arrange
            var modelErr = new UserRegisterReceiveModel
            {
                Name = "Test User",
                UserId = "日本語",
                Email = "test",
                PhoneNumber = "19348348399999999999999898999",
                Password = "password"
            };
            
            var modelOk = new UserRegisterReceiveModel
            {

                Email = "test@test.com",
                UserId = "test_user",
                PhoneNumber = "+819012345678"
            };
            // Act
            var errResult = _controller.Post(modelErr);
            var okResult = _controller.Post(modelOk);
            // Assert
            if (errResult is BadRequestObjectResult badRequestResult && badRequestResult.Value is UserRegisterResponseModel response)
            {
                Assert.False(response.NameIsError);
                Assert.False(response.PasswordIsError);
                Assert.True(response.UserIdIsError);
                Assert.True(response.PhoneNumberIsError);
                Assert.True(response.EmailIsError);
            }
            else
            {
                Assert.Fail("Expected BadRequestObjectResult with UserRegisterResponseModel");
            }

            if (okResult is BadRequestObjectResult badRequestResultOk && badRequestResultOk.Value is UserRegisterResponseModel responseOk)
            {
                Assert.True(responseOk.NameIsError);
                Assert.True(responseOk.PasswordIsError);
                Assert.False(responseOk.UserIdIsError);
                Assert.False(responseOk.PhoneNumberIsError);
                Assert.False(responseOk.EmailIsError);
            }
            else
            {
                Assert.Fail("Expected BadRequestObjectResult with UserRegisterResponseModel");
            }

           
        }

        /// <summary>
        /// 既に登録されているユーザIDで登録リクエストが来た場合
        /// </summary>
        [Fact]
        public void Post_ShouldReturnBadRequest_WhenUserIdAlreadyExists()
        {
            // Arrange
            var model = new UserRegisterReceiveModel
            {
                Name = "Test User",
                UserId = "testuser",
                Email = "test@example.com",
                PhoneNumber = CommonFunc.GenerateRandomE164PhoneNumber(),
                Password = "password"
            };

            // テストデータを追加
            var existingUser = new UserEntity
            {
                UserId = "testuser",
                Email = "existing@example.com",
                Name = "Existing User",
                PhoneNumber = "1234567890",
                Password = Src.Common.UserCommonFunc.HashPassword("password")
            };

            var data = new List<UserEntity> { existingUser }.AsQueryable();

            _mockUserSet.As<IQueryable<UserEntity>>().Setup(m => m.Provider).Returns(data.Provider);
            _mockUserSet.As<IQueryable<UserEntity>>().Setup(m => m.Expression).Returns(data.Expression);
            _mockUserSet.As<IQueryable<UserEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
            _mockUserSet.As<IQueryable<UserEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            // Act
            var result = _controller.Post(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<UserRegisterResponseModel>(badRequestResult.Value);
            Assert.True(response.UserIdIsError);
        }

        /// <summary>
        /// ユーザ登録が成功した場合
        /// </summary>
        [Fact]
        public void Post_ShouldReturnOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var model = new UserRegisterReceiveModel
            {
                Name = "Test User",
                UserId = "testuser",
                Email = "test@example.com",
                PhoneNumber = CommonFunc.GenerateRandomE164PhoneNumber(),
                Password = "password"
            };

            // Act
            var result = _controller.Post(model);
            Console.WriteLine(result);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User registration success", okResult.Value);
        }
    }
}