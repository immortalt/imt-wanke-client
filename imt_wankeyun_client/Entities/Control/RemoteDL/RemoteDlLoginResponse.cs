using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Control.RemoteDL
{
    public class RemoteDlLoginResponse
    {
        public int rtn { get; set; }
        public List<string> pathList { get; set; }
        public int clientVersion { get; set; }
    }
}
