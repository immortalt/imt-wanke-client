using imt_wankeyun_client.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace imt_wankeyun_client
{
    /// <summary>
    /// AdWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AdWindow : Window
    {
        public AdWindow(string pic)
        {
            InitializeComponent();
            try
            {
                this.img_ad.Source = new BitmapImage(new Uri(pic));
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载失败");
            }
        }

        private void About1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
