using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Uyulin
{
    public class RfcDoge
    {
        public string price { get; set; }
    }

    public class LtcDoge
    {
        public string price { get; set; }
    }

    public class EthDoge
    {
        public string price { get; set; }
    }

    public class BtcDoge
    {
        public string price { get; set; }
    }

    public class WkcDoge
    {
        public string price { get; set; }
    }

    public class TrfcLtc
    {
        public string price { get; set; }
    }

    public class BtcLtc
    {
        public string price { get; set; }
    }

    public class EthLtc
    {
        public string price { get; set; }
    }

    public class TwkcBtc
    {
        public string price { get; set; }
    }

    public class DogeLtc
    {
        public string price { get; set; }
    }

    public class WkcLtc
    {
        public string price { get; set; }
    }

    public class Menu
    {
        public RfcDoge rfc_doge { get; set; }
        public LtcDoge ltc_doge { get; set; }
        public EthDoge eth_doge { get; set; }
        public BtcDoge btc_doge { get; set; }
        public WkcDoge wkc_doge { get; set; }
        public TrfcLtc trfc_ltc { get; set; }
        public BtcLtc btc_ltc { get; set; }
        public EthLtc eth_ltc { get; set; }
        public TwkcBtc twkc_btc { get; set; }
        public DogeLtc doge_ltc { get; set; }
        public WkcLtc wkc_ltc { get; set; }
    }

    public class Depth
    {
        public List<List<string>> b { get; set; }
        public List<List<string>> s { get; set; }
    }

    public class UyulinWkc_DogeResponse
    {
        public Menu menu { get; set; }
        public List<string> top { get; set; }
        public Depth depth { get; set; }
        public List<List<object>> trades { get; set; }
    }
}
