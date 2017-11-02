using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Control.RemoteDL
{
    public class SubList
    {
        public long status { get; set; }
        public long failCode { get; set; }
        public string name { get; set; }
        public long selected { get; set; }
        public object exist { get; set; }
        public long progress { get; set; }
        public string id { get; set; }
        public long size { get; set; }
    }

    public class TaskInfo
    {
        public long failCode { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public long speed { get; set; }
        public long downTime { get; set; }
        public List<SubList> subList { get; set; }
        public long createTime { get; set; }
        public DcdnChannel dcdnChannel { get; set; }
        public long state { get; set; }
        public object exist { get; set; }
        public long remalongime { get; set; }
        public long progress { get; set; }
        public string path { get; set; }
        public long type { get; set; }
        public string id { get; set; }
        public long completeTime { get; set; }
        public long size { get; set; }
    }

    public class TaskInfoResponse
    {
        public long rtn { get; set; }
        public string infohash { get; set; }
        public TaskInfo taskInfo { get; set; }
    }
}
