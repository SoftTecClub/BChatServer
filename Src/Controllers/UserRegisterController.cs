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
    /// ヌルチェック、パラメータチェック、ユーザIDの重複チェックを行った後ユーザデータを登録する
    /// </summary>
    /// <param name="model"></param>
    /// <returns>ステータス200の時トークンを返す</returns>
    [HttpPost]
    public IActionResult Post([FromBody] UserRegisterReceiveModel model){
        // パラメータチェック
        UserRegisterResponseModel response = CheckParameter(model);
        if(response.NameIsError || response.UserIdIsError || response.EmailIsError || response.PhoneNumberIsError){
            return BadRequest(response);
        }
        // ユーザIDが既に登録されているか確認
        if(_context.Users.FirstOrDefault(u => u.UserId == model.UserId) != null){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            response.UserIdIsError = true;
            return BadRequest(response);
        }
        // ユーザ登録
        UserEntity user = new UserEntity(){
            UserId = model.Name,
            Email = model.Email,
            Password = model.Password,
            PhoneNumber = model.PhoneNumber
        };
        _context.Users.Add(user);
        _context.SaveChanges();
        Log.Information("User registration request from {Name} User registration Success", model.Name);
        return Ok("User registration success");
    }

    private UserRegisterResponseModel CheckParameter(UserRegisterReceiveModel model){
        UserRegisterResponseModel response = new UserRegisterResponseModel();
        if(model.Name == string.Empty){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            response.NameIsError = true;
        }
        if(model.UserId == string.Empty){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            response.UserIdIsError = true;
        }
        if(model.Email == string.Empty){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            response.EmailIsError = true;
        }
        if(model.PhoneNumber == string.Empty){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            response.PhoneNumberIsError = true;
        }

        return response;
    }
}
