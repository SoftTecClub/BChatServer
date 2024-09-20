using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using StackExchange.Redis;
namespace BChatServer.Src.Service;

/// <summary>
/// トークン生成サービス
/// トークンの秘密鍵はサーバが再起動するたびに変わる
/// 有効期限
/// 　Develop：1分
/// 　Production：30分
/// </summary>
public class TokenManageService
{
    private static readonly string SecretKey = GenerateSecretKey(); 
    
    private readonly IConnectionMultiplexer _redis;

    /// <summary>
    /// トークンの有効期限
    /// デフォルト１時間
    /// </summary>
    public virtual int ExpiryDurationSec { get; set; } = 30*60;
    

    public TokenManageService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    public string GenerateToken(string userId)
    {
        var db = _redis.GetDatabase();
        var oldToken = db.StringGet(userId);

        if (!string.IsNullOrEmpty(oldToken))
        {
            // 古いトークンを無効化
            db.KeyDelete(userId);
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(SecretKey);
        var uniqueId = Guid.NewGuid().ToString(); // ランダムな要素を追加
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("unique_id", uniqueId) 
            }),
            Expires = DateTime.UtcNow.AddSeconds(ExpiryDurationSec),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // 新しいトークンを保存
        db.StringSet(userId, tokenString, TimeSpan.FromSeconds(ExpiryDurationSec));

        return tokenString;
    }

    /// <summary>
    /// userIdに対応するトークンを取得する
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public string? GetToken(string userId)
    {
        var db = _redis.GetDatabase();
        return db.StringGet(userId);
    }
    /// <summary>
    /// 秘密鍵を生成する
    /// </summary>
    /// <returns></returns>
    private static string GenerateSecretKey()
    {
        var rng = RandomNumberGenerator.Create();
        var keyBytes = new byte[32]; // 256 bits
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }

    /// <summary>
    /// トークンが有効かどうかを検証する
    /// </summary>
    /// <param name="token">検証するトークン</param>
    /// <returns>トークンが有効であれば true、無効であれば false</returns>
    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(SecretKey);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// アクセストークンからユーザIDを取得する
    /// </summary>
    /// <param name="token">トークンを入れる</param>
    /// <returns></returns>
    public string? GetUserIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(SecretKey);
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim?.Value;
    }
}