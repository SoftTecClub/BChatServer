namespace BChatServer.Src.Model
{
    /// <summary>
    /// チャット作成レスポンスモデル
    /// </summary>
    public class ChatCreateResponseModel
    {
        /// <summary>
        /// トークン
        /// </summary>
        public string ChatId { get; set; } = String.Empty;
    }
}