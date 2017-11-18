using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Monitor
{
    public class TopNWkb
    {
        public long user_id { get; set; }
        public long wkb { get; set; }
        public string phone { get; set; }
    }

    public class TopNBandwidth
    {
        public long bandwidth { get; set; }
        public long user_id { get; set; }
        public double wkb { get; set; }
        public string phone { get; set; }
    }

    public class TopNDisk
    {
        public long disk { get; set; }
        public long user_id { get; set; }
        public double wkb { get; set; }
        public string phone { get; set; }
    }

    public class WkbInfo
    {
        public long block_num { get; set; }
        public long wkb_num { get; set; }
        public long average_onlinetime { get; set; }
        public long average_bandwidth { get; set; }
        public long average_disk { get; set; }
        public List<TopNWkb> topN_wkb { get; set; }
        public List<TopNBandwidth> topN_bandwidth { get; set; }
        public List<TopNDisk> topN_disk { get; set; }
    }

    public class WkbInfoQueryResponse
    {
        public long iRet { get; set; }
        public string sMsg { get; set; }
        public WkbInfo data { get; set; }
    }
}
