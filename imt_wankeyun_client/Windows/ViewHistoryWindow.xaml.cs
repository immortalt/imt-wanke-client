using imt_wankeyun_client.Entities.WKB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        List<Income> historyArr;
        public List<double> incomeArr = new List<double>();
        string phone;
        public ViewHistoryWindow(string phone, IncomeHistory incomeHistory)
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/img/icon.ico", UriKind.RelativeOrAbsolute);
            this.phone = phone;
            this.Icon = BitmapFrame.Create(iconUri);
            historyArr = new List<Income>();
            if (incomeHistory != null)
            {
                var ih = incomeHistory.incomeArr.OrderBy(t => t.date).ToList();
                chart_history.DataSource = ih;
                for (var i = 0; i < ih.Count; i++)
                {
                    try
                    {
                        var ic = new Income
                        {
                            date = ih[i].date,
                            num = ih[i].num
                        };
                        historyArr.Add(ic);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lv_incomeHistory.ItemsSource = null;
            lv_incomeHistory.ItemsSource = historyArr.OrderByDescending(t => t.date).ToList();
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

        private void btu_export_detail_excel_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            sfd.Filter = "csv文件(*.csv)|*.csv";
            sfd.RestoreDirectory = true;
            sfd.FileName = phone + "历史收入" + DateTime.Now.ToLongDateString() + "-不朽玩客云客户端.csv";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string filename = sfd.FileName;
                    System.IO.File.WriteAllText(filename, GetHistroyCSV(historyArr.OrderByDescending(t => t.date).ToList()), Encoding.Default);
                    MessageBox.Show("导出成功！", "提示");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败！" + ex.Message, "提示");
                }
            }
        }
        private string GetHistroyCSV(List<Income> ins)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("日期,收入");
            sb.Append(Environment.NewLine);
            var dot = ",";
            foreach (var inc in ins)
            {
                sb.Append(inc.date);
                sb.Append(dot);
                sb.Append(inc.num);
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
