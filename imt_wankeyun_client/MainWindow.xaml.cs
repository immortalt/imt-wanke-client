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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using imt_wankeyun_client.Entities.ServerChan;

namespace imt_wankeyun_client
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        bool CanOpenNotify = false;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        bool IsHandRefreshing = false;
        LoadingWindow ld;
        internal static string password;
        private ObservableCollection<DeviceInfoVM> _deviceInfos = null;
        private ObservableCollection<DlTaskVM> _dlTasks = null;
        private ObservableCollection<DlTaskVM> _dlTasks_finished = null;
        List<Income> dayIncomes = new List<Income>();
        private ObservableCollection<FileVM> _partitions = null;
        DispatcherTimer NotifyTimer;
        DispatcherTimer StatusTimer;
        DispatcherTimer RemoteDlTimer;
        internal static WankeSettings settings;
        internal static string curAccount = null;
        ObservableCollection<string> userList;
        Rect rcnormal;//定义一个全局rect记录还原状态下窗口的位置和大小
        System.Drawing.Icon onlineIcon;
        System.Drawing.Icon offlineIcon;
        private Dictionary<string, bool> OnlineStatus = new Dictionary<string, bool>();
        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += delegate (object sender, EventArgs e)//执行拖拽
            {
                this._HwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            };
            this.MouseMove += new MouseEventHandler(Window_MouseMove);//鼠标移入到边缘收缩

            onlineIcon = Properties.Resources.online;
            offlineIcon = Properties.Resources.offline;
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.BalloonTipText = "系统运行中……";
            this.notifyIcon.ShowBalloonTip(2000);
            this.notifyIcon.Text = "系统运行中……";
            this.notifyIcon.Icon = onlineIcon;
            this.notifyIcon.Visible = true;
            //打开菜单项
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("打开主面板");
            open.Click += new EventHandler(Show);
            //彻底隐藏菜单项
            System.Windows.Forms.MenuItem hide = new System.Windows.Forms.MenuItem("彻底隐藏");
            hide.Click += new EventHandler(TotallyHide);
            //退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("退出程序");
            exit.Click += new EventHandler(Close);
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, hide, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left) this.Show(o, e);
            });

            try
            {
                tbk_version.Text = GetEdition();
            }
            catch (Exception ex)
            {
                tbk_version.Text = "开发版";
                Debug.WriteLine("MainWindow error" + ex.Message);
            }
            NotifyTimer = new DispatcherTimer();
            NotifyTimer.Interval = TimeSpan.FromMinutes(1);
            NotifyTimer.Tick += NotifyTimer_Tick;
            StatusTimer = new DispatcherTimer();
            StatusTimer.Interval = TimeSpan.FromSeconds(15);
            StatusTimer.Tick += StatusTimer_Tick;
            RemoteDlTimer = new DispatcherTimer();
            RemoteDlTimer.Interval = TimeSpan.FromSeconds(5);
            RemoteDlTimer.Tick += RemoteDlTimer_Tick;
            LoadSettings();//载入设置
        }
        private void TotallyHide(object sender, EventArgs e)
        {
            var r = MessageBox.Show("是否彻底隐藏？\n彻底隐藏后请通过任务管理器来结束本程序(imt_wankeyun_client.exe)", "确认", MessageBoxButton.YesNo);
            if (r == MessageBoxResult.Yes)
            {
                this.Hide();
                notifyIcon.Visible = false;
            }
        }
        private void Show(object sender, EventArgs e)
        {
            //Visibility = Visibility.Visible;
            Opacity = 1;
            ShowInTaskbar = true;
            Activate();
        }
        private void Close(object sender, EventArgs e)
        {
            Environment.Exit(0);
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
            //this.WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            Opacity = 0;
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
        async Task RefreshStatus(string singlePhone = null, bool Hand = false)
        {
            if (IsHandRefreshing && (!Hand))
            {
                return;
            }
            if (!Hand && singlePhone == null)
            {
                tbk_isAutoRefreshing.Text = "正在自动刷新";
            }
            try
            {
                if (ApiHelper.userBasicDatas.Count == 0)
                {
                    deviceInfos = null;
                    lv_DeviceStatus.ItemsSource = deviceInfos;
                    AutoHeaderWidth(lv_DeviceStatus);
                    return;
                }
                StatusTimer.Interval = TimeSpan.FromSeconds((ApiHelper.userBasicDatas.Count * 3) + 3);
                for (int i = 0; i < ApiHelper.userBasicDatas.Count; i++)
                {
                    if (!Hand && singlePhone == null)
                    {
                        tbk_isAutoRefreshing.Text = $"正在自动刷新({i + 1}/{ApiHelper.userBasicDatas.Count})";
                    }
                    var t = ApiHelper.userBasicDatas.ElementAt(i);
                    var phone = t.Key;
                    var basic = t.Value;
                    if (singlePhone != null && phone != singlePhone)
                    {
                        continue;
                    }
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
                        var cClone = new Income
                        {
                            date = c.date,
                            num = c.num
                        };
                        if (dayIncomes.Where(tt => tt.date == c.date).Count() == 0)
                        {
                            dayIncomes.Add(cClone);
                        }
                        else
                        {
                            var ii = dayIncomes.Find(tt => tt.date == cClone.date);
                            ii.num = (Convert.ToDouble(ii.num) + Convert.ToDouble(cClone.num)).ToString();
                        }
                    }
                }
                dayIncomes = dayIncomes.OrderBy(t => t.date).ToList();
                lv_incomeHistory.ItemsSource = null;
                lv_incomeHistory.ItemsSource = dayIncomes.OrderByDescending(t => t.date).ToList();
                AutoHeaderWidth(lv_incomeHistory);
                tbk_isAutoRefreshing.Text = "";
            }
            catch (Exception ex)
            {
                tbk_isAutoRefreshing.Text = "";
                Debug.WriteLine("RefreshStatus error:" + ex.Message);
            }
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
        async Task<string> DeviceReboot(string phone)
        {
            HttpMessage resp = await ApiHelper.DeviceReboot(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        return "错误！获取数据为空！";
                    }
                    var r = resp.data as SimpleResponse;
                    return r.msg;
                default:
                    Debug.WriteLine("DeviceReboot-网络异常错误！");
                    return "错误！网络异常错误！";
            }
        }
        async Task<string> UmountUSBDisk(string phone)
        {
            HttpMessage resp = await ApiHelper.UmountUSBDisk(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        return "错误！获取数据为空！";
                    }
                    var r = resp.data as SimpleResponse;
                    return r.msg;
                default:
                    Debug.WriteLine("UmountUSBDisk-网络异常错误！");
                    return "错误！网络异常错误！";
            }
        }
        async Task<string> UpgradeProcess(string phone)
        {
            HttpMessage resp = await ApiHelper.UpgradeProgress(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        MessageBox.Show("获取数据为空！", "错误！");
                    }
                    var r = resp.data as UpgradeResponse;
                    if (r.rtn == 0)
                    {
                        if (r.result != null && r.result.Count > 1)
                        {
                            UpgradeProgressResult upr = JsonHelper.Deserialize<UpgradeProgressResult>(r.result[1].ToString());
                            if (upr.message != "已经准备好升级")
                            {
                                MessageBox.Show(upr.message, $"账号{phone}固件升级状态");
                            }
                            if (upr.name == "ready")
                            {
                                await UpgradeCheck(phone);
                            }
                        }
                    }
                    else
                    {
                        if (r.result != null && r.result.Count > 1)
                        {
                            UpgradeProgressResult upr = JsonHelper.Deserialize<UpgradeProgressResult>(r.result[1].ToString());
                            MessageBox.Show(upr.message, $"账号{phone}固件升级状态");
                        }
                        Debug.WriteLine($"UpgradeProcess-获取数据出错{r.msg}:{r.rtn}");
                    }
                    return r.msg;
                default:
                    Debug.WriteLine("UpgradeProcess-网络异常错误！");
                    MessageBox.Show("固件升级-网络异常错误！", "错误");
                    return "错误！网络异常错误！";
            }
        }
        async Task<string> UpgradeCheck(string phone)
        {
            HttpMessage resp = await ApiHelper.UpgradeCheck(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        MessageBox.Show("获取数据为空！", "错误！");
                    }
                    var r = resp.data as UpgradeResponse;
                    if (r.rtn == 0)
                    {
                        if (r.result != null && r.result.Count > 1)
                        {
                            UpgradeCheckResult ucr = JsonHelper.Deserialize<UpgradeCheckResult>(r.result[1].ToString());
                            if (ucr.app == "")
                            {
                                MessageBox.Show("已经是最新版本", "提示");
                            }
                            else
                            {
                                string msg = "更新版本：" + ucr.app + Environment.NewLine;
                                msg += "更新内容：" + ucr.description;
                                var yes = MessageBox.Show(msg, "固件升级", MessageBoxButton.YesNo);
                                if (yes == MessageBoxResult.Yes)
                                {
                                    await UpgradeStart(phone);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"UpgradeProcess-获取数据出错{r.msg}:{r.rtn}");
                    }
                    return r.msg;
                default:
                    Debug.WriteLine("UpgradeProcess-网络异常错误！");
                    MessageBox.Show("固件升级-网络异常错误！", "错误");
                    return "错误！网络异常错误！";
            }
        }
        async Task<string> UpgradeStart(string phone)
        {
            HttpMessage resp = await ApiHelper.UpgradeStart(phone);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        MessageBox.Show("获取数据为空！", "错误！");
                    }
                    var r = resp.data as UpgradeResponse;
                    if (r.rtn == 0)
                    {
                        if (r.msg == "success")
                        {
                            MessageBox.Show("升级成功！请等待设备重启升级", "升级结果");
                        }
                        else
                        {
                            MessageBox.Show(JsonHelper.Serialize(r), "升级结果");
                        }
                    }
                    else
                    {
                        MessageBox.Show(JsonHelper.Serialize(r), "升级结果");
                        Debug.WriteLine($"UpgradeStart-获取数据出错{r.msg}:{r.rtn}");
                    }
                    return r.msg;
                default:
                    Debug.WriteLine("UpgradeStart-网络异常错误！");
                    MessageBox.Show("固件升级-网络异常错误！", "错误");
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
                await RefreshStatus();
            }
        }
        public ObservableCollection<DeviceInfoVM> deviceInfos
        {
            get
            {
                try
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
                        for (int i = 0; i < ApiHelper.userBasicDatas.Count; i++)
                        {
                            var ubd = ApiHelper.userBasicDatas.Values.ElementAt(i);
                            Device device = new Device
                            {
                                ip = "暂无数据",
                                lan_ip = "暂无数据",
                                status = "暂无数据",
                                device_name = "暂无数据",
                                dcdn_upnp_status = "暂无数据",
                                system_version = "暂无数据",
                                dcdn_upload_speed = 0,
                                dcdn_download_speed = 0,
                                exception_message = "暂无数据",
                                features = new Features
                                {
                                    onecloud_coin = 0
                                },
                                dcdn_clients = new List<DcdnClient>(),
                                dcdn_upnp_message = "暂无数据",
                                upgradeable = false,
                                ip_info = new IpInfo
                                {
                                    city = "暂无数据",
                                    country = "暂无数据",
                                    isp = "暂无数据",
                                    province = "暂无数据"
                                },
                                device_sn = "暂无数据",
                            };
                            if (ApiHelper.userDevices.ContainsKey(ubd.phone))
                            {
                                device = ApiHelper.userDevices[ubd.phone];
                            }
                            UserInfo userInfo = new UserInfo
                            {
                                yes_wkb = 0,
                                activate_days = 0,
                            }; ;
                            if (ApiHelper.userInfos.ContainsKey(ubd.phone))
                            {
                                userInfo = ApiHelper.userInfos[ubd.phone];
                            }
                            List<UsbInfoPartition> partitions = new List<UsbInfoPartition>();
                            ulong cap = 0;
                            ulong used = 0;
                            string volume = "";
                            string volume_color = "Blue";
                            if (ApiHelper.usbInfoPartitions.ContainsKey(ubd.phone))
                            {
                                partitions = ApiHelper.usbInfoPartitions[ubd.phone];
                                partitions.ForEach(p =>
                                {
                                    cap += p.capacity;
                                    used += p.used;
                                });
                                volume = $"{UtilHelper.ConvertToSizeString(used)}/{UtilHelper.ConvertToSizeString(cap)}";
                                if (used == 0 && cap == 0)
                                {
                                    volume = "无硬盘";
                                    volume_color = "Red";
                                }
                            }
                            else
                            {
                                volume = "暂无数据";
                                volume_color = "Green";
                            }
                            WkbAccountInfo wkbAccountInfo = new WkbAccountInfo
                            {
                                balance = 0,
                                addr = "暂无数据"
                            };
                            if (ApiHelper.wkbAccountInfos.ContainsKey(ubd.phone))
                            {
                                wkbAccountInfo = ApiHelper.wkbAccountInfos[ubd.phone];
                            }
                            IncomeHistory incomeHistory = new IncomeHistory
                            {
                                incomeArr = new List<Income>(),
                                totalIncome = 0
                            };
                            if (ApiHelper.incomeHistorys.ContainsKey(ubd.phone))
                            {
                                incomeHistory = ApiHelper.incomeHistorys[ubd.phone];
                            }
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
                                phone = ubd.phone != null ? ubd.phone : "暂无数据",
                                nickname = ubd.nickname != null ? ubd.nickname : "暂无数据",
                                ip = device.ip != null ? device.ip : "暂无数据",
                                lan_ip = device.lan_ip != null ? device.lan_ip : "暂无数据",
                                device_name = device.device_name != null ? device.device_name : "暂无数据",
                                status = device.status == "offline" ? "离线" : (device.status == "online" ? "在线" : (device.status == "exception" ? "在线" : device.status)),
                                status_color = device.status == "offline" ? "Red" : "Green",
                                dcdn_upnp_status = device.dcdn_upnp_status != null ? device.dcdn_upnp_status : "暂无数据",
                                system_version = device.system_version != null ? device.system_version : "暂无数据",
                                dcdn_download_speed = UtilHelper.ConvertToSpeedString(device.dcdn_download_speed),
                                dcdn_upload_speed = UtilHelper.ConvertToSpeedString(device.dcdn_upload_speed),
                                exception_message = device.exception_message != null ? device.exception_message : "暂无数据",
                                isActived = (device.features != null ? device.features.onecloud_coin : 0).ToString() == "False" ? "未激活" : "已激活" + userInfo.activate_days.ToString() + "天",
                                dcdn_clients_count = (device.dcdn_clients != null ? device.dcdn_clients.Count : 0).ToString(),
                                dcdn_upnp_message = device.dcdn_upnp_message != null ? device.dcdn_upnp_message : "暂无数据",
                                upgradeable = device.upgradeable ? "可升级" : "已最新！",
                                ip_info = device.ip_info != null ? $"{device.ip_info.province}{device.ip_info.city}{device.ip_info.isp}" : "暂无数据",
                                yes_wkb = userInfo.yes_wkb,
                                activate_days = userInfo.activate_days,
                                totalIncome = incomeHistory.totalIncome,
                                volume = device.status != "offline" ? volume : "设备离线",
                                device_sn = device.device_sn != null ? device.device_sn : "暂无数据",
                                ketiWkb = wkbAccountInfo.balance,
                                wkbAddr = wkbAccountInfo.addr != null ? wkbAccountInfo.addr : "暂无",
                                showUpgrade = device.upgradeable ? Visibility.Visible : Visibility.Collapsed,
                                volume_color = volume_color,
                            };
                            DiList.Add(di);
                            //确保获取到了设备的状态的情况下
                            if (ApiHelper.userDevices.ContainsKey(ubd.phone))
                            {
                                //检测并记录设备在线状态
                                if (!OnlineStatus.ContainsKey(di.phone))
                                {
                                    //初始化设备在线状态
                                    OnlineStatus.Add(di.phone, di.status == "在线");
                                }
                                else
                                {
                                    //如果设备在线状态发生了变更
                                    if (OnlineStatus[di.phone] != (di.status == "在线"))
                                    {
                                        Debug.WriteLine(di.phone + "在线状态发生了变更");
                                        OnlineStatus[di.phone] = (di.status == "在线");
                                        SendNotifyMail(di);
                                        SendNotifyServerChan(di);
                                    }
                                }
                            }
                        }
                        DiList = DiList.OrderBy(t => t.device_name).ToList();
                        DiList.ForEach(t => _deviceInfos.Add(t));
                        tbk_yesAllCoin.Text = yesAllCoin.ToString();
                        tbk_hisAllCoin.Text = hisAllCoin.ToString();
                        tbk_onlineCount.Text = onlineCount.ToString();
                        tbk_offlineCount.Text = offlineCount.ToString();
                        if (offlineCount > 0)
                        {
                            notifyIcon.Icon = offlineIcon;
                            notifyIcon.Text = "离线设备数量：" + offlineCount.ToString();
                        }
                        else
                        {
                            notifyIcon.Icon = onlineIcon;
                            notifyIcon.Text = "所有" + onlineCount.ToString() + "设备正常在线";
                        }
                        tbk_ketiWkb.Text = ketiWkb.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Source + ex.StackTrace + ex.InnerException + ex.Message);
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
                    if (settings.mailAccount.port == 0)
                    {
                        settings.mailAccount.port = 25;
                    }
                    if (settings.mailAccount.smtpServer == null)
                    {
                        settings.mailAccount.smtpServer = "smtp.qq.com";
                    }
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
                    //MessageBox.Show(JsonHelper.Serialize(settings));
                    if (settings.mailAccount.port == 0)
                    {
                        settings.mailAccount.port = 25;
                    }
                    if (settings.mailAccount.smtpServer == null)
                    {
                        settings.mailAccount.smtpServer = "smtp.qq.com";
                    }
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
            if (settings.mailAccount.mailTo == null)
            {
                settings.mailAccount.mailTo = settings.mailAccount.username;
                SettingHelper.WriteSettings(settings, password);
            }
            grid_mailNotify.DataContext = settings.mailAccount;
            grid_serverchan.DataContext = settings;
            tbx_mailPwd.Password = settings.mailAccount.password;
            if (settings.mailNotify)
            {
                btu_mailNotify.Content = "关闭提醒";
                NotifyTimer.Start();
            }
            else
            {
                btu_mailNotify.Content = "开启提醒";
            }
            if (settings.serverchanNotify)
            {
                btu_serverchanNotify.Content = "关闭提醒";
                NotifyTimer.Start();
            }
            else
            {
                btu_serverchanNotify.Content = "开启提醒";
            }
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
            if (settings.mailNotify)
            {
                SendDailyNotifyMail();
            }
            if (settings.serverchanNotify)
            {
                SendDailyNotifyServerChan();
            }
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
            IsHandRefreshing = true;
            StatusTimer.Stop();
            ld = new LoadingWindow();
            ld.Show();
            await RefreshStatus(null, true);
            ld.Close();
            ld = null;
            if (chk_autoRefresh.IsChecked == true)
            {
                StatusTimer.Start();
            }
            IsHandRefreshing = false;
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
                Thread.Sleep(3 * 1000);//防止提币过快引起风控
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
        private async void btu_upgrade_Click(object sender, RoutedEventArgs e)
        {
            var btu = sender as Button;
            var phone = btu.CommandParameter as string;
            await UpgradeProcess(phone);
        }
        private async void btu_reboot_Click(object sender, RoutedEventArgs e)
        {
            var btu = sender as Button;
            var phone = btu.CommandParameter as string;
            var result = MessageBox.Show($"确定重启账号{phone}的设备?", "提示", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                var resp = await DeviceReboot(phone);
                MessageBox.Show(resp == "success" ? "重启指令发送成功！请等待设备重启" : resp, "提示");
                await RefreshStatus(phone);
            }
        }
        private async void btu_umountUSBDisk_Click(object sender, RoutedEventArgs e)
        {
            var btu = sender as Button;
            var phone = btu.CommandParameter as string;
            var result = MessageBox.Show($"确定安全弹出账号{phone}设备的硬盘?", "提示", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                var resp = await UmountUSBDisk(phone);
                MessageBox.Show(resp == "success" ? "安全弹出硬盘成功！可以拔掉硬盘了" : resp, "提示");
                await RefreshStatus(phone);
            }
        }
        #region 初始化窗体可以缩放大小
        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource _HwndSource;
        private Dictionary<ResizeDirection, Cursor> cursors = new Dictionary<ResizeDirection, Cursor>
        {
            {ResizeDirection.BottomRight, Cursors.SizeNWSE},
        };
        private enum ResizeDirection
        {
            BottomRight = 8,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        #endregion
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if (element != null && !element.Name.Contains("Resize"))
                    this.Cursor = Cursors.Arrow;
            }
        }
        private void ResizePressed(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ResizeDirection direction = (ResizeDirection)Enum.Parse(typeof(ResizeDirection), element.Name.Replace("Resize", ""));
            this.Cursor = cursors[direction];
            if (e.LeftButton == MouseButtonState.Pressed)
                ResizeWindow(direction);
        }
        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_HwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        private void btu_mailNotify_Click(object sender, RoutedEventArgs e)
        {
            if (!settings.mailNotify)
            {
                if (!CanOpenNotify)
                {
                    MessageBox.Show("请先发送测试邮件然后保存设置，\n确认可以发送成功，才能开启提醒", "提示");
                    return;
                }
            }
            settings.mailNotify = !settings.mailNotify;
            SettingHelper.WriteSettings(settings, password);
            if (settings.mailNotify)
            {
                btu_mailNotify.Content = "关闭提醒";
                NotifyTimer.Start();
                SendDailyNotifyMail();
            }
            else
            {
                btu_mailNotify.Content = "开启提醒";
            }
        }
        private void btu_saveMail_Click(object sender, RoutedEventArgs e)
        {
            settings.mailAccount.password = tbx_mailPwd.Password;
            SettingHelper.WriteSettings(settings, password);
            MessageBox.Show("设置保存成功！", "提示");
        }
        private async void btu_sendTestMail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbx_mailUsername.Text.Trim() == "")
                {
                    MessageBox.Show("用户名不能为空！", "提示");
                    return;
                }
                if (tbx_mailPwd.Password.Trim() == "")
                {
                    MessageBox.Show("密码不能为空！", "提示");
                    return;
                }
                if (tbx_smtpServer.Text.Trim() == "")
                {
                    MessageBox.Show("SMTP服务器不能为空！", "提示");
                    return;
                }
                if (tbx_mailPort.Text.Trim() == "")
                {
                    MessageBox.Show("SMTP服务器端口不能为空！", "提示");
                    return;
                }
                MailHelper.username = tbx_mailUsername.Text.Trim();
                MailHelper.password = tbx_mailPwd.Password.Trim();
                MailHelper.smtpServer = tbx_smtpServer.Text.Trim();
                MailHelper.port = Convert.ToInt32(tbx_mailPort.Text.Trim());
                var ct = "这是一封测试邮件，测试提醒邮件能不能发送<br/>测试时间：" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                var result = await MailHelper.SendEmail(settings.mailAccount.mailTo, "测试邮件-不朽玩客云客户端", ct);
                MessageBox.Show(result, "提示");
                if (result == "发送成功")
                {
                    CanOpenNotify = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误！" + ex.Message, "提示");
            }
        }
        private async void SendNotifyMail(DeviceInfoVM di)
        {
            if (settings.mailNotify)
            {
                MailHelper.username = settings.mailAccount.username;
                MailHelper.password = settings.mailAccount.password;
                MailHelper.smtpServer = settings.mailAccount.smtpServer;
                MailHelper.port = settings.mailAccount.port;
                var title = di.status == "在线" ? $"{di.phone}设备恢复在线-不朽玩客云客户端" : $"{di.phone}设备离线-不朽玩客云客户端";
                var result = await MailHelper.SendEmail(settings.mailAccount.mailTo, title, GetNotifyHtml(di));
                Debug.WriteLine($"SendNotifyMail {di.phone}:" + result);
            }
        }
        string GetNotifyHtml(DeviceInfoVM di)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            var status = di.status == "在线" ? $"恢复在线" : $"离线";
            sb.Append($"<p style='display:inline;'>账号{di.phone}的设备</p><p style='display:inline;color:{(di.status == "在线" ? "green" : "red")};'>{status}</p>");
            sb.Append($"<br/>");
            sb.Append($"时间：{DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()}");
            sb.Append($"<br/>");
            sb.Append($"设备详情：");
            sb.Append($"<br/>");
            sb.Append(Properties.Resources.TableStart);
            sb.Append(Properties.Resources.TableContent.Replace("title", "名称").Replace("value", di.device_name));
            sb.Append(Properties.Resources.TableContent.Replace("title", "SN").Replace("value", di.device_sn));
            sb.Append(Properties.Resources.TableContent.Replace("title", "激活状态").Replace("value", di.isActived));
            sb.Append(Properties.Resources.TableContent.Replace("title", "内网IP").Replace("value", di.lan_ip));
            sb.Append(Properties.Resources.TableContent.Replace("title", "外网IP").Replace("value", di.ip));
            sb.Append(Properties.Resources.TableContent.Replace("title", "昨日挖矿").Replace("value", di.yes_wkb.ToString()));
            sb.Append(Properties.Resources.TableContent.Replace("title", "可提币").Replace("value", di.ketiWkb.ToString()));
            sb.Append(Properties.Resources.TableContent.Replace("title", "总收入").Replace("value", di.totalIncome.ToString()));
            sb.Append(Properties.Resources.TableContent.Replace("title", "硬盘容量").Replace("value", di.volume));
            sb.Append(Properties.Resources.TableContent.Replace("title", "CDN上传速度").Replace("value", di.dcdn_upload_speed));
            sb.Append(Properties.Resources.TableContent.Replace("title", "CDN下载速度").Replace("value", di.dcdn_download_speed));
            sb.Append(Properties.Resources.TableContent.Replace("title", "UPNP状态").Replace("value", di.dcdn_upnp_status));
            sb.Append(Properties.Resources.TableContent.Replace("title", "UPNP消息").Replace("value", di.dcdn_upnp_message));
            sb.Append(Properties.Resources.TableContent.Replace("title", "固件版本").Replace("value", di.system_version));
            sb.Append(Properties.Resources.TableContent.Replace("title", "固件能否升级").Replace("value", di.upgradeable));
            sb.Append(Properties.Resources.TableContent.Replace("title", "网络运营商").Replace("value", di.ip_info));
            sb.Append(Properties.Resources.TableContent.Replace("title", "绑定的玩客币地址").Replace("value", di.wkbAddr));
            sb.Append(Properties.Resources.TableEnd);
            sb.Append("</html>");
            return sb.ToString();
        }
        string GetNotifyMarkdown(DeviceInfoVM di)
        {
            StringBuilder sb = new StringBuilder();
            var br = "  " + Environment.NewLine;
            var status = di.status == "在线" ? $"恢复在线" : $"离线";
            sb.Append($"账号{di.phone}的设备{status}");
            sb.Append(br);
            sb.Append($"时间：{DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()}");
            sb.Append(br);
            sb.Append($"设备详情：");
            sb.Append(br);
            sb.Append(Properties.Resources.MdTableStart);
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "名称").Replace("value", di.device_name));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "SN").Replace("value", di.device_sn));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "激活状态").Replace("value", di.isActived));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "内网IP").Replace("value", di.lan_ip));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "外网IP").Replace("value", di.ip));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "昨日挖矿").Replace("value", di.yes_wkb.ToString()));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "可提币").Replace("value", di.ketiWkb.ToString()));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "总收入").Replace("value", di.totalIncome.ToString()));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "硬盘容量").Replace("value", di.volume));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "CDN上传速度").Replace("value", di.dcdn_upload_speed));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "CDN下载速度").Replace("value", di.dcdn_download_speed));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "UPNP状态").Replace("value", di.dcdn_upnp_status));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "UPNP消息").Replace("value", di.dcdn_upnp_message));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "固件版本").Replace("value", di.system_version));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "固件能否升级").Replace("value", di.upgradeable));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "网络运营商").Replace("value", di.ip_info));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "绑定的玩客币地址").Replace("value", di.wkbAddr));
            sb.Append(Properties.Resources.TableEnd);
            return sb.ToString();
        }
        private void NotifyTimer_Tick(object sender, EventArgs e)
        {
            if (settings.mailNotify)
            {
                if (DateTime.Now.Hour == 9 && DateTime.Now.Minute == 1)
                {
                    SendDailyNotifyMail();
                    web_tongji.Refresh();//确保一直挂机检测的客户端也可以做到每日统计一次访问量，而不是只统计第一次打开那天的访问，从而保证访问统计真实性
                }
            }
            if (settings.serverchanNotify)
            {
                if (DateTime.Now.Hour == 9 && DateTime.Now.Minute == 1)
                {
                    SendDailyNotifyServerChan();
                    web_tongji.Refresh();//确保一直挂机检测的客户端也可以做到每日统计一次访问量，而不是只统计第一次打开那天的访问，从而保证访问统计真实性
                }
            }
        }
        private async void SendDailyNotifyMail()
        {
            MailHelper.username = settings.mailAccount.username;
            MailHelper.password = settings.mailAccount.password;
            MailHelper.smtpServer = settings.mailAccount.smtpServer;
            MailHelper.port = settings.mailAccount.port;
            var result = await MailHelper.SendEmail(settings.mailAccount.mailTo, $"{DateTime.Now.ToShortDateString()}汇报-不朽玩客云客户端", GetDailyNotifyHtml(deviceInfos));
            Debug.WriteLine($"SendDailyNotifyMail:" + result);
        }
        private async void SendDailyNotifyServerChan()
        {
            var result = await ServerChanNotify(settings.SCKEY, $"{DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()}汇报-不朽玩客云客户端", GetDailyNotifyMarkdown(deviceInfos));
            Debug.WriteLine($"SendDailyNotifyServerChan:" + result);
        }
        private string GetDailyNotifyHtml(ObservableCollection<DeviceInfoVM> dis)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<body>");
            sb.Append($"<p style='font-weight:bold;'>今日总览</p>");
            sb.Append(Properties.Resources.TableStart);
            sb.Append(Properties.Resources.TableContent.Replace("title", "统计时间").Replace("value", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()));
            sb.Append(Properties.Resources.TableContent.Replace("title", "在线设备数量").Replace("value", tbk_onlineCount.Text));
            sb.Append(Properties.Resources.TableContent.Replace("title", "离线设备数量").Replace("value", tbk_offlineCount.Text));
            sb.Append(Properties.Resources.TableContent.Replace("title", "昨日总收入").Replace("value", tbk_yesAllCoin.Text));
            sb.Append(Properties.Resources.TableContent.Replace("title", "历史总收入").Replace("value", tbk_hisAllCoin.Text));
            sb.Append(Properties.Resources.TableContent.Replace("title", "可提玩客币").Replace("value", tbk_ketiWkb.Text));
            sb.Append(Properties.Resources.TableEnd);
            sb.Append($"<p style='font-weight:bold;'>设备详情</p>");
            foreach (var di in dis)
            {
                sb.Append($"<p style='display:inline;'>账号{di.phone}的设备</p><p style='display:inline;color:{(di.status == "在线" ? "green" : "red")};'>{di.status}</p>");
                sb.Append($"<br/>");
                sb.Append(Properties.Resources.TableStart);
                sb.Append(Properties.Resources.TableContent.Replace("title", "名称").Replace("value", di.device_name));
                sb.Append(Properties.Resources.TableContent.Replace("title", "SN").Replace("value", di.device_sn));
                sb.Append(Properties.Resources.TableContent.Replace("title", "激活状态").Replace("value", di.isActived));
                sb.Append(Properties.Resources.TableContent.Replace("title", "内网IP").Replace("value", di.lan_ip));
                sb.Append(Properties.Resources.TableContent.Replace("title", "外网IP").Replace("value", di.ip));
                sb.Append(Properties.Resources.TableContent.Replace("title", "昨日挖矿").Replace("value", di.yes_wkb.ToString()));
                sb.Append(Properties.Resources.TableContent.Replace("title", "可提币").Replace("value", di.ketiWkb.ToString()));
                sb.Append(Properties.Resources.TableContent.Replace("title", "总收入").Replace("value", di.totalIncome.ToString()));
                sb.Append(Properties.Resources.TableContent.Replace("title", "硬盘容量").Replace("value", di.volume));
                sb.Append(Properties.Resources.TableContent.Replace("title", "CDN上传速度").Replace("value", di.dcdn_upload_speed));
                sb.Append(Properties.Resources.TableContent.Replace("title", "CDN下载速度").Replace("value", di.dcdn_download_speed));
                sb.Append(Properties.Resources.TableContent.Replace("title", "UPNP状态").Replace("value", di.dcdn_upnp_status));
                sb.Append(Properties.Resources.TableContent.Replace("title", "UPNP消息").Replace("value", di.dcdn_upnp_message));
                sb.Append(Properties.Resources.TableContent.Replace("title", "固件版本").Replace("value", di.system_version));
                sb.Append(Properties.Resources.TableContent.Replace("title", "固件能否升级").Replace("value", di.upgradeable));
                sb.Append(Properties.Resources.TableContent.Replace("title", "网络运营商").Replace("value", di.ip_info));
                sb.Append(Properties.Resources.TableContent.Replace("title", "绑定的玩客币地址").Replace("value", di.wkbAddr));
                sb.Append(Properties.Resources.TableEnd);
            }
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }
        private string GetDailyNotifyMarkdown(ObservableCollection<DeviceInfoVM> dis)
        {
            StringBuilder sb = new StringBuilder();
            var br = "  " + Environment.NewLine;
            sb.Append($"今日总览");
            sb.Append(br);
            sb.Append(Properties.Resources.MdTableStart);
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "统计时间").Replace("value", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "在线设备数量").Replace("value", tbk_onlineCount.Text));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "离线设备数量").Replace("value", tbk_offlineCount.Text));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "昨日总收入").Replace("value", tbk_yesAllCoin.Text));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "历史总收入").Replace("value", tbk_hisAllCoin.Text));
            sb.Append(Properties.Resources.MdTableContent.Replace("title", "可提玩客币").Replace("value", tbk_ketiWkb.Text));
            sb.Append(Properties.Resources.TableEnd);
            return sb.ToString();
        }
        async Task<string> ServerChanNotify(string sckey, string text, string desp)
        {
            HttpMessage resp = await ApiHelper.ServerChanNotify(sckey, text, desp);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        return "错误！获取数据为空！";
                    }
                    var r = resp.data as ServerChanResponse;
                    if (r.errmsg == "success")
                    {
                        return "推送成功";
                    }
                    else
                    {
                        return "推送失败:" + JsonHelper.Serialize(r);
                    }
                default:
                    Debug.WriteLine("ServerChanNotify-网络异常错误！");
                    return "错误！网络异常错误！";
            }
        }
        private async void SendNotifyServerChan(DeviceInfoVM di)
        {
            if (settings.serverchanNotify)
            {
                var title = di.status == "在线" ? $"{di.phone}玩客云恢复在线-不朽玩客云客户端" : $"{di.phone}玩客云离线-不朽玩客云客户端";
                var result = await ServerChanNotify(settings.SCKEY, title + " " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), GetNotifyMarkdown(di));
                Debug.WriteLine($"SendNotifyServerChan {di.phone}:" + result);
            }
        }
        private async void btu_serverchanNotify_Click(object sender, RoutedEventArgs e)
        {
            if (settings.serverchanNotify != true)
            {
                if (tbx_serverchan.Text.Trim() == "")
                {
                    MessageBox.Show("SCKEY不能为空！", "提示");
                    return;
                }
                var sckey = tbx_serverchan.Text.Trim();
                var time = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                var r = await ServerChanNotify(sckey, "不朽玩客云客户端-测试推送", "推送服务开启成功 " + time);
                if (r == "推送成功")
                {
                    settings.serverchanNotify = true;
                    settings.SCKEY = sckey;
                    SettingHelper.WriteSettings(settings, password);
                    MessageBox.Show("Server酱推送服务开启成功", "恭喜");
                    btu_serverchanNotify.Content = "关闭推送";
                }
                else
                {
                    MessageBox.Show(r, "错误");
                }
            }
            else
            {
                settings.serverchanNotify = false;
                SettingHelper.WriteSettings(settings, password);
                btu_serverchanNotify.Content = "开启推送";
            }
        }
    }
}