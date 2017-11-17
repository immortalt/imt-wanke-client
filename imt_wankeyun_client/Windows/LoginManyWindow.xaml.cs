using imt_wankeyun_client.Entities;
using imt_wankeyun_client.Entities.Account;
using imt_wankeyun_client.Entities.Control;
using imt_wankeyun_client.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace imt_wankeyun_client.Windows
{
    /// <summary>
    /// LoadingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginManyWindow : Window
    {
        public static LoginResponse loginResponse;
        public string url_login_vali;
        public bool isShowVali = false;
        public static bool LoginSuccess = false;
        LoginData ld;
        public LoginManyWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/img/icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void loginWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void btu_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btu_loginMany_Click(object sender, RoutedEventArgs e)
        {
            await HandleLogin();
            btu_loginMany.Visibility = Visibility.Collapsed;
        }
        private async Task HandleLogin()
        {
            try
            {
                string result = "";
                string[] accounts = tbx_loginMany.Text.Split('\n');
                var ldw = new LoadingWindow();
                ldw.Show();
                ldw.SetTitle("登陆中");
                ldw.SetTip("正在登陆");
                for (var i = 0; i < accounts.Length; i++)
                {
                    try
                    {
                        ldw.SetPgr(0, i);
                        tbx_loginMany.Text = result;
                        var ac = accounts[i];
                        var tr = "";
                        if (ac.Length < 13)
                        {
                            tr = $"第{i + 1}个账号密码{ac}:账号或密码格式错误" + Environment.NewLine;
                            result += tr;
                            continue;
                        }
                        var phone = ac.Substring(0, 11);
                        var pwd = ac.Substring(12, ac.Length - 13);
                        if (ApiHelper.userBasicDatas.ContainsKey(phone))
                        {
                            tr = $"第{i + 1}个账号{phone}:该账号已经添加" + Environment.NewLine;
                            result += tr;
                            continue;
                        }
                        ld = new LoginData
                        {
                            account_type = "4",
                            deviceid = UtilHelper.RandomCode(16),
                            imeiid = UtilHelper.RandomCode(15),
                            phone = phone,
                            pwd = pwd
                        };
                        HttpMessage resp = await ApiHelper.Login(
                        ld.phone, ld.pwd, "", ld.account_type, ld.deviceid, ld.imeiid);
                        switch (resp.statusCode)
                        {
                            case HttpStatusCode.OK:
                                loginResponse = resp.data as LoginResponse;
                                if (loginResponse.iRet == 0)
                                {
                                    var devices = await GetDevices(ld.phone);
                                    if (devices == null || devices.Count == 0)
                                    {
                                        tr = $"第{i + 1}个账号{phone}:请先用app绑定玩客云设备" + Environment.NewLine;
                                        result += tr;
                                        continue;
                                    }
                                    tr = $"第{i + 1}个账号{phone}:登陆成功！" + Environment.NewLine;
                                    result += tr;
                                    if (MainWindow.settings.loginDatas == null)
                                    {
                                        MainWindow.settings.loginDatas = new List<LoginData>();
                                    }
                                    MainWindow.settings.loginDatas.Add(ld);
                                    SettingHelper.WriteSettings(MainWindow.settings, MainWindow.password);
                                    //保存登陆信息
                                    ApiHelper.userBasicDatas.Add(loginResponse.data.phone, loginResponse.data);
                                }
                                else if (loginResponse.iRet == -121)
                                {
                                    tr = $"第{i + 1}个账号{phone}:验证码输入错误(-121)" + Environment.NewLine;
                                    result += tr;
                                    continue;
                                }
                                else if (loginResponse.iRet == -122)
                                {
                                    tr = $"第{i + 1}个账号{phone}:请输入验证码(-122)" + Environment.NewLine;
                                    result += tr;
                                    continue;
                                }
                                else
                                {
                                    tr = $"第{i + 1}个账号{phone}:登陆失败({loginResponse.iRet})" + Environment.NewLine;
                                    result += tr;
                                    continue;
                                }
                                break;
                            default:
                                tr = $"第{i + 1}个账号{phone}:网络异常错误" + Environment.NewLine;
                                result += tr;
                                continue;

                        }
                    }
                    catch (Exception ex)
                    {
                        var tr = $"第{i + 1}个账号出现错误:{ex.Message}" + Environment.NewLine;
                        result += tr;
                        continue;
                    }
                }
                ldw.Close();
                tbx_loginMany.Text = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "发生错误");
            }
        }
        async Task<List<Device>> GetDevices(string phone)
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
                                Devices devicesArr = JsonHelper.Deserialize<Devices>(pr.result[1].ToString());
                                var devices = devicesArr.devices;
                                return devices;
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("获取数据出错！");
                    }
                    return null;
                default:
                    Debug.WriteLine("网络异常错误！");
                    return null;
            }
        }

    }
}