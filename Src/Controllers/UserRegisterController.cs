namespace BChatServer.Src.Controllers;

using BChatServer.Src.Common;
using BChatServer.Src.DB.Rdb;
using BChatServer.Src.DB.Rdb.Entity;
using BChatServer.Src.Model;
using BChatServer.Src.Service;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// ユーザ登録用のAPIクラス
/// <author>Ryokugyoku</author>
/// <Date>2024/09/21</Date>
/// </summary>
/// <returns>ユーザ登録時 200を返す</returns>
[ApiController]
[Route("[controller]")]
public class UserRegisterController : ControllerBase{
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
    public UserRegisterController(MyContext context, IConnectionMultiplexer redis, TokenManageService tokenManageService){
        _context = context;
        _redis = redis;
        _tokenManageService = tokenManageService;
    }

    /// <summary>
    /// ユーザ登録API
    /// </summary>
    /// <param name="model">登録情報 NameにユーザID、パスワードにハッシュ化したパスワードを入力してください</param>
    /// <returns>ステータス200の時トークンを返す</returns>
    [HttpPost]
    public IActionResult Post([FromBody] UserRegisterModel model){
        // ユーザIDが既に登録されているか確認
        if(_context.Users.FirstOrDefault(u => u.UserId == model.Name) != null){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            return BadRequest("User registration failed");
        }
        // ユーザ登録
        UserEntity user = new UserEntity(){
            UserId = model.Name,
            Email = model.Email,
            Password = model.Password,
        };
        _context.Users.Add(user);
        _context.SaveChanges();
        Log.Information("User registration request from {Name} User registration Success", model.Name);
        return Ok("User registration success");
    }
}
