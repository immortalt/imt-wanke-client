using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Control.RemoteDL
{
    public class TaskInfo
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
        public int size { get; set; }
    }

    public class TaskInfoResponse
    {
        public int rtn { get; set; }
        public string infohash { get; set; }
        public TaskInfo taskInfo { get; set; }
    }
}
