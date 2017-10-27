using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.WKB
{
    public class DrawWkbData
    {
        public string drawMsg { get; set; }
        public int drawRet { get; set; }
    }

    public class DrawWkbResponse
    {
        public int iRet { get; set; }
        public string sMsg { get; set; }
        public DrawWkbData data { get; set; }
    }
}
