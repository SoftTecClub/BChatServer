using Xunit;
using Microsoft.AspNetCore.Mvc;
using BChatServer.Src.Controllers;
using BChatServer.Src.DB.Rdb;
using BChatServer.Src.DB.Rdb.Entity;
using BChatServer.Tests.Common;
using BChatServer.Src.Service;

namespace BChatServer.Tests.TestSrc.Controllers;
    /// <summary>
    /// ログインAPIのテストクラス
    /// </summary>
    public class LoginControllerTests : IDisposable
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
        private readonly UserRegisterController _userRegisterController;

        /// <summary>
        /// ユーザ名とプレーンパスワードのペア
        /// </summary>
        private readonly Dictionary<string, string> _userPlainPass= new Dictionary<string, string>();
        private readonly List<UserEntity> _users = new List<UserEntity>();

        private LoginController _controller;  

        /// <summary>
        /// ログインAPIのテストコンストラクタ
        /// </summary>
        public LoginControllerTests()
        {
            _context = CommonFunc.GenerateContext();
            _redis = CommonFunc.GenerateRedis();
            _tokenManageService = new TokenManageService(_redis);

            _controller = new LoginController(_context, _redis, _tokenManageService);
            _userRegisterController = new UserRegisterController(_context, _redis, _tokenManageService);
            _users = UserCommonFunc.CreateUserEntity(10);

            foreach (var user in _users)
            {
                var result = _userRegisterController.Post(new UserRegisterReceiveModel
                {
                    Email = user.Email,
                    UserId = user.UserId,
                    Name = user.Name,
                    Password = user.Password,
                    PhoneNumber = user.PhoneNumber
                });
            }
        }
        /// <summary>
        /// テストログイン成功パターン
        /// </summary>
        [Fact]
        public void Post_LoginSuccessful_ReturnsOk()
        {
            // Arrange
            var loginModel = new LoginModel { Name = _users[0].UserId, Password = _users[0].Password };
            // Act
            var result = _controller.Post(loginModel);
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Assert
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
        }

        /// <summary>
        /// 際ログインが実行された時、アクセストークンが再発行されることを確認する
        /// </summary>
        [Fact]
        public void Post_Login_ReGenerate_Token(){
            var loginModel = new LoginModel { Name = _users[0].UserId, Password = _users[0].Password };
            // Act
            var result = _controller.Post(loginModel);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseBody = Assert.IsType<LoginResponse>(okResult.Value);

            var loginModelSec = new LoginModel { Name = _users[0].UserId, Password = _users[0].Password };

            var resultSec = _controller.Post(loginModelSec);
            var okResultSec = Assert.IsType<OkObjectResult>(resultSec);
            var responseBodySec = Assert.IsType<LoginResponse>(okResultSec.Value);
            // Assert
            Assert.NotEqual(responseBody.Token, responseBodySec.Token);
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
