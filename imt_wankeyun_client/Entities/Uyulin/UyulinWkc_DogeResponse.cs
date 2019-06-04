using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Uyulin
{
    public class Body
    {
        public string marketId { get; set; }
        public string usname { get; set; }
        public string ename { get; set; }
        public string coinImg { get; set; }
        public string coincode { get; set; }
        public string gCoincode { get; set; }
        public double newPrice { get; set; }
        public string fCoincode { get; set; }
        public double mchange { get; set; }
        public double buyPrice { get; set; }
        public double sellPrice { get; set; }
        public double volume { get; set; }
        public int minPrice { get; set; }
        public int maxPrice { get; set; }
        public int round { get; set; }
        public int closePrice { get; set; }
        public int feeSell { get; set; }
        public int feeBuy { get; set; }
    }

    public class UyulinWkc_DogeResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Body> body { get; set; }
    }
}
