using System.Security.Cryptography;
using System.Text;

namespace BChatServer.Src.Common;

public class UserCommonFunc
{
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