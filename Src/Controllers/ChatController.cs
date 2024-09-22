using System.Linq.Expressions;
using BChatServer.Src.DB.Rdb;
using BChatServer.Src.DB.Rdb.Entity;
using BChatServer.Src.DB.Redis;
using BChatServer.Src.Model;
using BChatServer.Src.Service;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StackExchange.Redis;

namespace BChatServer.Src.Controllers;

/// <summary>
/// チャット用のAPIクラス
/// <author>Ryokugyoku</author>
/// <Date>2024/09/21</Date>
/// </summary>
[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase{
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
    /// チャットAPI コンストラクタ
    /// </summary>
    /// <param name="context"></param>
    /// <param name="redis"></param>
    /// <param name="tokenManageService"></param>
    public ChatController(MyContext context, IConnectionMultiplexer redis, TokenManageService tokenManageService){
        _context = context;
        _redis = redis;
        _tokenManageService = tokenManageService;
    }

    /// <summary>
    /// チャット作成API
    /// </summary>
    /// <param name="model"></param>
    /// <returns>ステータス200の時トークンを返す</returns>
    [HttpPost]
    [Route("create")]
    public IActionResult CreateChatPost([FromBody] ChatCreateReceiveModel model){
        try{
            string myUserId;
            // パラメータチェック
            if (string.IsNullOrEmpty(model.ToUserId) || string.IsNullOrEmpty(model.Token)){
                return BadRequest();
            }else{
                var userId = _tokenManageService.GetUserIdFromToken(model.Token);
                if (userId == null)
                {
                    return Unauthorized();
                }
                myUserId = userId;
            }

            if(!_tokenManageService.ValidateToken(model.Token)){
                return Unauthorized();
            }

            UserEntity? user = _context.Users.Where(u => u.UserId == model.ToUserId).FirstOrDefault();
            if(user == null){
                return NotFound();
            }
            
            string chatId = Guid.NewGuid().ToString();
            List<ChatEntity> chat = new List<ChatEntity> {
                new ChatEntity {
                    ChatId = chatId,
                    UserId = model.ToUserId,
                },
                new ChatEntity {
                    ChatId = myUserId,
                    UserId = model.ToUserId,
                },
            };
            
            _context.Chats.AddRange(chat);
            _context.SaveChanges();

        }catch(Exception e){
            Log.Error("Create Chat Error", model.ToUserId);
            Log.Error(e.Message);
            return StatusCode(500, e.Message);
        }
        return Ok();
    }
    
    /// <summary>
    /// チャット送信API
    /// チャットIDが存在しないとき、404を返す
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("send")]
    public IActionResult SendChatPost([FromBody] ChatSendReceiveModel model){
        try{
            string userId ;
            // パラメータチェック
            if (string.IsNullOrEmpty(model.ChatId) || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.Message)){
                return BadRequest();
            }else{
                userId = _tokenManageService.GetUserIdFromToken(model.Token)??"";
                if (userId == null || userId == string.Empty)
                {
                    return Unauthorized();
                }
            }

            if(!_tokenManageService.ValidateToken(model.Token)){
                return Unauthorized();
            }
            ChatEntity? chat = _context.Chats.Where(c => c.ChatId == model.ChatId && c.UserId == userId).FirstOrDefault();
            if(chat == null){
                return NotFound();
            }
        
        var db = _redis.GetDatabase((int)RedisDbTypeEnum.Chat);
        db.HashSet(model.ChatId, new HashEntry[] {
            new HashEntry("UserId", userId),
            new HashEntry("Message", model.Message),
            new HashEntry("SendDate", DateTime.UtcNow.ToString())
        });

        }catch(Exception e){
            Log.Error("Send Chat Error", model.ChatId);
            Log.Error(e.Message);
            return StatusCode(500, e.Message);
        }
        return Ok();
    }

    /// <summary>
    /// チャット取得API
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public IActionResult GetChat([FromBody] ChatGetReceiveModel model){
        try{
            string userId ;
            // パラメータチェック
            if (string.IsNullOrEmpty(model.ChatId) || string.IsNullOrEmpty(model.Token)){
                return BadRequest();
            }else{
                userId = _tokenManageService.GetUserIdFromToken(model.Token)??"";
                if (userId == null || userId == string.Empty)
                {
                    return Unauthorized();
                }
            }

            if(!_tokenManageService.ValidateToken(model.Token)){
                return Unauthorized();
            }
            ChatEntity? chat = _context.Chats.Where(c => c.ChatId == model.ChatId && c.UserId == userId).FirstOrDefault();
            if(chat == null){
                return NotFound();
            }
        
        var db = _redis.GetDatabase((int)RedisDbTypeEnum.Chat);
        var chatData = db.HashGetAll(model.ChatId);
        return Ok(chatData);
        }catch(Exception e){
            Log.Error("Get Chat Error", model.ChatId);
            Log.Error(e.Message);
            return StatusCode(500, e.Message);
        }
    }
}