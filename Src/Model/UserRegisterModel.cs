using BChatServer.Src.Common;

namespace BChatServer.Src.Model;
public class UserRegisterModel
{
    /// <summary>
    /// ユーザ名
    /// </summary>
    public string Name { get; set; } = String.Empty;
    /// <summary>
    /// ログインId
    /// </summary>
    public string userId { get; set; } = String.Empty;
    /// <summary>
    /// メールアドレス
    /// </summary>
    public string Email { get; set; } = String.Empty;
    private string _password = string.Empty;

    /// <summary>
    /// パスワード
    /// 代入されたパスワードは自動的にハッシュ化される
    /// </summary>
    public string Password
    {
        get => _password;
        set => _password = UserCommonFunc.HashPassword(value);
    }
    /// <summary>
    /// 電話番号
    /// </summary>
    public string PhoneNumber { get; set; } = String.Empty;

}