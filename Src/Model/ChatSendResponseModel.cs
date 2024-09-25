namespace BChatServer.Src.Model
{
    /// <summary>
    /// チャット送信レスポンスモデル
    /// ChatSendResponseを複数件
    /// </summary>
    public class ChatSendResponseModel{
        /// <summary>
        /// チャットリスト
        /// </summary>
        public List<ChatSendResponse> ChatSendResponses { get; set; } = new List<ChatSendResponse>();
    }
    /// <summary>
    /// チャット作成レスポンスモデル
    /// </summary>
    public class ChatSendResponse
    {
        /// <summary>
        /// トークン
        /// </summary>
        public string ChatId { get; set; } = String.Empty;

        /// <summary>
        /// ユーザID    
        /// </summary>
        public string UserId { get; set; } = String.Empty;

        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; } = String.Empty;

        /// <summary>
        /// 送信日時
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}