using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace imt_wanke_client.Entities
{
    public class Ads
    {
        public int code { get; set; }
        public List<Ad> data { get; set; }
    }
}