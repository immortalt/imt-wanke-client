using imt_wankeyun_client.Entities.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imt_wankeyun_client.Entities
{
    public class WankeSettings
    {
        public bool autoRefresh { get; set; }
        public List<LoginData> loginDatas { get; set; }
    }
}
