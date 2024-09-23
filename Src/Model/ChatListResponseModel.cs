namespace BChatServer.Src.Model
{
    /// <summary>
    /// チャットリストレスポンスモデル
    /// </summary>
    public class ChatListResponseModel
    {
        /// <summary>
        /// チャットリスト
        /// </summary>
        public List<string> chatIds { get; set; } = new List<string>();
    }
}