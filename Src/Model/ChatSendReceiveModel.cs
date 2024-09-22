namespace BChatServer.Src.Model
{
    /// <summary>
    /// チャット送受信モデル
    /// </summary>
    public class ChatSendReceiveModel
    {
        /// <summary>
        /// 送信者ID
        /// </summary>
        public string ChatId { get; set; } = String.Empty;

        /// <summary>
        /// トークン
        /// </summary>
        public string Token { get; set; } = String.Empty;
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; } = String.Empty;
    }
}