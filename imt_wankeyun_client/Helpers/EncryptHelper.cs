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
        /// <summary>RC4加密算法
        /// 返回进过rc4加密过的字符
        /// </summary>
        /// <param name="str">被加密的字符</param>
        /// <param name="ckey">密钥</param>
        public static string EncryptRC4(string str, string ckey)
        {
            int[] s = new int[256];
            for (int i = 0; i < 256; i++)
            {
                s[i] = i;
            }
            //密钥转数组
            char[] keys = ckey.ToCharArray();//密钥转字符数组
            int[] key = new int[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                key[i] = keys[i];
            }
            //明文转数组
            char[] datas = str.ToCharArray();
            int[] mingwen = new int[datas.Length];
            for (int i = 0; i < datas.Length; i++)
            {
                mingwen[i] = datas[i];
            }

            //通过循环得到256位的数组(密钥)
            int j = 0;
            int k = 0;
            int length = key.Length;
            int a;
            for (int i = 0; i < 256; i++)
            {
                a = s[i];
                j = (j + a + key[k]);
                if (j >= 256)
                {
                    j = j % 256;
                }
                s[i] = s[j];
                s[j] = a;
                if (++k >= length)
                {
                    k = 0;
                }
            }
            //根据上面的256的密钥数组 和 明文得到密文数组
            int x = 0, y = 0, a2, b, c;
            int length2 = mingwen.Length;
            int[] miwen = new int[length2];
            for (int i = 0; i < length2; i++)
            {
                x = x + 1;
                x = x % 256;
                a2 = s[x];
                y = y + a2;
                y = y % 256;
                s[x] = b = s[y];
                s[y] = a2;
                c = a2 + b;
                c = c % 256;
                miwen[i] = mingwen[i] ^ s[c];
            }
            //密文数组转密文字符
            char[] mi = new char[miwen.Length];
            for (int i = 0; i < miwen.Length; i++)
            {
                mi[i] = (char)miwen[i];
            }
            string miwenstr = new string(mi);
            return miwenstr;
        }

        /// <summary>RC4解密算法
        /// 返回进过rc4解密过的字符
        /// </summary>
        /// <param name="str">被解密的字符</param>
        /// <param name="ckey">密钥</param>
        public static string DecryptRC4(string str, string ckey)
        {
            int[] s = new int[256];
            for (int i = 0; i < 256; i++)
            {
                s[i] = i;
            }
            //密钥转数组
            char[] keys = ckey.ToCharArray();//密钥转字符数组
            int[] key = new int[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                key[i] = keys[i];
            }
            //密文转数组
            char[] datas = str.ToCharArray();
            int[] miwen = new int[datas.Length];
            for (int i = 0; i < datas.Length; i++)
            {
                miwen[i] = datas[i];
            }

            //通过循环得到256位的数组(密钥)
            int j = 0;
            int k = 0;
            int length = key.Length;
            int a;
            for (int i = 0; i < 256; i++)
            {
                a = s[i];
                j = (j + a + key[k]);
                if (j >= 256)
                {
                    j = j % 256;
                }
                s[i] = s[j];
                s[j] = a;
                if (++k >= length)
                {
                    k = 0;
                }
            }
            //根据上面的256的密钥数组 和 密文得到明文数组
            int x = 0, y = 0, a2, b, c;
            int length2 = miwen.Length;
            int[] mingwen = new int[length2];
            for (int i = 0; i < length2; i++)
            {
                x = x + 1;
                x = x % 256;
                a2 = s[x];
                y = y + a2;
                y = y % 256;
                s[x] = b = s[y];
                s[y] = a2;
                c = a2 + b;
                c = c % 256;
                mingwen[i] = miwen[i] ^ s[c];
            }
            //明文数组转明文字符
            char[] ming = new char[mingwen.Length];
            for (int i = 0; i < mingwen.Length; i++)
            {
                ming[i] = (char)mingwen[i];
            }
            string mingwenstr = new string(ming);
            return mingwenstr;
        }
    }
}
