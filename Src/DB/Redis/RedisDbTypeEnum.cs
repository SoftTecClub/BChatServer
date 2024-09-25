namespace BChatServer.Src.DB.Redis
{
    /// <summary>
    /// RedisのDB種別
    /// </summary>
    public enum RedisDbTypeEnum
    {
        /// <summary>
        /// アクセストークン用
        /// </summary>
        AccessToken = 0,
        /// <summary>
        /// チャット用
        /// </summary>
        Chat = 1
    }
}