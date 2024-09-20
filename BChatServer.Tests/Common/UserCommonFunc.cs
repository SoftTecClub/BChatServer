using BChatServer.Src.DB.Rdb.Entity;

namespace BChatServer.Tests.Common
{
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
                    UserId = CommonFunc.GenerateRandomString(10),
                    Name = CommonFunc.GenerateRandomString(10),
                    Email = CommonFunc.GenerateRandomString(10),
                    Password = CommonFunc.GenerateRandomString(10)
                };
                users.Add(user);
            }
            
            return users;
        }

        
    }


    
}
