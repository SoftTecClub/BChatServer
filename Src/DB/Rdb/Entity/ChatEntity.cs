namespace BChatServer.Src.DB.Rdb.Entity
{
    /// <summary>
    /// チャットエンティティ
    /// </summary>
    public class ChatEntity
    {
        /// <summary>
        /// チャットID,
        /// チャットIDを検索することで、そのチャットに参加しているユーザを取得できる
        /// </summary>
        public string ChatId { get; set; } = String.Empty;
        /// <summary>
        /// ユーザID
        /// </summary>
        public string UserId { get; set; } = String.Empty;

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}