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
    /// SortWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SortWindow : Window
    {
        public SortWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/img/icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var sb = MainWindow.settings.SortBy;
            switch (sb)
            {
                case "设备名称":
                    cbx_sort.SelectedIndex = 0;
                    break;
                case "昨日收入":
                    cbx_sort.SelectedIndex = 1;
                    break;
                case "可提玩客币":
                    cbx_sort.SelectedIndex = 2;
                    break;
                case "总收入":
                    cbx_sort.SelectedIndex = 3;
                    break;
                case "手机号":
                    cbx_sort.SelectedIndex = 4;
                    break;
                case "在线状态":
                    cbx_sort.SelectedIndex = 5;
                    break;
                case "不排序":
                    cbx_sort.SelectedIndex = 6;
                    break;
                default:
                    cbx_sort.SelectedIndex = 0;
                    break;
            }
            cbx_sortOrder.SelectedIndex = MainWindow.settings.SortOrder;
            tbx_search.Text = MainWindow.searchWord;
        }
        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]");
            e.Handled = re.IsMatch(e.Text);
        }

        private void loginWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btu_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbx_sort_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string sb = "设备名称";
            switch (cbx_sort.SelectedIndex)
            {
                case 0:
                    sb = "设备名称";
                    break;
                case 1:
                    sb = "昨日收入";
                    break;
                case 2:
                    sb = "可提玩客币";
                    break;
                case 3:
                    sb = "总收入";
                    break;
                case 4:
                    sb = "手机号";
                    break;
                case 5:
                    sb = "在线状态";
                    break;
                case 6:
                    sb = "不排序";
                    break;
            }
            MainWindow.settings.SortBy = sb;
            SettingHelper.WriteSettings(MainWindow.settings, MainWindow.password);
        }

        private void cbx_sortOrder_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MainWindow.settings.SortOrder = cbx_sortOrder.SelectedIndex;
            SettingHelper.WriteSettings(MainWindow.settings, MainWindow.password);
        }

        private void btu_search_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.searchWord = tbx_search.Text.Trim();
            this.Close();
        }
    }
}
