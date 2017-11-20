using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Suiqiu
{
    public class SuiqiuPrice
    {
        public string updatetime { get; set; }
        public double change_24h { get; set; }
        public string vol { get; set; }
        public int cny_rate { get; set; }
        public double price { get; set; }
        public int pair_code { get; set; }
        public double change_7d { get; set; }
    }

    public class SuiqiuPriceResponse
    {
        public List<SuiqiuPrice> data { get; set; }
        public bool result { get; set; }
    }
}
