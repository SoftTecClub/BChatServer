using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BChatServer.Src.Common;

/// <summary>
/// ユーザー共通関数クラス
/// <author>Ryokugyoku</author>
/// <date>2024/09/22</date>
/// <remarks>ユーザー関連の共通関数をまとめたクラス</remarks>
/// </summary>
public class UserCommonFunc
{
    /// <summary>
    /// パスワードをSHA256でハッシュ化します。
    /// </summary>
    /// <param name="password">ハッシュ化するパスワード</param>
    /// <returns>ハッシュ化されたパスワード</returns>
    public static string HashPassword(string password)
    {
        if(string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
        {
            return string.Empty;
        }
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

    /// <summary>
    /// メールアドレスが有効かどうかを判定します。
    /// </summary>
    /// <date>2024/09/22</date>
    /// <author>Ryokugyoku</author>
    /// <param name="email"></param>
    /// <returns></returns>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // 正規表現パターンを定義
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
            return regex.IsMatch(email);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// 電話番号が有効かどうかを判定します。
    /// E.264形式に準拠しているかどうかを判定します。
    /// </summary>
    /// <date>2024/09/22</date>
    /// <author>Ryokugyoku</author>
    /// <param name="phoneNumber"></param>
    /// <returns></returns>
    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        try
        {
            // 正規表現パターンを定義
            var regex = new Regex(@"^\+?[1-9]\d{1,14}$", RegexOptions.Compiled);
            return regex.IsMatch(phoneNumber);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// UserIdが数字、英文字、およびアンダースコアのみを含むかどうかを検証します。
    /// </summary>
    /// <param name="userId">検証するUserId</param>
    /// <returns>有効な場合はtrue、無効な場合はfalse</returns>
    public static  bool IsValidUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        try
        {
            // 正規表現パターンを定義
            var regex = new Regex(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);
            return regex.IsMatch(userId);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}