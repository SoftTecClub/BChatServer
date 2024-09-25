using BChatServer.Src.DB.Rdb.Entity;

namespace BChatServer.Tests.Common
{
    /// <summary>
    /// ユーザ関連の共通関数
    /// <date>2024/09/21</date>
    /// <author>Ryokugyoku</author>
    /// </summary>
    public static class UserCommonFunc
    {
        /// <summary>
        /// テスト用ユーザの作成
        /// パスワードはハッシュ化されている状態で作成を行う
        /// </summary>
        /// <param name="recordNum">作成するレコード件数</param>
        /// <returns></returns>
        public static  List<UserEntity> CreateUserEntity(int recordNum)
        {
            List<UserEntity> users = new ();
            foreach(var i in Enumerable.Range(0, recordNum))
            {
                var user = new UserEntity
                {
                    UserId = "userId"+CommonFunc.GenerateRandomString(5),
                    Name = "name"+CommonFunc.GenerateRandomString(5),
                    Email = CommonFunc.GenerateRandomEmail(),
                    Password = CommonFunc.GenerateRandomString(10),
                    PhoneNumber = CommonFunc.GenerateRandomE164PhoneNumber(),
                };
                users.Add(user);
            }
            
            return users;
        }

        
    }


    
}
