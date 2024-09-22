using System.Linq.Expressions;
using BChatServer.Src.Controllers;
using BChatServer.Src.DB.Rdb;
using BChatServer.Src.DB.Rdb.Entity;
using BChatServer.Src.Model;
using BChatServer.Src.Service;
using BChatServer.Tests.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace BChatServer.Tests.Controllers
{
    /// <summary>
    /// ChatControllerのテストクラス
    /// </summary>
    public class ChatControllerTests
    {
        private readonly MyContext _context;
        private readonly IConnectionMultiplexer _redis;
        private readonly TokenManageService _tokenService;
        private readonly ChatController _controller;

        private readonly LoginController _loginController;

        private readonly UserRegisterController _userRegisterController;

        /// <summary>
        /// トークンデータを扱うDictionary
        /// Key:user_id, Value:token
        /// </summary>
        private IDictionary<string,string> TokenData = new Dictionary<string,string>();
        private List<UserEntity> _users;  

        /// <summary>
        /// ChatControllerTestsのコンストラクタ
        /// </summary>
        public ChatControllerTests()
        {
            _context = CommonFunc.GenerateContext();
            _redis = CommonFunc.GenerateRedis();
            _tokenService = new TokenManageService(_redis);
            _controller = new ChatController(_context, _redis, _tokenService);
            _loginController = new LoginController(_context, _redis, _tokenService);
            _userRegisterController = new UserRegisterController(_context, _redis, _tokenService);
            _users = UserCommonFunc.CreateUserEntity(20);

            foreach (var user in _users)
            {
                var result = _userRegisterController.Post(new UserRegisterReceiveModel
                {
                    Email = user.Email,
                    Name = user.Name,
                    Password = user.Password,
                    PhoneNumber = user.PhoneNumber
                });
                if(result is OkObjectResult okResult){
                    var loginResult =_loginController.Post(new LoginModel
                    {
                        Name = user.UserId,
                        Password = user.Password
                    });
                    if(loginResult is OkObjectResult loginOkResult){
                        TokenData.Add(user.UserId, loginOkResult.Value?.ToString()?? "");
                    }
                }
                
            }
            _context.SaveChanges();
        }



        /// <summary>
        /// ChatControllerTestsのデストラクタ
        /// </summary>
        ~ChatControllerTests()
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
            //一時ファイルの削除
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            server.FlushDatabase();

        }
    }
}