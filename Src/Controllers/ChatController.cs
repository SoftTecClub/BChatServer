using BChatServer.Src.Common;
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
    /// トークンが不正な場合401を返す。
    /// ユーザが存在しない場合404を返す。
    ///  既にチャットが存在する場合409を返す。
    /// その他のエラーの場合500を返す。
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

            UserEntity? toUser = _context.Users.Where(u => u.UserId == model.ToUserId).FirstOrDefault();
            UserEntity? myUser = _context.Users.Where(u => u.UserId == myUserId).FirstOrDefault();
            if(toUser == null || myUser == null){
                return NotFound();
            }
            List<ChatEntity> chatList = [.. toUser.Chats, .. myUser.Chats];
            
            var existingChatGroup = chatList
                .GroupBy(c => c.ChatId)
                .FirstOrDefault(g => g.Any(c => c.UserId == myUserId) && g.Any(c => c.UserId == model.ToUserId));
            if (existingChatGroup != null) {
                 // 同じChatIdグループが存在する場合の処理
                return Conflict();
            }
            string chatId = Guid.NewGuid().ToString();
            List<ChatEntity> chat = new List<ChatEntity> {
                new ChatEntity {
                    ChatId = chatId,
                    UserId = model.ToUserId
                },
                new ChatEntity {
                    ChatId = chatId,
                    UserId = myUserId
                },
            };
            
            _context.Chats.AddRange(chat);
            _context.SaveChanges();
            ChatCreateResponseModel response = new ChatCreateResponseModel
            {
                ChatId = chatId
            };
            return Ok(response);
        }catch(Exception e){
            Log.Error("Create Chat Error", model.ToUserId);
            Log.Error(e.Message);
            return StatusCode(500, e.Message);
        }
        
    }
    
    /// <summary>
    /// チャット送信API
    /// チャットIDが存在しないとき404を返す。
    /// トークンが不正な場合401を返す。
    /// その他のエラーの場合500を返す。
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
        db.HashSet(model.ChatId+":"+SecurityCommonFunc.GenerateRandomString(5), new HashEntry[] {
            new HashEntry("UserId", userId),
            new HashEntry("Message", model.Message),
            new HashEntry("SendDate", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ff"))
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
    [HttpPost]
    [Route("getChat")]
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
        var server = _redis.GetServer(_redis.GetEndPoints().First());

        // プレフィックスを持つ全てのキーを取得
        var keys = new List<RedisKey>();
        var cursor = 0L;
        do
        {
            var result = server.Keys((int)cursor, pattern: model.ChatId + ":*", pageSize: 1000).ToArray();
            keys.AddRange(result);
            cursor = result.Length == 0 ? 0 : cursor + 1;
            Console.WriteLine($"Cursor: {cursor}, Keys: {string.Join(", ", result)}");
        } while (cursor != 0);
        var chatData = db.HashGetAll(model.ChatId);
        var response = new ChatSendResponseModel();
        foreach(var data in chatData){
            ChatSendResponse chatSendResponse = new ChatSendResponse
            {
                ChatId = model.ChatId,
                UserId = data.Name == "UserId" ? (data.Value.HasValue ? (string?)data.Value ?? "" : "") : "",
                Message = data.Name == "Message" ? (data.Value.HasValue ? (string?)data.Value ?? "" : "") : "",
                CreatedAt = data.Name == "SendDate" && data.Value.HasValue && !string.IsNullOrEmpty(data.Value) ? DateTime.Parse(data.Value.ToString()) : DateTime.MinValue
            };
            response.ChatSendResponses.Add(chatSendResponse);
        }
        return Ok(response);
        }catch(Exception e){
            Log.Error("Get Chat Error", model.ChatId);
            Log.Error(e.Message);
            return StatusCode(500, e.Message);
        }
    }

    /// <summary>
    /// チャットリスト取得API
    /// トークンが存在しないとき404を返す。
    /// トークンが不正な場合401を返す。
    /// その他のエラーの場合500を返す。
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public IActionResult GetChatList([FromBody] ChatGetListReceiveModel model){
        try{
            string userId ;
            // パラメータチェック
            if (string.IsNullOrEmpty(model.Token)){
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
            List<ChatEntity> chatList = _context.Chats.Where(c => c.UserId == userId).ToList();
            ChatListResponseModel chatListResponse = new ChatListResponseModel();
            chatList.ForEach(c => {
                chatListResponse.chatIds.Add(c.ChatId);
            });
            return Ok(chatListResponse);
        }catch(Exception e){
            Log.Error(e.Message);
            return StatusCode(500, e.Message);
        }
    }
}