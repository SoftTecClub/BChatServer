using BChatServer.Src.Common;


/// <summary>
/// ユーザ登録情報を含むモデル
/// </summary>
public class UserRegisterReceiveModel
{
    private string _name = string.Empty;
    private string _userId = string.Empty;
    private string _email = string.Empty;
    private string _phoneNumber = string.Empty;

    /// <summary>
    /// ユーザ名
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = SecurityCommonFunc.SanitizeInput(value);
    }

    /// <summary>
    /// ログインId
    /// </summary>
    public string UserId
    {
        get => _userId;
        set => _userId = SecurityCommonFunc.SanitizeInput(value);
    }

    /// <summary>
    /// メールアドレス
    /// </summary>
    public string Email
    {
        get => _email;
        set => _email = SecurityCommonFunc.SanitizeInput(value);
    }

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
    public string PhoneNumber
    {
        get => _phoneNumber;
        set => _phoneNumber = SecurityCommonFunc.SanitizeInput(value);
    }
}