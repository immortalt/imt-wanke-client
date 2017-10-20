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

namespace imt_wankeyun_client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<DeviceInfoVM> _deviceInfos = null;
        DispatcherTimer StatusTimer;
        internal static WankeSettings settings;
        public MainWindow()
        {
            InitializeComponent();
            if (SettingHelper.ReadSettings() != null)
            {
                settings = SettingHelper.ReadSettings();
                if (settings.loginDatas != null && settings.loginDatas.Count > 0)
                {
                    foreach (var t in settings.loginDatas)
                    {
                        UserLogin(t);
                    }
                }
                chk_autoRefresh.IsChecked = settings.autoRefresh;
                RefreshStatus();
            }
            else
            {
                settings = new WankeSettings();
                settings.loginDatas = new List<LoginData>();
                settings.autoRefresh = true;
                SettingHelper.WriteSettings(settings);
            }
            StatusTimer = new DispatcherTimer();
            StatusTimer.Interval = TimeSpan.FromSeconds(6);
            StatusTimer.Tick += StatusTimer_Tick;
            StatusTimer.Start();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch
            {
                // do nothing
            }
        }
        private void x_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
        async void RefreshStatus()
        {
            for (int i = 0; i < ApiHelper.userBasicDatas.Count; i++)
            {
                var t = ApiHelper.userBasicDatas.ElementAt(i);
                var phone = t.Key;
                var basic = t.Value;
                var lp = await ListPeer(phone);
                if (lp)
                {
                    var gui = await GetUserInfo(phone);
                }
            }
            deviceInfos = null;
            lv_DeviceStatus.ItemsSource = deviceInfos;
            //调整列宽  
            GridView gv = lv_DeviceStatus.View as GridView;
            if (gv != null)
            {
                foreach (GridViewColumn gvc in gv.Columns)
                {
                    gvc.Width = gvc.ActualWidth;
                    gvc.Width = Double.NaN;
                }
            }
        }
        private void Btu_AddAccount_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow lw = new LoginWindow();
            lw.ShowDialog();
            RefreshStatus();
        }
        async Task<bool> ListPeer(string phone)
        {
            HttpMessage resp = await ApiHelper.ListPeer(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    var pr = resp.data as PeerRoot;
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
        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (settings.autoRefresh)
            {
                System.GC.Collect();
                RefreshStatus();
            }
        }
        public ObservableCollection<DeviceInfoVM> deviceInfos
        {
            get
            {
                if (this._deviceInfos == null)
                {
                    _deviceInfos = new ObservableCollection<DeviceInfoVM>();
                    foreach (var t in ApiHelper.userBasicDatas)
                    {
                        try
                        {
                            var ubd = t.Value;
                            var device = ApiHelper.userDevices[ubd.phone];
                            var userInfo = ApiHelper.userInfos[ubd.phone];
                            var di = new DeviceInfoVM
                            {
                                phone = ubd.phone,
                                bind_pwd = ubd.bind_pwd,
                                nickname = ubd.nickname,
                                ip = device.ip,
                                device_name = device.device_name,
                                status = device.status,
                                dcdn_upnp_status = device.dcdn_upnp_status,
                                system_version = device.system_version,
                                dcdn_download_speed = device.dcdn_download_speed.ToString(),
                                dcdn_upload_speed = device.dcdn_upload_speed.ToString(),
                                exception_message = device.exception_message,
                                onecloud_coin = (device.features.onecloud_coin / 10E8).ToString(),
                                dcdn_clients_count = (device.dcdn_clients.Count).ToString(),
                                dcdn_upnp_message = device.dcdn_upnp_message,
                                imported = device.imported,
                                upgradeable = device.upgradeable ? "可升级" : "已最新",
                                ip_info = $"{device.ip_info.province}{device.ip_info.city}{device.ip_info.isp}",
                                yes_wkb = userInfo.yes_wkb.ToString(),
                                activate_days = userInfo.activate_days.ToString(),
                            };
                            _deviceInfos.Add(di);
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(ex.Message);
                        }
                    }
                }
                return _deviceInfos;
            }
            set
            {
                this._deviceInfos = value;
            }
        }
        async void UserLogin(LoginData ld)
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
                        RefreshStatus();
                        //载入登陆响应信息到主窗口
                    }
                    else if (loginResponse.iRet == -121)
                    {
                        if (settings.loginDatas != null && settings.loginDatas.Contains(ld))
                        {
                            settings.loginDatas.Remove(ld);
                        }
                        MessageBox.Show($"账号{ld.phone}登陆失败：验证码输入错误！请重新添加账号", "错误(-121)");
                    }
                    else if (loginResponse.iRet == -122)
                    {
                        if (settings.loginDatas != null && settings.loginDatas.Contains(ld))
                        {
                            settings.loginDatas.Remove(ld);
                        }
                        MessageBox.Show($"账号{ld.phone}登陆失败：需要输入验证码！请重新添加账号", "提示(-122)");
                    }
                    else
                    {
                        if (settings.loginDatas != null && settings.loginDatas.Contains(ld))
                        {
                            settings.loginDatas.Remove(ld);
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
            SettingHelper.WriteSettings(settings);
        }
        private void Btu_refreshStatus_Click(object sender, RoutedEventArgs e)
        {
            RefreshStatus();
        }
        private void link_sourcecode_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }
    }
}