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
    /// LoadingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
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
        internal void SetTitle(string title)
        {
            tbk_title.Text = title;
            this.Title = title;
        }
        internal void SetTip(string tip)
        {
            this.tbk_tip.Text = tip;
        }
        internal void SetPgr(int value, int maximum)
        {
            this.pgr.Value = value;
            this.pgr.Maximum = maximum;
        }
    }
}
