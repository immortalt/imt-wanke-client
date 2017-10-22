using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Control.RemoteDL
{
    public class CreateTask
    {
        public int filesize { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class CreateTaskInfo
    {
        public string path { get; set; }
        public List<CreateTask> tasks { get; set; }
    }
}
