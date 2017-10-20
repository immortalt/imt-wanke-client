using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
namespace imt_wankeyun_client.Helpers
{
    class EncryptHelper
    {
        private static byte[] Keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };//密钥
        #region DES加密字符串
        ///<summary>   
        ///DES加密字符串   
        ///</summary>   
        ///<param name="str">待加密的字符串</param>   
        ///<param name="key">加密密钥,要求为8位</param>   
        ///<returns>加密成功返回加密后的字符串，失败返回源字符串</returns>   
        public static string EncryptDES(string str, string key)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(key.Substring(0, 8));
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(str);
                DESCryptoServiceProvider myDES = new DESCryptoServiceProvider();
                MemoryStream MStream = new MemoryStream();
                CryptoStream CStream = new CryptoStream(MStream, myDES.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                CStream.Write(inputByteArray, 0, inputByteArray.Length);
                CStream.FlushFinalBlock();
                return Convert.ToBase64String(MStream.ToArray());
            }
            catch
            {
                return str;
            }
        }
        #endregion

        #region DES解密字符串
        ///<summary>   
        ///DES解密字符串   
        ///</summary>   
        ///<param name="str">待解密的字符串</param>   
        ///<param name="key">解密密钥,要求为8位,和加密密钥相同</param>   
        ///<returns>解密成功返回解密后的字符串，失败返源字符串</returns>   
        public static string DecryptDES(string str, string key)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(key);
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Convert.FromBase64String(str);
                DESCryptoServiceProvider myDES = new DESCryptoServiceProvider();
                MemoryStream MStream = new MemoryStream();
                CryptoStream CStream = new CryptoStream(MStream, myDES.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                CStream.Write(inputByteArray, 0, inputByteArray.Length);
                CStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(MStream.ToArray());
            }
            catch
            {
                return str;
            }
        }
        #endregion
        public static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider(); rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
