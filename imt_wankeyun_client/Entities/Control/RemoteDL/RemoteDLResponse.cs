using System.Collections.Generic;

namespace imt_wankeyun_client.Entities.Control.RemoteDL
{
    public class DcdnChannel
    {
        public int available { get; set; }
        public int state { get; set; }
        public int failCode { get; set; }
        public int speed { get; set; }
        public object dlBytes { get; set; }
    }

    public class DlTask
    {
        public int failCode { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public int speed { get; set; }
        public int downTime { get; set; }
        public List<object> subList { get; set; }
        public int createTime { get; set; }
        public DcdnChannel dcdnChannel { get; set; }
        public int state { get; set; }
        public int remainTime { get; set; }
        public int progress { get; set; }
        public string path { get; set; }
        public int type { get; set; }
        public string id { get; set; }
        public int completeTime { get; set; }
        public object size { get; set; }
    }

    public class RemoteDLResponse
    {
        public int recycleNum { get; set; }
        public int serverFailNum { get; set; }
        public int rtn { get; set; }
        public int completeNum { get; set; }
        public int sync { get; set; }
        public List<DlTask> tasks { get; set; }
        public int dlNum { get; set; }
    }
}
