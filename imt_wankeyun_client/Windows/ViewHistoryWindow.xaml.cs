using imt_wankeyun_client.Entities;
using imt_wankeyun_client.Entities.Account;
using imt_wankeyun_client.Entities.WKB;
using imt_wankeyun_client.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace imt_wankeyun_client.Windows
{
    /// <summary>
    /// ViewHistoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ViewHistoryWindow : Window
    {
        List<Income> historyArr = new List<Income>();
        public ViewHistoryWindow(IncomeHistory incomeHistory)
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/img/icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            if (incomeHistory != null)
            {
                historyArr = incomeHistory.incomeArr;
                for (var i = 0; i < historyArr.Count; i++)
                {
                    var dtstr = historyArr[i].date;
                    var dt = new DateTime(int.Parse(dtstr.Substring(0, 4)),
                        int.Parse(dtstr.Substring(4, 2)), int.Parse(dtstr.Substring(6, 2)));
                    historyArr[i].date = dt.ToLongDateString();
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lv_incomeHistory.ItemsSource = null;
            lv_incomeHistory.ItemsSource = historyArr;
            AutoHeaderWidth(lv_incomeHistory);
        }
        private void loginWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void btu_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
    }
}
