using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Account
{
    public class UsbInfoPartition
    {
        public ulong used { get; set; }
        public ulong capacity { get; set; }
        public int disk_id { get; set; }
        public string label { get; set; }
        public string path { get; set; }
        public int unique { get; set; }
        public int id { get; set; }
    }

    public class UsbInfoPartitions
    {
        public List<UsbInfoPartition> partitions { get; set; }
    }

    public class UsbInfoResponse
    {
        public string msg { get; set; }
        public int rtn { get; set; }
        public List<object> result { get; set; }
    }
}
