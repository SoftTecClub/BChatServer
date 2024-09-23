using BChatServer.Src.Controllers;
using BChatServer.Src.DB.Rdb;
using BChatServer.Src.DB.Rdb.Entity;
using BChatServer.Src.Model;
using BChatServer.Src.Service;
using BChatServer.Tests.Common;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Xunit;

namespace BChatServer.Tests.Controllers
{
    /// <summary>
    /// ChatControllerのテストクラス
    /// </summary>
    public class ChatControllerTests: IDisposable
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
                
                if(result is OkObjectResult okResult){
                    var loginResult =_loginController.Post(new LoginModel
                    {
                        Name = user.UserId,
                        Password = user.Password
                    });
                    if(loginResult is OkObjectResult loginOkResult){
                        // loginOkResult.Value がトークンを含む文字列であることを確認
                        if (loginOkResult.Value is LoginResponse loginResponseModel)
                        {
                            TokenData.Add(user.UserId, loginResponseModel.Token);
                        }
                    }
                }
                
            }
            _context.SaveChanges();
        }

        /// <summary>
        /// チャット作成APIのテスト
        /// チェック内容
        /// 1. チャット作成APIが正常に動作するか
        /// 2. トークンが不正な場合、401を返すか
        /// 3. ユーザが存在しない場合、404を返すか
        /// 4. 既にチャットが存在する場合、409を返すか
        /// </summary>
        [Fact]
        public void ChatCreateTest()
        {
            ChatCreateReceiveModel model = new ChatCreateReceiveModel
            {
                ToUserId = _users[0].UserId,
                Token = TokenData[_users[1].UserId]
            };
            // Given
           var firstResult = _controller.CreateChatPost(model);
           var secondResult = _controller.CreateChatPost(new ChatCreateReceiveModel{
               ToUserId = _users[0].UserId,
               Token = "InvalidToken"});
            var thirdResult = _controller.CreateChatPost(new ChatCreateReceiveModel{
            ToUserId = "InvalidUserId",
            Token = TokenData[_users[1].UserId]});
            var fourthResult = _controller.CreateChatPost(model);
            
            if(firstResult is OkResult okResult){
                Assert.Equal(200, okResult.StatusCode);
            }else{
                Assert.Fail("Failed to create chat");
                return;
            }

            if(secondResult is UnauthorizedResult){
                Assert.Equal(401, (secondResult as UnauthorizedResult)?.StatusCode);
            }else{  
                Assert.Fail("Failed to Unauthorized");
            }

            if(thirdResult is NotFoundResult){
                Assert.Equal(404, (thirdResult as NotFoundResult)?.StatusCode);
            }else{
                Assert.Fail("Failed to NotFound");
            }

            if(fourthResult is ConflictResult){
                Assert.Equal(409, (fourthResult as ConflictResult)?.StatusCode); 
            }else{
                Assert.Fail("Failed to Conflict");
            } 

        }

        /// <summary>
        /// ChatControllerTests終了時に実行される処理
        /// </summary>
        public void Dispose()
        {
            foreach (var user in _users)
            {
                var userToDelete = _context.Users.SingleOrDefault(u => u.UserId == user.UserId);
                var chatToDelete = _context.Chats.Where(c => c.UserId == user.UserId);
                if (userToDelete != null)
                {
                    _context.Users.Remove(userToDelete);
                    _context.Chats.RemoveRange(chatToDelete);
                }
            }
            _context.SaveChanges();
            // Redisのキーを個別に削除
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys();
            foreach (var key in keys)
            {
                _redis.GetDatabase().KeyDelete(key);
            }
            _context.Dispose();
            _redis.Dispose();
        }
    }
}