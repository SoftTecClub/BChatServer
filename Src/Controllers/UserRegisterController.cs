namespace BChatServer.Src.Controllers;

using BChatServer.Src.Common;
using BChatServer.Src.DB.Rdb;
using BChatServer.Src.DB.Rdb.Entity;
using BChatServer.Src.Model;
using BChatServer.Src.Service;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StackExchange.Redis;

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
    /// <summary>
    /// ユーザ作成API コンストラクタ
    /// </summary>
    /// <param name="context"></param>
    /// <param name="redis"></param>
    /// <param name="tokenManageService"></param>
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
        if(response.NameIsError || response.UserIdIsError || response.EmailIsError || response.PhoneNumberIsError || response.PasswordIsError){ 
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
            UserId = model.UserId,
            Email = model.Email,
            Name = model.Name,
            Password = model.Password,
            PhoneNumber = model.PhoneNumber
        };
        _context.Users.Add(user);
        _context.SaveChanges();
        Log.Information("User registration request from {Name} User registration Success", model.Name);
        return Ok("User registration success");
    }

    /// <summary>
    /// チェック処理
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private UserRegisterResponseModel CheckParameter(UserRegisterReceiveModel model){
        UserRegisterResponseModel response = new UserRegisterResponseModel();
        if(string.IsNullOrEmpty(model.Name) || string.IsNullOrWhiteSpace(model.Name)){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            response.NameIsError = true;
        }
        if(string.IsNullOrEmpty(model.UserId) || string.IsNullOrWhiteSpace(model.UserId)){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            response.UserIdIsError = true;
        }else{
            if(!UserCommonFunc.IsValidUserId(model.UserId)){
                Log.Information("User registration request from {Name} User registration Failed", model.Name);
                response.UserIdIsError = true;
            }
        }
        if(string.IsNullOrEmpty(model.Email) || string.IsNullOrWhiteSpace(model.Email)){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            response.EmailIsError = true;
        }else{
            if(!UserCommonFunc.IsValidEmail(model.Email)){
                Log.Information("User registration request from {Name} User registration Failed", model.Name);
                response.EmailIsError = true;
            }
        }
        if(string.IsNullOrEmpty(model.PhoneNumber) || string.IsNullOrWhiteSpace(model.PhoneNumber)){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            response.PhoneNumberIsError = true;
        }else{
            if(!UserCommonFunc.IsValidPhoneNumber(model.PhoneNumber)){
                Log.Information("User registration request from {Name} User registration Failed", model.Name);
                response.PhoneNumberIsError = true;
            }
        }
        if(string.IsNullOrEmpty(model.Password) || string.IsNullOrWhiteSpace(model.Password)){
            Log.Information("User registration request from {Name} User registration Failed", model.Name);
            response.PasswordIsError = true;
        }
        return response;
    }
}
