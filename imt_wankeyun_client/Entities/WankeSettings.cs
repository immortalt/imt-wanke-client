using imt_wankeyun_client.Entities.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imt_wankeyun_client.Entities
{
    public class WankeSettings
    {
        public WankeSettings()
        {
            loginDatas = new List<LoginData>();
            mailAccount = new MailAccount
            {
                port = 25,
                smtpServer = "smtp.qq.com"
            };
        }
        public bool autoRefresh { get; set; }
        public bool mailNotify { get; set; }
        public List<LoginData> loginDatas { get; set; }
        public MailAccount mailAccount { get; set; }
    }
    public class MailAccount
    {
        public string username { get; set; }
        public string password { get; set; }
        public string smtpServer { get; set; }
        public int port { get; set; }
    }
}
