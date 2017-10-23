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
    /// SetPasswordWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetPasswordWindow : Window
    {
        public SetPasswordWindow()
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
            Environment.Exit(0);
        }

        private void btu_submit_Click(object sender, RoutedEventArgs e)
        {
            if (tbx_password.Password.Length > 32)
            {
                MessageBox.Show("密码的长度不能超过32位", "错误");
                return;
            }
            if (tbx_password.Password != tbx_passwordConfirm.Password)
            {
                MessageBox.Show("两次输入的密码不相同！请重新输入", "错误");
                return;
            }
            var password = tbx_password.Password;
            if (SettingHelper.ReadOldSettings() != null)
            {
                var settings = SettingHelper.ReadOldSettings();//载入旧版设置
                SettingHelper.WriteSettings(settings, password);
                MainWindow.password = password;
                MainWindow.settings = settings;
                MessageBox.Show("设置启动密码成功！已导入您之前的账号列表","恭喜");
                Close();
            }
            else
            {
                var settings = new WankeSettings();//新建设置
                settings.loginDatas = new List<LoginData>();
                settings.autoRefresh = true;
                SettingHelper.WriteSettings(settings, password);
                MainWindow.settings = settings;
                MainWindow.password = password;
                MessageBox.Show("设置启动密码成功！", "恭喜");
                Close();
            }
        }
    }
}
