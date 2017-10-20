using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imt_wankeyun_client.Entities.Account
{
    public class PopFrame
    {
        public int code { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string button1 { get; set; }
        public string button2 { get; set; }
    }

    public class UserBasicData
    {
        public string userid { get; set; }
        public string phone { get; set; }
        public string account_type { get; set; }
        public int bind_pwd { get; set; }
        public string nickname { get; set; }
        public PopFrame pop_frame { get; set; }
    }

    public class LoginResponse
    {
        public int iRet { get; set; }
        public string sMsg { get; set; }
        public UserBasicData data { get; set; }
    }
}
