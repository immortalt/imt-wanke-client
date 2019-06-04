using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace imt_wankeyun_client.Helpers
{
    public class HttpHelper
    {
        public static async Task<BitmapImage> GetPicAsync(string url)
        {
            try
            {
                var imagedata = await HttpHelper.DownloadDataAsync(url);
                using (var ms = new MemoryStream(imagedata))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch
            {
                return null;
            }
        }
        public static Task<byte[]> DownloadDataAsync(string url)
        {
            return Task.Run(() =>
            {
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                return client.DownloadData(request);
            });
        }
        public static Task<IRestResponse> GetAsync(string url)
        {
            return Task.Run(() =>
            {
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("accept-language", "zh-CN,zh;q=0.9");
                request.AddHeader("accept-encoding", "gzip, deflate");
                request.AddHeader("dnt", "1");
                request.AddHeader("accept", "application/javascript,text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                request.AddHeader("upgrade-insecure-requests", "1");
                request.AddHeader("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.89 Safari/537.36");
                return client.Execute(request);
            });
        }
    }
}
