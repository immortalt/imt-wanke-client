using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.ServerChan
{
    public class ServerChanResponse
    {
        public int errno { get; set; }
        public string errmsg { get; set; }
        public string dataset { get; set; }
    }
}
