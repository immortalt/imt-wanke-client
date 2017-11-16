using imt_wankeyun_client.Helpers;
using System;
using System.IO;
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
        bool isAuthed = false;
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
            isAuthed = false;
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
                //MessageBox.Show("验证成功！", "恭喜");
                isAuthed = true;
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
        private void btu_import_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ofd.Filter = "不朽玩客云配置文件(*.ini)|*.ini";
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string filename = ofd.FileName;
                    File.Copy(filename, SettingHelper.settingPath, true);
                    MessageBox.Show("导入成功！", "提示");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导入失败！" + ex.Message, "提示");
                }
            }
        }
        int exp = 1;
        private void btu_export_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            sfd.Filter = "不朽玩客云配置文件(*.ini)|*.ini";
            sfd.RestoreDirectory = true;
            sfd.FileName = DateTime.Now.ToLongDateString() + (exp++) + ".ini";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string filename = sfd.FileName;
                    File.Copy(SettingHelper.settingPath, filename);
                    MessageBox.Show("导出成功！", "提示");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败！" + ex.Message, "提示");
                }
            }
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isAuthed)
            {
                Environment.Exit(0);
            }
        }
    }
}
