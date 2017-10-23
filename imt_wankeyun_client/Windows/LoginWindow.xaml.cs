using imt_wankeyun_client.Entities;
using imt_wankeyun_client.Entities.Account;
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
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public static LoginResponse loginResponse;
        public string url_login_vali;
        public bool isShowVali = false;
        public bool LoginSuccess = false;
        LoginData ld = new LoginData
        {
            account_type = "4",
            deviceid = UtilHelper.RandomCode(16),
            imeiid = UtilHelper.RandomCode(15),
        };
        public LoginWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/img/icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowVali(false);
            if (tbx_password.Password != null && tbx_password.Password != "")
            {
                tbx_valicode.Focus();
            }
            else
            {
                tbx_username.Focus();
            }
        }
        private async void btu_login_Click(object sender, RoutedEventArgs e)
        {
            await HandleLogin();
        }
        private async void RefreshVali()
        {
            if (url_login_vali != null)
            {
                tbx_tip.Text = "正在加载验证码";
                var image = await ApiHelper.GetValiImg(tbx_username.Text.Trim(), this.url_login_vali);
                if (image != null)
                {
                    img_vali.Source = image;
                    tbx_tip.Visibility = Visibility.Hidden;
                }
                else
                {
                    tbx_tip.Text = "验证码加载失败，点击刷新";
                }
            }
        }
        void ShowVali(bool show)
        {
            if (show)
            {
                tbx_valicode.Visibility = Visibility.Visible;
                img_vali.Visibility = Visibility.Visible;
                label_vali.Visibility = Visibility.Visible;
            }
            else
            {
                tbx_valicode.Visibility = Visibility.Collapsed;
                img_vali.Visibility = Visibility.Collapsed;
                label_vali.Visibility = Visibility.Collapsed;
            }
            isShowVali = show;
        }
        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]");
            e.Handled = re.IsMatch(e.Text);
        }
        private async Task HandleLogin()
        {
            if (ApiHelper.userBasicDatas.ContainsKey(tbx_username.Text.Trim()))
            {
                MessageBox.Show("该账号已经添加！", "提示");
                return;
            }
            ld.phone = tbx_username.Text.Trim();
            ld.pwd = tbx_password.Password.Trim();
            tbx_tip.Text = "正在登陆...";
            HttpMessage resp = await ApiHelper.Login(
                ld.phone, ld.pwd, tbx_valicode.Text.Trim(), ld.account_type, ld.deviceid, ld.imeiid);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    tbx_tip.Text = "";
                    loginResponse = resp.data as LoginResponse;
                    if (loginResponse.iRet == 0)
                    {
                        LoginSuccess = true;
                        MessageBox.Show("登陆成功", "恭喜");
                        Debug.WriteLine(MainWindow.settings.loginDatas == null);
                        if (MainWindow.settings.loginDatas == null)
                        {
                            MainWindow.settings.loginDatas = new List<LoginData>();
                        }
                        MainWindow.settings.loginDatas.Add(ld);
                        SettingHelper.WriteSettings(MainWindow.settings, MainWindow.password);
                        //保存登陆信息
                        ApiHelper.userBasicDatas.Add(loginResponse.data.phone, loginResponse.data);
                        //载入登陆响应信息到主窗口
                        this.Close();
                    }
                    else if (loginResponse.iRet == -121)
                    {
                        MessageBox.Show("验证码输入错误", "错误(-121)");
                        ShowVali(true);
                        RefreshVali();
                        Debug.WriteLine(this.url_login_vali);
                    }
                    else if (loginResponse.iRet == -122)
                    {
                        MessageBox.Show("请输入验证码", "提示(-122)");
                        ShowVali(true);
                        this.url_login_vali = loginResponse.sMsg.Replace(@"\/", @"/");
                        this.url_login_vali = loginResponse.sMsg.Replace(@"http://account.onethingpcs.com/", "");
                        RefreshVali();
                        Debug.WriteLine(this.url_login_vali);
                    }
                    else
                    {
                        LoginSuccess = true;
                        MessageBox.Show(loginResponse.sMsg, $"登陆失败({loginResponse.iRet})");
                    }
                    break;
                default:
                    tbx_tip.Text = "";
                    MessageBox.Show(resp.data.ToString(), "网络异常错误！");
                    break;
            }
        }

        private void img_vali_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RefreshVali();
        }

        private void tbx_valicode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (isShowVali)
                {
                    btu_login_Click(sender, null);
                }
            }
        }

        private void tbx_tip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RefreshVali();
        }

        private void loginWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btu_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbx_password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!isShowVali)
                {
                    btu_login_Click(sender, null);
                }
            }
        }
    }
}
