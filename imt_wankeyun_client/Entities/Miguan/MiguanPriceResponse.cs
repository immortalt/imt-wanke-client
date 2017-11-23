using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Miguan
{
    public class Dict
    {
        public string code { get; set; }
        public string name { get; set; }
        public double rate { get; set; }
        public int status { get; set; }
        public string url { get; set; }
        public string urls { get; set; }
    }

    public class Result
    {
        public double? buy { get; set; }
        public double change { get; set; }
        public double cnyPrice { get; set; }
        public object createTime { get; set; }
        public Dict dict { get; set; }
        public int mark { get; set; }
        public double? sell { get; set; }
        public int status { get; set; }
        public double? total { get; set; }
        public double turnover { get; set; }
    }

    public class MiguanPriceResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
        public List<Result> result { get; set; }
        public bool success { get; set; }
    }
}
