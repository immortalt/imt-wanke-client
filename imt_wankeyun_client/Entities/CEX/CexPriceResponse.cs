using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.CEX
{
    public class __invalid_type__1
    {
        public string num { get; set; }
        public string trade_num { get; set; }
        public string price { get; set; }
        public int type { get; set; }
        public string color { get; set; }
        public string amount { get; set; }
        public string total { get; set; }
        public double sum { get; set; }
    }

    public class Trade
    {
        public string num { get; set; }
        public string trade_num { get; set; }
        public string price { get; set; }
        public int type { get; set; }
        public string color { get; set; }
        public string amount { get; set; }
        public string total { get; set; }
        public double sum { get; set; }
    }

    public class Depth
    {
        public List<Trade> buy { get; set; }
        public List<Trade> sell { get; set; }
    }

    public class Ctrade
    {
        public string add_time { get; set; }
        public string type { get; set; }
        public double price { get; set; }
        public string num { get; set; }
        public double money { get; set; }
        public double amount { get; set; }
        public string time { get; set; }
    }

    public class Cmark
    {
        public double new_price { get; set; }
        public string currency_id { get; set; }
        public string currency_mark { get; set; }
        public string currency_logo { get; set; }
        public string currency_name { get; set; }
        public string symbol { get; set; }
        public string tcid { get; set; }
        public string tmark { get; set; }
        public int nstatus { get; set; }
        public double min_price { get; set; }
        public double max_price { get; set; }
        public string H24_change { get; set; }
        public int H24_done_num { get; set; }
        public int H24_done_money { get; set; }
        public double H24_money { get; set; }
    }

    public class CexPriceResponse
    {
        public Depth depth { get; set; }
        public List<Ctrade> ctrade { get; set; }
        public Cmark cmark { get; set; }
    }
}
