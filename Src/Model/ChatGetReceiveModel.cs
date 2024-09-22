namespace BChatServer.Src.Model
{
    /// <summary>
    /// チャット受信モデル
    /// </summary>
    public class ChatGetReceiveModel
    {
        /// <summary>
        /// チャットID
        /// </summary>
        public string ChatId { get; set; } = String.Empty;
        /// <summary>
        /// トークン
        /// </summary>
        public string Token { get; set; } = String.Empty;
    }
}