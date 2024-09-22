namespace BChatServer.Src.Model
{
    /// <summary>
    /// ユーザ登録時のレスポンスモデル
    /// デフォルトは全てFalseに設定されている
    /// <date>2024/09/22</date>
    /// <author>Ryokugyoku</author>
    /// </summary>
    public class UserRegisterResponseModel
    {
        /// <summary>
        /// ユーザ名
        /// </summary>
        public bool NameIsError { get; set; } = false;
        /// <summary>
        /// ログインId
        /// </summary>
        public bool UserIdIsError { get; set; } = false;
        /// <summary>
        /// メールアドレス
        /// </summary>
        public bool EmailIsError { get; set; } = false;
        /// <summary>
        /// 電話番号
        /// </summary>
        public bool PhoneNumberIsError { get; set; } = false;
    }
}