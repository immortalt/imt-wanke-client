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
    /// AuthWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/img/icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbx_password.Focus();
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
            var password = tbx_password.Password;
            var settings = SettingHelper.ReadSettings(password);
            if (settings != null)
            {
                MainWindow.password = password;
                MainWindow.settings = settings;
                MessageBox.Show("验证成功！", "恭喜");
                Close();
            }
            else
            {
                MessageBox.Show("密码错误！", "提示");
            }
        }

        private void btu_delete_Click(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show("确定删除配置文件（包括所有的账号和密码）？", "提示", MessageBoxButton.OKCancel);
            if (r == MessageBoxResult.OK)
            {
                SettingHelper.DeleteSettings();
                MessageBox.Show("已删除配置文件！请重启程序", "提示");
                Environment.Exit(0);
            }
        }

        private void tbx_password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btu_submit_Click(btu_submit, null);
            }
        }
    }
}
