using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Net;
using System.Windows.Threading;
using imt_wankeyun_client.Windows;
using imt_wankeyun_client.Helpers;
using imt_wankeyun_client.Entities.Account;
using System.Collections.ObjectModel;
using imt_wankeyun_client.Entities.Control;
using imt_wankeyun_client.Entities;
using imt_wankeyun_client.Entities.ViewModel;
using System.Threading.Tasks;
using imt_wankeyun_client.Entities.Account.Activate;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using imt_wankeyun_client.Entities.WKB;
using imt_wankeyun_client.Entities.Control.RemoteDL;
using System.Windows.Media.Imaging;
using System.Deployment.Application;
using Microsoft.VisualBasic;

namespace imt_wankeyun_client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        LoadingWindow ld;
        internal static string password;
        private ObservableCollection<DeviceInfoVM> _deviceInfos = null;
        private ObservableCollection<DlTaskVM> _dlTasks = null;
        private ObservableCollection<DlTaskVM> _dlTasks_finished = null;
        List<Income> dayIncomes = new List<Income>();
        private ObservableCollection<FileVM> _partitions = null;
        DispatcherTimer StatusTimer;
        DispatcherTimer RemoteDlTimer;
        internal static WankeSettings settings;
        internal static string curAccount = null;
        ObservableCollection<string> userList;
        Rect rcnormal;//定义一个全局rect记录还原状态下窗口的位置和大小
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                tbk_version.Text = GetEdition();
            }
            catch (Exception ex)
            {
                tbk_version.Text = "开发版";
                Debug.WriteLine(ex.Message);
            }
            LoadSettings();//载入设置
            StatusTimer = new DispatcherTimer();
            StatusTimer.Interval = TimeSpan.FromSeconds(15);
            StatusTimer.Tick += StatusTimer_Tick;
            RemoteDlTimer = new DispatcherTimer();
            RemoteDlTimer.Interval = TimeSpan.FromSeconds(5);
            RemoteDlTimer.Tick += RemoteDlTimer_Tick;
        }
        public static string GetEdition()
        {
            return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
        }
        /// <summary>
        /// 载入账号列表
        /// </summary>
        void LoadAccounts()
        {
            try
            {
                if (ApiHelper.userBasicDatas.Keys == null)
                {
                    return;
                }
                userList = new ObservableCollection<string>();
                ApiHelper.userBasicDatas.Keys.ToList().ForEach(t =>
                {
                    userList.Add(t);
                });
                cbx_curAccount.ItemsSource = userList;
                if (cbx_curAccount.Items != null && cbx_curAccount.Items.Count > 0)
                {
                    if (cbx_curAccount.SelectedIndex == -1)
                    {
                        cbx_curAccount.SelectedIndex = 0;//默认选中第一项
                    }
                    if (curAccount == null)
                    {
                        curAccount = cbx_curAccount.SelectedValue.ToString();
                    }
                    else
                    {
                        if (!ApiHelper.userBasicDatas.Keys.Contains(curAccount))
                        {
                            curAccount = cbx_curAccount.SelectedValue.ToString();
                        }
                        else
                        {
                            cbx_curAccount.SelectedIndex = ApiHelper.userBasicDatas.Keys.ToList().IndexOf(curAccount);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadAccounts " + ex.Message);
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(btu_max.Visibility == Visibility.Collapsed))
                {//如果不是最大化状态
                    this.DragMove();
                }
            }
            catch
            {
                // do nothing
            }
        }
        private void x_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void ___Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void s_Click(object sender, RoutedEventArgs e)
        {
            mainmenu.IsContextMenuOpen = true;
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow a = new AboutWindow();
            a.ShowDialog();
        }
        void AutoHeaderWidth(ListView lv)
        {
            //调整列宽  
            GridView gv = lv.View as GridView;
            if (gv != null)
            {
                foreach (GridViewColumn gvc in gv.Columns)
                {
                    gvc.Width = gvc.ActualWidth;
                    gvc.Width = Double.NaN;
                }
            }
        }
        async Task RefreshStatus()
        {
            if (ApiHelper.userBasicDatas.Count == 0)
            {
                deviceInfos = null;
                lv_DeviceStatus.ItemsSource = deviceInfos;
                AutoHeaderWidth(lv_DeviceStatus);
                return;
            }
            for (int i = 0; i < ApiHelper.userBasicDatas.Count; i++)
            {
                var t = ApiHelper.userBasicDatas.ElementAt(i);
                var phone = t.Key;
                var basic = t.Value;
                if (ld != null)
                {
                    ld.SetTitle($"正在获取数据");
                    ld.SetPgr(i, ApiHelper.userBasicDatas.Count);
                    ld.SetTip($"正在获取账号{phone}的数据");
                }
                if (await ListPeer(phone))
                {
                    if (await GetUserInfo(phone))
                    {
                        await GetIncomeHistory(phone);
                        await GetWkbAccountInfo(phone);
                        await GetUsbInfo(phone);
                    }
                }
                deviceInfos = null;
                lv_DeviceStatus.ItemsSource = deviceInfos;
                AutoHeaderWidth(lv_DeviceStatus);
            }
            var v = ApiHelper.incomeHistorys.Values;
            dayIncomes = null;
            dayIncomes = new List<Income>();
            for (int i = 0; i < v.Count; i++)
            {
                var t = v.ElementAt(i);
                var inc = t.incomeArr;
                foreach (var c in inc)
                {
                    if (dayIncomes.Where(tt => tt.date == c.date).Count() == 0)
                    {
                        dayIncomes.Add(c);
                    }
                    else
                    {
                        var ii = dayIncomes.Find(tt => tt.date == c.date);
                        ii.num = (Convert.ToDouble(ii.num) + Convert.ToDouble(c.num)).ToString();
                    }
                }
            }
            dayIncomes = dayIncomes.OrderBy(t => t.date).ToList();
        }
        async void RefreshRemoteDlStatus()
        {
            if (curAccount != null)
            {
                Debug.WriteLine("curAccount=" + curAccount);
                await RemoteDlLogin(curAccount);
                if (await GetRemoteDlInfo(curAccount, 0))
                {
                    dlTasks = null;
                    lv_remoteDlStatus.ItemsSource = dlTasks;
                }
            }
        }
        async void RefreshRemoteDlStatus_finished()
        {
            if (curAccount != null)
            {
                Debug.WriteLine("curAccount=" + curAccount);
                await RemoteDlLogin(curAccount);
                if (await GetRemoteDlInfo(curAccount, 1))
                {
                    dlTasks_finished = null;
                    lv_remoteDlStatus_finished.ItemsSource = dlTasks_finished;
                }
            }
        }
        async void RefreshFileStatus()
        {
            if (curAccount != null)
            {
                Debug.WriteLine("curAccount=" + curAccount);
                if (await GetUsbInfo(curAccount))
                {
                    partitions = null;
                    lv_file.ItemsSource = partitions;
                    AutoHeaderWidth(lv_file);
                }
            }
        }
        static bool IsLogined()
        {
            return curAccount != null && curAccount != "";
        }
        private void Btu_AddAccount_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow lw = new LoginWindow();
            lw.ShowDialog();
            if (LoginWindow.LoginSuccess)
            {
                LoadAccounts();
                RefreshStatus();
            }
        }
        async Task<bool> ListPeer(string phone)
        {
            HttpMessage resp = await ApiHelper.ListPeer(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    var pr = resp.data as PeerResponse;
                    if (pr.rtn == 0)
                    {
                        if (pr.result.Count > 1)
                        {
                            if (pr.result[1] != null)
                            {
                                Devices devices = JsonHelper.Deserialize<Devices>(pr.result[1].ToString());
                                var device = devices.devices[0];
                                if (device != null)
                                {
                                    if (!ApiHelper.userDevices.ContainsKey(phone))
                                    {
                                        ApiHelper.userDevices.Add(phone, device);
                                    }
                                    else
                                    {
                                        ApiHelper.userDevices[phone] = device;
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("获取数据出错！");
                    }
                    return false;
                default:
                    Debug.WriteLine("网络异常错误！");
                    //MessageBox.Show(resp.data.ToString(), "网络异常错误！");
                    return false;
            }
        }
        async Task<bool> GetUserInfo(string phone)
        {
            HttpMessage resp = await ApiHelper.GetUserInfo(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    var r = resp.data as UserInfoResponse;
                    if (r.iRet == 0)
                    {
                        var userInfo = r.data;
                        if (userInfo != null)
                        {
                            if (!ApiHelper.userInfos.ContainsKey(phone))
                            {
                                ApiHelper.userInfos.Add(phone, userInfo);
                            }
                            else
                            {
                                ApiHelper.userInfos[phone] = userInfo;
                            }
                            return true;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"GetUserInfo-获取数据出错{r.iRet}:{r.sMsg}");
                    }
                    return false;
                default:
                    Debug.WriteLine("GetUserInfo-网络异常错误！");
                    //MessageBox.Show(resp.data.ToString(), "网络异常错误！");
                    return false;
            }
        }
        async Task<bool> GetIncomeHistory(string phone, int page = 0)
        {
            HttpMessage resp = await ApiHelper.GetIncomeHistory(phone, page);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    var r = resp.data as IncomeHistoryResponse;
                    if (r.iRet == 0)
                    {
                        var incomeHistory = r.data;
                        if (incomeHistory != null)
                        {
                            if (!ApiHelper.incomeHistorys.ContainsKey(phone))
                            {
                                ApiHelper.incomeHistorys.Add(phone, incomeHistory);
                            }
                            else
                            {
                                ApiHelper.incomeHistorys[phone] = incomeHistory;
                            }
                            return true;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"GetIncomeHistory-获取数据出错{r.iRet}:{r.sMsg}");
                    }
                    return false;
                default:
                    Debug.WriteLine("GetIncomeHistory-网络异常错误！");
                    //MessageBox.Show(resp.data.ToString(), "网络异常错误！");
                    return false;
            }
        }
        async Task<bool> GetWkbAccountInfo(string phone)
        {
            HttpMessage resp = await ApiHelper.GetWkbAccountInfo(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    var r = resp.data as WkbAccountInfoResponse;
                    if (r.iRet == 0)
                    {
                        if (r.data != null)
                        {
                            if (!ApiHelper.wkbAccountInfos.ContainsKey(phone))
                            {
                                ApiHelper.wkbAccountInfos.Add(phone, r.data);
                            }
                            else
                            {
                                ApiHelper.wkbAccountInfos[phone] = r.data;
                            }
                            return true;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"GetWkbAccountInfo-获取数据出错{r.iRet}:{r.sMsg}");
                    }
                    return false;
                default:
                    Debug.WriteLine("GetWkbAccountInfo-网络异常错误！");
                    //MessageBox.Show(resp.data.ToString(), "网络异常错误！");
                    return false;
            }
        }
        async Task<bool> GetRemoteDlInfo(string phone, int type)
        {
            HttpMessage resp = await ApiHelper.GetRemoteDlInfo(phone, type);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        return false;
                    }
                    var r = resp.data as RemoteDLResponse;
                    if (r.rtn == 0)
                    {
                        if (type == 0)
                        {
                            if (!ApiHelper.remoteDlInfos.ContainsKey(phone))
                            {
                                ApiHelper.remoteDlInfos.Add(phone, r);
                            }
                            else
                            {
                                ApiHelper.remoteDlInfos[phone] = r;
                            }
                        }
                        else
                        {
                            if (!ApiHelper.remoteDlInfos_finished.ContainsKey(phone))
                            {
                                ApiHelper.remoteDlInfos_finished.Add(phone, r);
                            }
                            else
                            {
                                ApiHelper.remoteDlInfos_finished[phone] = r;
                            }
                        }
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"GetRemoteDlInfo-获取数据出错{r.rtn}:{r.rtn}");
                    }
                    return false;
                default:
                    Debug.WriteLine("GetRemoteDlInfo-网络异常错误！");
                    //MessageBox.Show(resp.data.ToString(), "网络异常错误！");
                    return false;
            }
        }
        async Task<bool> RemoteDlLogin(string phone)
        {
            HttpMessage resp = await ApiHelper.RemoteDlLogin(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        return false;
                    }
                    var r = resp.data as RemoteDlLoginResponse;
                    if (r.rtn == 0)
                    {
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"RemoteDlLogin-获取数据出错{r.rtn}:{r.rtn}");
                    }
                    return false;
                default:
                    Debug.WriteLine("RemoteDlLogin-网络异常错误！");
                    return false;
            }
        }
        async Task<string> SetDeviceName(string phone, string device_name)
        {
            HttpMessage resp = await ApiHelper.SetDeviceName(phone, device_name);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        return "错误！获取数据为空！";
                    }
                    var r = resp.data as SetDeviceNameResponse;
                    if (r.rtn == 0)
                    {
                        return "0";
                    }
                    else
                    {
                        Debug.WriteLine($"SetDeviceName-获取数据出错{r.msg}:{r.rtn}");
                    }
                    return r.msg;
                default:
                    Debug.WriteLine("SetDeviceName-网络异常错误！");
                    return "错误！网络异常错误！";
            }
        }
        async Task<bool> GetUsbInfo(string phone)
        {
            HttpMessage resp = await ApiHelper.GetUSBInfo(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        return false;
                    }
                    var r = resp.data as UsbInfoResponse;
                    if (r.rtn == 0)
                    {
                        if (r.result.Count > 1)
                        {
                            if (r.result[1] != null)
                            {
                                UsbInfoPartitions parts = JsonHelper.Deserialize<UsbInfoPartitions>(r.result[1].ToString());
                                if (parts != null)
                                {
                                    if (!ApiHelper.usbInfoPartitions.ContainsKey(phone))
                                    {
                                        ApiHelper.usbInfoPartitions.Add(phone, parts.partitions);
                                    }
                                    else
                                    {
                                        ApiHelper.usbInfoPartitions[phone] = parts.partitions;
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"GetRemoteDlInfo-获取数据出错{r.rtn}:{r.rtn}");
                    }
                    return false;
                default:
                    Debug.WriteLine("GetRemoteDlInfo-网络异常错误！");
                    //MessageBox.Show(resp.data.ToString(), "网络异常错误！");
                    return false;
            }
        }
        async Task<DrawWkbResponse> DrawWkb(string phone)
        {
            HttpMessage resp = await ApiHelper.DrawWkb(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data != null)
                    {
                        return resp.data as DrawWkbResponse;
                    }
                    else
                    {
                        Debug.WriteLine($"DrawWkb-获取数据出错{(resp.data as DrawWkbResponse).iRet}");
                    }
                    return null;
                default:
                    Debug.WriteLine("DrawWkb-网络异常错误！");
                    return null;
            }
        }
        private void RemoteDlTimer_Tick(object sender, EventArgs e)
        {
            if (chk_refreshRemoteDl.IsChecked == true)
            {
                RefreshRemoteDlStatus();
            }
        }
        private async void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (settings.autoRefresh)
            {
                System.GC.Collect();
                await RefreshStatus();
            }
        }
        public ObservableCollection<DeviceInfoVM> deviceInfos
        {
            get
            {
                if (this._deviceInfos == null)
                {
                    double yesAllCoin = 0;
                    double hisAllCoin = 0;
                    double ketiWkb = 0;
                    int onlineCount = 0;
                    int offlineCount = 0;
                    _deviceInfos = new ObservableCollection<DeviceInfoVM>();
                    var DiList = new List<DeviceInfoVM>();
                    foreach (var t in ApiHelper.userBasicDatas)
                    {
                        try
                        {
                            var ubd = t.Value;
                            var device = ApiHelper.userDevices[ubd.phone];
                            var userInfo = ApiHelper.userInfos[ubd.phone];
                            List<UsbInfoPartition> partitions = new List<UsbInfoPartition>();
                            ulong cap = 0;
                            ulong used = 0;
                            if (ApiHelper.usbInfoPartitions.ContainsKey(ubd.phone))
                            {
                                partitions = ApiHelper.usbInfoPartitions[ubd.phone];
                                partitions.ForEach(p =>
                                {
                                    cap += p.capacity;
                                    used += p.used;
                                });
                            }
                            var wkbAccountInfo = ApiHelper.wkbAccountInfos[ubd.phone];
                            string volume = $"{UtilHelper.ConvertToSizeString(used)}/{UtilHelper.ConvertToSizeString(cap)}";
                            var incomeHistory = ApiHelper.incomeHistorys[ubd.phone];
                            yesAllCoin += userInfo.yes_wkb;
                            hisAllCoin += incomeHistory.totalIncome;
                            ketiWkb += wkbAccountInfo.balance;
                            if (device.status == "offline")
                            {
                                offlineCount++;
                            }
                            else
                            {
                                onlineCount++;
                            }
                            var di = new DeviceInfoVM
                            {
                                phone = ubd.phone,
                                bind_pwd = ubd.bind_pwd,
                                nickname = ubd.nickname,
                                ip = device.ip,
                                device_name = device.device_name,
                                status = device.status == "offline" ? "离线" : (device.status == "online" ? "在线" : (device.status == "exception" ? "在线" : device.status)),
                                status_color = device.status == "offline" ? "Red" : "Green",
                                dcdn_upnp_status = device.dcdn_upnp_status,
                                system_version = device.system_version,
                                dcdn_download_speed = device.dcdn_download_speed.ToString(),
                                dcdn_upload_speed = device.dcdn_upload_speed.ToString(),
                                exception_message = device.exception_message,
                                isActived = (device.features.onecloud_coin).ToString() == "False" ? "未激活" : "已激活" + userInfo.activate_days.ToString() + "天",
                                dcdn_clients_count = (device.dcdn_clients.Count).ToString(),
                                dcdn_upnp_message = device.dcdn_upnp_message,
                                upgradeable = device.upgradeable ? "可升级" : "已最新",
                                ip_info = $"{device.ip_info.province}{device.ip_info.city}{device.ip_info.isp}",
                                yes_wkb = userInfo.yes_wkb.ToString(),
                                activate_days = userInfo.activate_days.ToString(),
                                totalIncome = incomeHistory.totalIncome.ToString(),
                                volume = device.status != "offline" ? volume : "设备离线",
                                device_sn = device.device_sn,
                                ketiWkb = wkbAccountInfo.balance.ToString(),
                            };
                            DiList.Add(di);
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(ex.Message);
                        }
                    }
                    DiList = DiList.OrderBy(t => t.device_name).ToList();
                    DiList.ForEach(t => _deviceInfos.Add(t));
                    tbk_yesAllCoin.Text = yesAllCoin.ToString();
                    tbk_hisAllCoin.Text = hisAllCoin.ToString();
                    tbk_onlineCount.Text = onlineCount.ToString();
                    tbk_offlineCount.Text = offlineCount.ToString();
                    tbk_ketiWkb.Text = ketiWkb.ToString();
                }
                return _deviceInfos;
            }
            set
            {
                this._deviceInfos = value;
            }
        }
        public ObservableCollection<DlTaskVM> dlTasks
        {
            get
            {
                try
                {
                    if (this._dlTasks == null)
                    {
                        _dlTasks = new ObservableCollection<DlTaskVM>();
                        if (!ApiHelper.remoteDlInfos.ContainsKey(curAccount))
                        {
                            return _dlTasks;
                        }
                        if (ApiHelper.remoteDlInfos[curAccount] == null)
                        {
                            return _dlTasks;
                        }
                        if (ApiHelper.remoteDlInfos[curAccount].tasks == null)
                        {
                            return _dlTasks;
                        }
                        tbx_taskCount.Text = ApiHelper.remoteDlInfos[curAccount].dlNum.ToString();
                        tbx_taskFinishedCount.Text = ApiHelper.remoteDlInfos[curAccount].completeNum.ToString();
                        foreach (var t in ApiHelper.remoteDlInfos[curAccount].tasks)
                        {
                            var st = t.state;
                            if (st != 0 && st != 8 && st != 12)
                            {
                                st = 12;
                            }
                            var stateimg = new BitmapImage(new Uri($"pack://application:,,,/img/state_{st}.png"));
                            var task = new DlTaskVM
                            {
                                name = t.name,
                                state = t.state == 0 ? "添加中" : (t.state == 8 ? "正在等待" : "链接无效(130)"),
                                state_color = t.state == 0 ? "DodgerBlue" : (t.state == 8 ? "LightBlue" : "Orange"),
                                state_img = stateimg,
                                speed = UtilHelper.ConvertToSpeedString(t.speed),
                                progress = (t.progress / 100d).ToString("f2") + "%",
                            };
                            _dlTasks.Add(task);
                        }
                    }
                    return _dlTasks;
                }
                catch (Exception ex)
                {
                    Debug.Write("dlTasks-get-error " + ex.Message);
                    return _dlTasks;
                }
            }
            set
            {
                _dlTasks = value;
            }
        }
        public ObservableCollection<DlTaskVM> dlTasks_finished
        {
            get
            {
                try
                {
                    if (this._dlTasks_finished == null)
                    {
                        _dlTasks_finished = new ObservableCollection<DlTaskVM>();
                        if (!ApiHelper.remoteDlInfos_finished.ContainsKey(curAccount))
                        {
                            return _dlTasks_finished;
                        }
                        if (ApiHelper.remoteDlInfos_finished[curAccount] == null)
                        {
                            return _dlTasks_finished;
                        }
                        if (ApiHelper.remoteDlInfos_finished[curAccount].tasks == null)
                        {
                            return _dlTasks_finished;
                        }
                        tbx_taskCount.Text = ApiHelper.remoteDlInfos_finished[curAccount].dlNum.ToString();
                        tbx_taskFinishedCount.Text = ApiHelper.remoteDlInfos_finished[curAccount].completeNum.ToString();
                        foreach (var t in ApiHelper.remoteDlInfos_finished[curAccount].tasks)
                        {
                            var task = new DlTaskVM
                            {
                                name = t.name,
                                state = "已完成",
                                state_color = "Green",
                                state_img = null,
                                speed = UtilHelper.ConvertToSpeedString(t.speed),
                                progress = (t.progress / 100d).ToString("f2") + "%",
                            };
                            _dlTasks_finished.Add(task);
                        }
                    }
                    return _dlTasks_finished;
                }
                catch (Exception ex)
                {
                    Debug.Write("dlTasks-get-error " + ex.Message);
                    return _dlTasks_finished;
                }
            }
            set
            {
                _dlTasks_finished = value;
            }
        }
        internal ObservableCollection<FileVM> partitions
        {
            get
            {
                try
                {
                    if (this._partitions == null)
                    {
                        _partitions = new ObservableCollection<FileVM>();
                        if (!ApiHelper.usbInfoPartitions.ContainsKey(curAccount))
                        {
                            return _partitions;
                        }
                        if (ApiHelper.usbInfoPartitions[curAccount] == null)
                        {
                            return _partitions;
                        }
                        foreach (var t in ApiHelper.usbInfoPartitions[curAccount])
                        {
                            var p = new FileVM
                            {
                                capacity = UtilHelper.ConvertToSizeString(t.capacity),
                                used = UtilHelper.ConvertToSizeString(t.used),
                                label = t.label,
                                path = t.path,
                                id = t.id.ToString(),
                                unique = t.unique.ToString(),
                                disk_id = t.disk_id.ToString()
                            };
                            _partitions.Add(p);
                        }
                    }
                    return _partitions;
                }
                catch (Exception ex)
                {
                    Debug.Write("partitions-get-error " + ex.Message);
                    return _partitions;
                }
            }
            set
            {
                _partitions = value;
            }
        }
        void LoadSettings()
        {
            if (SettingHelper.ReadOldSettings() != null)//检测到旧配置文件
            {
                var r = MessageBox.Show("软件已更新！检测到旧版本的用户列表，是否导入？", "提示", MessageBoxButton.YesNo);
                if (r == MessageBoxResult.Yes)
                {
                    settings = SettingHelper.ReadOldSettings();
                    SetPasswordWindow spw = new SetPasswordWindow();
                    spw.ShowDialog();
                    SettingHelper.DeleteOldSettings();
                    InitLogin();
                    return;
                }
                else
                {
                    SettingHelper.DeleteOldSettings();
                    MessageBox.Show("已删除旧版本的用户列表", "提示");
                }
            }
            if (SettingHelper.ExistSettings())//如果存在新版配置文件
            {
                AuthWindow aw = new AuthWindow();//开始验证
                aw.ShowDialog();
                if (settings != null)//如果验证通过，读取新配置文件成功
                {
                    InitLogin();
                }
                else//如果读取配置文件失败
                {
                    SettingHelper.DeleteSettings();
                    MessageBox.Show("读取配置文件失败，已删除配置文件", "提示");
                    Environment.Exit(0);
                }
            }
            else
            {
                SetPasswordWindow spw = new SetPasswordWindow();
                spw.ShowDialog();
                InitLogin();
            }
        }
        async void InitLogin()
        {
            grid_main.IsEnabled = false;
            if (settings.loginDatas != null && settings.loginDatas.Count > 0)
            {
                ld = new LoadingWindow();
                ld.Show();
                ld.SetTitle("登陆中");
                ld.SetTip("正在登陆");
                ld.SetPgr(0, settings.loginDatas.Count);
                for (int i = 0; i < settings.loginDatas.Count; i++)
                {
                    var t = settings.loginDatas[i];
                    ld.SetPgr(i, settings.loginDatas.Count);
                    ld.SetTip("正在登陆账号：" + t.phone);
                    await UserLogin(t);
                }
                chk_autoRefresh.IsChecked = settings.autoRefresh;
                LoadAccounts();
                await RefreshStatus();
                ld.Close();
                ld = null;
                StatusTimer.Start();
            }
            grid_main.IsEnabled = true;
        }
        async Task UserLogin(LoginData ld)
        {
            HttpMessage resp = await ApiHelper.Login(
                ld.phone, ld.pwd, "", ld.account_type, ld.deviceid, ld.imeiid);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    var loginResponse = resp.data as LoginResponse;
                    if (loginResponse.iRet == 0)
                    {
                        ApiHelper.userBasicDatas.Add(loginResponse.data.phone, loginResponse.data);
                        //RefreshStatus();
                        //载入登陆响应信息到主窗口
                    }
                    else if (loginResponse.iRet == -121)
                    {
                        if (settings.loginDatas != null && settings.loginDatas.Contains(ld))
                        {
                            settings.loginDatas.Remove(ld);
                            SettingHelper.WriteSettings(settings, password);
                        }
                        MessageBox.Show($"账号{ld.phone}登陆失败：验证码输入错误！请重新添加账号", "错误(-121)");
                    }
                    else if (loginResponse.iRet == -122)
                    {
                        if (settings.loginDatas != null && settings.loginDatas.Contains(ld))
                        {
                            settings.loginDatas.Remove(ld);
                            SettingHelper.WriteSettings(settings, password);
                        }
                        MessageBox.Show($"账号{ld.phone}登陆失败：需要输入验证码！请重新添加账号", "提示(-122)");
                    }
                    else
                    {
                        if (settings.loginDatas != null && settings.loginDatas.Contains(ld))
                        {
                            settings.loginDatas.Remove(ld);
                            SettingHelper.WriteSettings(settings, password);
                        }
                        MessageBox.Show($"账号{ld.phone}登陆失败：{loginResponse.sMsg}！请重新添加账号", $"登陆失败({loginResponse.iRet})");
                    }
                    break;
                default:
                    MessageBox.Show($"账号{ld.phone}登陆失败：{resp.data.ToString()}", "网络异常错误！");
                    break;
            }
        }
        private void chk_autoRefresh_Click(object sender, RoutedEventArgs e)
        {
            settings.autoRefresh = (bool)chk_autoRefresh.IsChecked;
            if (settings.autoRefresh)
            {
                StatusTimer.Start();
            }
            else
            {
                StatusTimer.Stop();
            }
            SettingHelper.WriteSettings(settings, password);
        }
        private async void Btu_refreshStatus_Click(object sender, RoutedEventArgs e)
        {
            StatusTimer.Stop();
            ld = new LoadingWindow();
            ld.Show();
            await RefreshStatus();
            ld.Close();
            ld = null;
            if (chk_autoRefresh.IsChecked == true)
            {
                StatusTimer.Start();
            }
        }
        private void link_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }
        private void btu_delete_Click(object sender, RoutedEventArgs e)
        {
            var btu = sender as Button;
            var phone = btu.CommandParameter as string;
            var result = MessageBox.Show($"确定删除账号{phone}?", "提示", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                ApiHelper.clients.Remove(phone);
                ApiHelper.userBasicDatas.Remove(phone);
                ApiHelper.userDevices.Remove(phone);
                ApiHelper.userInfos.Remove(phone);
                ApiHelper.incomeHistorys.Remove(phone);
                ApiHelper.usbInfoPartitions.Remove(phone);
                ApiHelper.wkbAccountInfos.Remove(phone);
                settings.loginDatas = settings.loginDatas.Where(t => t.phone != phone).ToList();
                if (curAccount == phone)
                {
                    cbx_curAccount.SelectedIndex = 0;
                }
                LoadAccounts();
                RefreshStatus();
                SettingHelper.WriteSettings(settings, password);
                MessageBox.Show($"删除账号{phone}成功", "提示");
            }
        }
        private void tbc_fileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbc_fileList.SelectedIndex == 0)
            {
                RefreshFileStatus();
            }
            else if (tbc_fileList.SelectedIndex == 3)
            {
                remoteDlTab_SelectionChanged(null, null);
                RemoteDlTimer.Start();
            }
            else
            {
                RemoteDlTimer.Stop();
            }
        }
        private void btu_addRemoteDlTask_Click(object sender, RoutedEventArgs e)
        {
            if (!IsLogined())
            {
                MessageBox.Show("请先登陆", "提示");
                return;
            }
            CreateTaskWindow crw = new CreateTaskWindow();
            crw.ShowDialog();
            RefreshRemoteDlStatus();
        }
        private void btu_refreshRemoteDlInfo_Click(object sender, RoutedEventArgs e)
        {
            remoteDlTab_SelectionChanged(null, null);
        }
        private void cbx_curAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbx_curAccount.SelectedValue != null)
            {
                curAccount = cbx_curAccount.SelectedValue.ToString();
                tbc_fileList_SelectionChanged(null, null);
            }
        }
        private void btu_max_Click(object sender, RoutedEventArgs e)
        {
            btu_maxnormal.Visibility = Visibility.Visible;
            btu_max.Visibility = Visibility.Collapsed;
            rcnormal = new Rect(this.Left, this.Top, this.Width, this.Height);//保存下当前位置与大小
            this.Left = 0;//设置位置
            this.Top = 0;
            Rect rc = SystemParameters.WorkArea;//获取工作区大小
            this.Width = rc.Width;
            this.Height = rc.Height;
        }
        private void btu_maxnormal_Click(object sender, RoutedEventArgs e)
        {
            btu_max.Visibility = Visibility.Visible;
            btu_maxnormal.Visibility = Visibility.Collapsed;
            this.Left = rcnormal.Left;
            this.Top = rcnormal.Top;
            this.Width = rcnormal.Width;
            this.Height = rcnormal.Height;
        }
        private void link_showQQ_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow a = new AboutWindow();
            a.ShowDialog();
        }
        private void btu_changePwd_Click(object sender, RoutedEventArgs e)
        {
            if (password == pwd_original.Password)
            {
                if (pwd_new.Password == pwd_new_confirm.Password)
                {
                    SettingHelper.WriteSettings(settings, pwd_new_confirm.Password);
                    password = pwd_new_confirm.Password;
                    MessageBox.Show("修改密码成功！", "提示");
                    pwd_original.Password = null;
                    pwd_new.Password = null;
                    pwd_new_confirm.Password = null;
                }
                else
                {
                    MessageBox.Show("修改密码失败！两次输入的新密码不一致", "提示");
                }
            }
            else
            {
                MessageBox.Show("修改密码失败！原密码错误", "提示");
            }
        }
        private void btu_refreshFile_Click(object sender, RoutedEventArgs e)
        {
            RefreshFileStatus();
        }
        private void file_item_DoubleClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("文件访问功能请等待后续开发和更新", "提示");
        }
        private void lv_file_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }
        private async void btu_tibi_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("请等待接下来的开发和更新", "提示");
            LoadingWindow wkld = new LoadingWindow();
            wkld.SetTitle("正在提取玩客币");
            wkld.Show();
            string result = "";
            for (int i = 0; i < settings.loginDatas.Count; i++)
            {
                var t = settings.loginDatas[i];
                wkld.SetTip($"正在提取账号{t.phone}的玩客币");
                wkld.SetPgr(i, settings.loginDatas.Count);
                string tresult = "";
                var r = await DrawWkb(t.phone);
                if (r != null)
                {
                    if (r.iRet == 0)
                    {
                        r.sMsg = "提币成功！";
                    }
                    tresult = $"{t.phone}:(状态码{r.iRet}){r.sMsg}";
                }
                else
                {
                    tresult = $"{t.phone}:网络通讯失败";
                }
                if (i != settings.loginDatas.Count - 1)
                {
                    result += tresult + Environment.NewLine;
                }
                else
                {
                    result += tresult;
                }
            }
            wkld.Close();
            MessageBox.Show(result, "提币结果");
        }
        private async void btu_drawWkb_Click(object sender, RoutedEventArgs e)
        {
            var btu = sender as Button;
            var phone = btu.CommandParameter as string;
            var result = MessageBox.Show($"确定提取账号{phone}的玩客币?", "提示", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                var r = await DrawWkb(phone);
                if (r != null)
                {
                    RefreshStatus();
                    MessageBox.Show($"{r.sMsg}", $"提示({r.iRet})");
                }
                else
                {
                    MessageBox.Show($"网络请求失败", "提示");
                }
            }
        }
        private void remoteDlTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (remoteDlTab.SelectedIndex == 0)
            {
                RefreshRemoteDlStatus();
            }
            else
            {
                RefreshRemoteDlStatus_finished();
            }
            if (e != null)
            {
                e.Handled = true;
            }
        }
        private async void btu_rename_Click(object sender, RoutedEventArgs e)
        {
            var btu = sender as Button;
            var phone = btu.CommandParameter as string;
            string newname = Interaction.InputBox("请输入新的设备名称", "设置设备名称", "", -1, -1);
            if (newname.Trim() != "")
            {
                string result = await SetDeviceName(phone, newname);
                if (result == "0")
                {
                    MessageBox.Show("设备名称修改成功！", "恭喜");
                    await RefreshStatus();
                }
                else
                {
                    MessageBox.Show("设备名称修改失败！您输入的名称格式有误或过长", "错误");
                }
            }
            else
            {
                MessageBox.Show("设备名称不能为空！", "提示");
            }
        }

        private void btu_viewHistoryIncome_Click(object sender, RoutedEventArgs e)
        {
            var btu = sender as Button;
            btu.IsEnabled = false;
            var phone = btu.CommandParameter as string;
            if (ApiHelper.incomeHistorys.ContainsKey(phone))
            {
                var ih = ApiHelper.incomeHistorys[phone];
                ViewHistoryWindow vhw = new ViewHistoryWindow(ih);
                vhw.ShowDialog();
            }
            else
            {
                MessageBox.Show("数据还没有获取成功！请刷新后重试", "提示");
            }
            btu.IsEnabled = true;
        }

        private void tab_account_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tab_account.SelectedIndex == 1)
            {
                LoadDayHistroy();
            }
        }
        private void LoadDayHistroy()
        {
            chart_dayHistory.DataSource = null;
            chart_dayHistory.DataSource = dayIncomes;
        }
    }
}