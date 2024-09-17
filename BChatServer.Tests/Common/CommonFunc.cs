namespace BChatServer.Tests.Common;
using System.Security.Cryptography;
using System.Text;
public static class CommonFunc{

        private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// 文字列をランダム生成するメソッド
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GenerateRandomString(int length)
        {
            if (length <= 0)
                throw new ArgumentException("Length must be a positive number", nameof(length));

            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var result = new StringBuilder(length);
            foreach (var b in randomBytes)
            {
                result.Append(AllowedChars[b % AllowedChars.Length]);
            }

            return result.ToString();
        }
}