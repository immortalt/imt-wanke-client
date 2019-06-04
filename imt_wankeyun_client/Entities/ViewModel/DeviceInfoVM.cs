using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.ViewModel
{
    public class DeviceInfoVM
    {
        public string phone { get; set; }
        //public int bind_pwd { get; set; }
        public string nickname { get; set; }
        public string device_name { get; set; }
        public string ip { get; set; }
        public string lan_ip { get; set; }       
        public string status { get; set; }
        public string status_color { get; set; }
        public string dcdn_upnp_status { get; set; }
        public string system_version { get; set; }
        //public string dcdn_download_speed { get; set; }
        //public string dcdn_upload_speed { get; set; }
        //public string exception_message { get; set; }
        public string onecloud_coin { get; set; }
        public string dcdn_clients_count { get; set; }
        public string dcdn_upnp_message { get; set; }
        public string upgradeable { get; set; }
        public string ip_info { get; set; }
        public string yes_wkb_color { get; set; }
        public double yes_wkb { get; set; }
        public double wkb_yes_before { get; set; }
        public int activate_days { get; set; }
        public double totalIncome { get; set; }
        public string isActived { get; set; }
        public string volume { get; set; }
        public string volume_color { get; set; }
        public double ketiWkb { get; set; }
        public string device_sn { get; set; }
        public System.Windows.Visibility showUpgrade { get; set; }
        public string wkbAddr { get; set; }

    }
}
