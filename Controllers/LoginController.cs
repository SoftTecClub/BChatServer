using BChatServer.DB.Rdb;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace BChatServer.Controllers
{
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

        public LoginController(MyContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }

        // POST: /Login
        [HttpPost]
        public IActionResult Post([FromBody] LoginModel model)
        {
            // Your code here
            return Ok("Login successful!");
        }
    }

    public class LoginModel
    {
        /// <summary>
        /// ユーザID
        /// </summary>
        public string Name { get; set; }

        public string Password { get; set; }
    }
}