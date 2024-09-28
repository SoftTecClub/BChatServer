using Microsoft.AspNetCore.Mvc;
using Xunit;
using BChatServer.Src.Controllers;
using BChatServer.Src.DB.Rdb;
using BChatServer.Src.Model;
using BChatServer.Src.Service;
using BChatServer.Tests.Common;
using BChatServer.Src.DB.Rdb.Entity;

namespace BChatServer.Tests.TestSrc.Controllers
{
    /// <summary>
    /// ユーザ登録APIのテストクラス
    /// </summary>
    public class UserRegisterControllerTests : IDisposable
    {
        /// <summary>
        /// データベースコンテキスト
        /// </summary>
        private readonly MyContext _context;

        /// <summary>
        /// Redis接続
        /// </summary>
        private readonly RedisService _redis;

        /// <summary>
        /// トークンマネージャ
        /// </summary>
        private readonly TokenManageService _tokenManageService;

        /// <summary>
        ///   ユーザ登録ようコントローラー
        /// </summary>
        private readonly UserRegisterController _controller;

        private readonly List<UserEntity> _users = new List<UserEntity>();

        /// <summary>
        /// ユーザ登録APIのテストコンストラクタ
        /// </summary>
        public UserRegisterControllerTests()
        {
            _context = CommonFunc.GenerateContext();
            _redis = CommonFunc.GenerateRedis();
            _tokenManageService = new TokenManageService(_redis);

            _controller = new UserRegisterController(_context, _redis, _tokenManageService);

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
                UserId = "test_user0",
                PhoneNumber = "+819012345678"
            };
            // Act
            var errResult = _controller.Post(modelErr);
            var okResult = _controller.Post(modelOk);
            _users.Add(new UserEntity
            {
                Name = modelOk.Name,
                UserId = modelOk.UserId,
                Email = modelOk.Email,
                PhoneNumber = modelOk.PhoneNumber,
                Password = modelOk.Password
            });
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
                UserId = "testuser1",
                Email = "test@example.com",
                PhoneNumber = CommonFunc.GenerateRandomE164PhoneNumber(),
                Password = "password"
            };

            // Act
            _controller.Post(model);
            var result = _controller.Post(model);

            _users.Add(new UserEntity
            {
                Name = model.Name,
                UserId = model.UserId,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password
            });

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
                UserId = "testuser2",
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
            _users.Add(new UserEntity
            {
                Name = model.Name,
                UserId = model.UserId,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Password = model.Password
            });
        }


        /// <summary>
        /// テスト終了時の後処理
        /// </summary>
        public void Dispose()
        {
            foreach (var user in _users)
            {
                var userToDelete = _context.Users.SingleOrDefault(u => u.UserId == user.UserId);
                if (userToDelete != null)
                {
                    _context.Users.Remove(userToDelete);
                }
            }
            _context.SaveChanges();
            _context.Dispose();
        }
    }
}