namespace BChatServer.Src.Model;

/// <summary>
/// 新規でチャットを開始するとき
/// </summary>
public class ChatCreateReceiveModel
{
    /// <summary>
    /// ユーザID
    /// </summary>
    public string ToUserId { get; set; } = String.Empty;
    /// <summary>
    /// トークン
    /// </summary>
    public string Token { get; set; } = String.Empty;
}