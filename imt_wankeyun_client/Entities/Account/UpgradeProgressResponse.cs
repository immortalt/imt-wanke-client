using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Account
{
    public class UpgradeCheckResult
    {
        public string app { get; set; }
        public string description { get; set; }
    }
    public class UpgradeProgressResult
    {
        public string message { get; set; }
        public string name { get; set; }
    }

    public class UpgradeResponse
    {
        public string msg { get; set; }
        public int rtn { get; set; }
        public List<object> result { get; set; }
    }
}
