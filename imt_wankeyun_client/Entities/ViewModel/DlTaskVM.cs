using imt_wankeyun_client.Entities.Control.RemoteDL;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace imt_wankeyun_client.Entities.ViewModel
{
    public class DlTaskVM
    {
        public int failCode { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string speed { get; set; }
        public int downTime { get; set; }
        public List<object> subList { get; set; }
        public int createTime { get; set; }
        public DcdnChannel dcdnChannel { get; set; }
        public string state { get; set; }
        public string state_color { get; set; }
        public BitmapImage state_img { get; set; }
        public int remainTime { get; set; }
        public string progress { get; set; }
        public string path { get; set; }
        public int type { get; set; }
        public string id { get; set; }
        public int completeTime { get; set; }
        public object size { get; set; }
    }
}
