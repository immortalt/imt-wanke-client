using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;

namespace imt_wankeyun_client.Helpers
{
    public class UtilHelper
    {
        public static string ConvertToSizeString(int size)
        {
            var GB = size / 1024d / 1024d / 1024d;
            if (GB < 1)
            {
                var MB = size / 1024d / 1024d;
                if (MB < 1)
                {
                    var KB = size / 1024d;
                    if (KB < 1)
                    {
                        return size.ToString("f2") + "B";
                    }
                    else
                    {
                        return KB.ToString("f2") + "KB";
                    }
                }
                else
                {
                    return MB.ToString("f2") + "MB";
                }
            }
            else
            {
                return GB.ToString("f2") + "GB";
            }
        }
        public static string ConvertToSpeedString(int speed)
        {
            var GB = speed / 1024d / 1024d / 1024d;
            if (GB < 1)
            {
                var MB = speed / 1024d / 1024d;
                if (MB < 1)
                {
                    var KB = speed / 1024d;
                    if (KB < 1)
                    {
                        return speed.ToString("f2") + "B/s";
                    }
                    else
                    {
                        return KB.ToString("f2") + "KB/s";
                    }
                }
                else
                {
                    return MB.ToString("f2") + "MB/s";
                }
            }
            else
            {
                return GB.ToString("f2") + "GB/s";
            }
        }
        public static string RandomCode(int length)
        {
            char[] arrChar = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder num = new StringBuilder();
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < length; i++)
            {
                num.Append(arrChar[rnd.Next(0, arrChar.Length)].ToString());
            }
            return num.ToString();
        }
        public static string SignPassword(string paramString)
        {
            if (paramString == null)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder(GetMD5(paramString));
            char c1 = sb[2];
            char c2 = sb[8];
            char c3 = sb[17];
            char c4 = sb[27];
            sb.Remove(27, 1);
            sb.Remove(17, 1);
            sb.Remove(8, 1);
            sb.Remove(2, 1);
            sb.Insert(2, c2);
            sb.Insert(8, c1);
            sb.Insert(17, c4);
            sb.Insert(27, c3);
            return GetMD5(sb.ToString());
        }
        public static string GetMD5(string source)
        {
            byte[] sor = Encoding.UTF8.GetBytes(source);
            MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(sor);
            StringBuilder strbul = new StringBuilder(40);
            for (int i = 0; i < result.Length; i++)
            {
                strbul.Append(result[i].ToString("x2"));//加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位
            }
            return strbul.ToString();
        }
    }
}
