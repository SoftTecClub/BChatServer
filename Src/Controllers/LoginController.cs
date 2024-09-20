using BChatServer.Src.DB.Rdb;
using BChatServer.Src.DB.Rdb.Entity;
using BChatServer.Src.Service;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace BChatServer.Src.Controllers
{
    /// <summary>
    /// ユーザのログインAPIメインクラス
    /// <author>Ryokugyoku</author>
    /// <Date>2024/09/16</Date>
    /// </summary>
    /// <returns>ステータス200の時トークンを返す</returns>
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {

        /// <summary>
        /// データベースコンテキスト
        /// </summary>
        private readonly MyContext _context;

        /// <summary>
        /// Redis接続
        /// </summary>
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// トークンマネージャ
        /// </summary>
        private readonly TokenManageService _tokenManageService;
        public LoginController(MyContext context, IConnectionMultiplexer redis, TokenManageService tokenManageService)
        {
            _context = context;
            _redis = redis;
            _tokenManageService = tokenManageService;
        }

                
        /// <summary>
        /// ユーザのログインAPI
        /// </summary>
        /// <param name="model">ログイン情報 NameにユーザID、パスワードにハッシュ化したパスワードを入力してください</param>
        /// <returns>ステータス200の時トークンを返す</returns>
        [HttpPost]
        public IActionResult Post([FromBody] LoginModel model)
        {         
            UserEntity? user = _context.Users.FirstOrDefault(u => u.UserId == model.Name && u.Password == model.Password);
            List<UserEntity> users = _context.Users.ToList();
            if(user is null){
                 Log.Information("Login request from {Name} Login Failed", model.Name);
                return BadRequest("Login failed");
            }
            
            string token = _tokenManageService.GenerateToken(user.UserId);
            return Ok(new LoginResponse{Token = token});
        }
        
    }

    /// <summary>
    /// LoginModelログイン時に使用するモデル
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// ユーザID
        /// </summary>
        public string Name { get; set; } = String.Empty;

        private string _password = string.Empty;

        /// <summary>
        /// パスワード
        /// </summary>
        public string Password
        {
            get => _password;
            set => _password = HashPassword(value);
        }

        /// <summary>
        /// パスワードをSHA256でハッシュ化します。
        /// </summary>
        /// <param name="password">ハッシュ化するパスワード</param>
        /// <returns>ハッシュ化されたパスワード</returns>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}