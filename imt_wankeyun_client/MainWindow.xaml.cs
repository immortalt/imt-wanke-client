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

namespace imt_wankeyun_client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static string password;
        private ObservableCollection<DeviceInfoVM> _deviceInfos = null;
        private ObservableCollection<DlTaskVM> _dlTasks = null;
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
            StatusTimer.Start();
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
                this.DragMove();
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
        async void RefreshStatus()
        {
            for (int i = 0; i < ApiHelper.userBasicDatas.Count; i++)
            {
                var t = ApiHelper.userBasicDatas.ElementAt(i);
                var phone = t.Key;
                var basic = t.Value;
                if (await ListPeer(phone))
                {
                    if (await GetUserInfo(phone))
                    {
                        await GetIncomeHistory(phone);
                    }
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
        async void RefreshRemoteDlStatus()
        {
            if (curAccount != null)
            {
                Debug.WriteLine("curAccount=" + curAccount);
                if (await GetRemoteDlInfo(curAccount))
                {
                    dlTasks = null;
                    lv_remoteDlStatus.ItemsSource = dlTasks;
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
            LoadAccounts();
            RefreshStatus();
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
        async Task<bool> GetRemoteDlInfo(string phone)
        {
            HttpMessage resp = await ApiHelper.GetRemoteDlInfo(phone);
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
                        if (!ApiHelper.remoteDlInfos.ContainsKey(phone))
                        {
                            ApiHelper.remoteDlInfos.Add(phone, r);
                        }
                        else
                        {
                            ApiHelper.remoteDlInfos[phone] = r;
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
        private void RemoteDlTimer_Tick(object sender, EventArgs e)
        {
            if (chk_refreshRemoteDl.IsChecked == true)
            {
                RefreshRemoteDlStatus();
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
                    double yesAllCoin = 0;
                    double hisAllCoin = 0;
                    _deviceInfos = new ObservableCollection<DeviceInfoVM>();
                    foreach (var t in ApiHelper.userBasicDatas)
                    {
                        try
                        {
                            var ubd = t.Value;
                            var device = ApiHelper.userDevices[ubd.phone];
                            var userInfo = ApiHelper.userInfos[ubd.phone];
                            var incomeHistory = ApiHelper.incomeHistorys[ubd.phone];
                            yesAllCoin += userInfo.yes_wkb;
                            hisAllCoin += incomeHistory.totalIncome;
                            var di = new DeviceInfoVM
                            {
                                phone = ubd.phone,
                                bind_pwd = ubd.bind_pwd,
                                nickname = ubd.nickname,
                                ip = device.ip,
                                device_name = device.device_name,
                                status = device.status,
                                status_color = device.status == "online" ? "Green" : "Red",
                                dcdn_upnp_status = device.dcdn_upnp_status,
                                system_version = device.system_version,
                                dcdn_download_speed = device.dcdn_download_speed.ToString(),
                                dcdn_upload_speed = device.dcdn_upload_speed.ToString(),
                                exception_message = device.exception_message,
                                isActived = (device.features.onecloud_coin).ToString() == "False" ? "未激活" : "已激活",
                                dcdn_clients_count = (device.dcdn_clients.Count).ToString(),
                                dcdn_upnp_message = device.dcdn_upnp_message,
                                upgradeable = device.upgradeable ? "可升级" : "已最新",
                                ip_info = $"{device.ip_info.province}{device.ip_info.city}{device.ip_info.isp}",
                                yes_wkb = userInfo.yes_wkb.ToString(),
                                activate_days = userInfo.activate_days.ToString(),
                                totalIncome = incomeHistory.totalIncome.ToString(),
                            };
                            _deviceInfos.Add(di);
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(ex.Message);
                        }
                    }
                    tbk_yesAllCoin.Text = yesAllCoin.ToString();
                    tbk_hisAllCoin.Text = hisAllCoin.ToString();
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
            if (settings.loginDatas != null && settings.loginDatas.Count > 0)
            {
                for (int i = 0; i < settings.loginDatas.Count; i++)
                {
                    var t = settings.loginDatas[i];
                    await UserLogin(t);
                }
            }
            chk_autoRefresh.IsChecked = settings.autoRefresh;
            LoadAccounts();
            RefreshStatus();
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
        private void Btu_refreshStatus_Click(object sender, RoutedEventArgs e)
        {
            RefreshStatus();
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
            if (tbc_fileList.SelectedIndex == 3)
            {
                RefreshRemoteDlStatus();
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
            RefreshRemoteDlStatus();
        }

        private void cbx_curAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show(cbx_curAccount.SelectedValue.ToString());
            if (cbx_curAccount.SelectedValue != null)
            {
                curAccount = cbx_curAccount.SelectedValue.ToString();
                RefreshRemoteDlStatus();
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
    }
}