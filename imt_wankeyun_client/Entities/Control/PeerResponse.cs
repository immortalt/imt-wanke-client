using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imt_wankeyun_client.Entities.Control
{
    public class Features
    {
        public long onecloud_coin { get; set; }
        public long miner { get; set; }
    }

    public class IpInfo
    {
        public string province { get; set; }
        public string country { get; set; }
        public string isp { get; set; }
        public string city { get; set; }
    }

    public class DcdnClient
    {
        public string status { get; set; }
        public long download_speed_max { get; set; }
        public string login_error { get; set; }
        public string login_status { get; set; }
        public string space_quota { get; set; }
        public long upload_speed_max { get; set; }
        public string data_path { get; set; }
        public long upload_speed { get; set; }
        public string device_sn { get; set; }
        public string space_used { get; set; }
        public long download_speed { get; set; }
        public string dcdn_id { get; set; }
        public string device_id { get; set; }
    }

    public class Params
    {
    }

    public class ScheduleHour
    {
        public long to { get; set; }
        public long from { get; set; }
        public Params @params { get; set; }
        public string type { get; set; }
    }

    public class Device
    {
        public long bind_time { get; set; }
        public Features features { get; set; }
        public IpInfo ip_info { get; set; }
        public string ip { get; set; }
        public long coturn_online { get; set; }
        public bool paused { get; set; }
        public string dcdn_upnp_message { get; set; }
        public string device_sn { get; set; }
        public string exception_message { get; set; }
        public string account_name { get; set; }
        public List<DcdnClient> dcdn_clients { get; set; }
        public bool hibernated { get; set; }
        public string imported { get; set; }
        public string exception_name { get; set; }
        public string hardware_model { get; set; }
        public string mac_address { get; set; }
        public string status { get; set; }
        public string lan_ip { get; set; }
        public string account_type { get; set; }
        public string account_id { get; set; }
        public bool upgradeable { get; set; }
        public long dcdn_download_speed { get; set; }
        public long dcdn_upload_speed { get; set; }
        public long disk_quota { get; set; }
        public string peerid { get; set; }
        public string dcdn_id { get; set; }
        public string system_version { get; set; }
        public string device_id { get; set; }
        public string system_name { get; set; }
        public string dcdn_upnp_status { get; set; }
        public long product_id { get; set; }
        public string device_name { get; set; }
        public List<ScheduleHour> schedule_hours { get; set; }
    }

    public class Devices
    {
        public List<Device> devices { get; set; }
    }

    public class PeerResponse
    {
        public long rtn { get; set; }
        public List<object> result { get; set; }
    }
}
