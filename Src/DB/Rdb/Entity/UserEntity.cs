using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BChatServer.Src.DB.Rdb.Entity
{
    /// <summary>
    /// ユーザデータを格納するためのエンティティ
    /// </summary>
    public class UserEntity
    {
        /// <summary>
        /// 管理用固有ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ログイン用のユーザID
        /// </summary>
        public string UserId { get; set; } = String.Empty;

        /// <summary>
        /// メールアドレス
        /// </summary>
        public string Email { get; set; } = String.Empty;

        /// <summary>
        /// ユーザ名
        /// </summary>
        public string Name { get; set; } = String.Empty;
        
        /// <summary>
        /// パスワード、サーバ側ではハッシュ化されているものが保存されている
        /// </summary>
        public string Password { get; set; } = String.Empty;

        /// <summary>
        /// 電話番号
        /// </summary>
        public string PhoneNumber { get; set; } = String.Empty;

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// チャット一覧
        /// </summary>
        public List<ChatEntity> Chats { get; set; } = new List<ChatEntity>();
    }
}