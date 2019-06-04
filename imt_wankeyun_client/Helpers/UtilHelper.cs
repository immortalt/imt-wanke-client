using imt_wankeyun_client.Entities.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace imt_wankeyun_client.Helpers
{
    public class UtilHelper
    {
        public static string ConvertToSizeString(ulong size)
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
        public static string ConvertToSpeedString(long speed)
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
        internal static int CompareString(string fileA, string fileB)
        {
            try
            {
                char[] arr1 = fileA.ToCharArray();
                char[] arr2 = fileB.ToCharArray();
                long i = 0, j = 0;
                while (i < arr1.Length && j < arr2.Length)
                {
                    if (char.IsDigit(arr1[i]) && char.IsDigit(arr2[j]))
                    {
                        string s1 = "", s2 = "";
                        while (i < arr1.Length && char.IsDigit(arr1[i]))
                        {
                            s1 += arr1[i];
                            i++;
                        }
                        while (j < arr2.Length && char.IsDigit(arr2[j]))
                        {
                            s2 += arr2[j];
                            j++;
                        }
                        if (Convert.ToInt64(s1) > Convert.ToInt64(s2))
                        {
                            return 1;
                        }
                        if (Convert.ToInt64(s1) < Convert.ToInt64(s2))
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        if (arr1[i] > arr2[j])
                        {
                            return 1;
                        }
                        if (arr1[i] < arr2[j])
                        {
                            return -1;
                        }
                        i++;
                        j++;
                    }
                }
                if (arr1.Length == arr2.Length)
                {
                    return 0;
                }
                else
                {
                    return arr1.Length > arr2.Length ? 1 : -1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("排序错误:" + ex.Message);
                return 0;
            }
        }
        ///<summary>
        ///主要用于设备名称的比较。
        ///</summary>
        public class DeviceNameComparerClass : IComparer<DeviceInfoVM>
        {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            ///<summary>
            ///比较两个字符串，如果含用数字，则数字按数字的大小来比较。
            ///</summary>
            ///<param name="x"></param>
            ///<param name="y"></param>
            ///<returns></returns>
            int IComparer<DeviceInfoVM>.Compare(DeviceInfoVM x, DeviceInfoVM y)
            {
                if (x == null || y == null)
                    throw new ArgumentException("Parameters can't be null");
                string fileA = x.device_name;
                string fileB = y.device_name;
                return CompareString(fileA, fileB);
            }
        }
    }
}
