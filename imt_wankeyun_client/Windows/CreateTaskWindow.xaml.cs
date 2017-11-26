using imt_wankeyun_client.Entities;
using imt_wankeyun_client.Entities.Account;
using imt_wankeyun_client.Entities.Control.RemoteDL;
using imt_wankeyun_client.Helpers;
using MonoTorrent.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace imt_wankeyun_client.Windows
{
    /// <summary>
    /// CreateTaskWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CreateTaskWindow : Window
    {
        TaskInfo taskInfo;
        public CreateTaskWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("pack://application:,,,/img/icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            grid_info.IsEnabled = false;
            grid_info.Opacity = 0.5;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbx_url.Focus();
        }
        async Task<TaskInfo> UrlResolve(string phone, string url)
        {
            HttpMessage resp = await ApiHelper.UrlResolve(phone, url);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        return null;
                    }
                    var r = resp.data as TaskInfoResponse;
                    if (r.rtn == 0)
                    {
                        Debug.WriteLine(r.taskInfo.name);
                        return r.taskInfo;
                    }
                    else
                    {
                        MessageBox.Show("错误的资源！无法下载", $"错误({r.rtn})");
                        Debug.WriteLine($"UrlResolve-获取数据出错{r.rtn}:{r.rtn}");
                    }
                    return null;
                default:
                    Debug.WriteLine("UrlResolve-网络异常错误！");
                    return null;
            }
        }
        async Task<string> CreateTask(string phone, CreateTaskInfo cti)
        {
            HttpMessage resp = await ApiHelper.CreateTask(phone, cti);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        return "添加失败！服务器返回的数据为空";
                    }
                    var r = resp.data as CreateTaskResponse;
                    if (r.rtn == 0)
                    {
                        if (r.tasks != null && r.tasks.Count > 0)
                        {
                            var t = r.tasks[0];
                            if (t.result == 0)
                            {
                                return $"任务{t.name}添加成功！";
                            }
                            else
                            {
                                var tmsg = t.msg == "repeat_task" ? "已经存在相同的任务！无法重复添加" : t.msg;
                                return $"添加失败！\n错误码:{t.result}\n错误原因:{tmsg}";
                            }
                        }
                        else
                        {
                            return "添加失败！服务器返回的结果为空";
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"CreateTask-出错({r.rtn})");
                        return "添加失败！网络请求错误";
                    }
                default:
                    Debug.WriteLine("CreateTask-网络异常错误！");
                    return "添加失败！网络异常错误"; ;
            }
        }
        private async void btu_createTask_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.curAccount != null)
            {
                if (taskInfo != null)
                {
                    CreateTaskInfo cti = new CreateTaskInfo
                    {
                        path = tbx_savePath.Text.Trim(),
                        tasks = new List<CreateTask>{
                                new CreateTask
                            {
                                name =taskInfo.name,
                                url = taskInfo.url,
                                filesize = 0
                            }
                        }
                    };
                    var ctk = await CreateTask(MainWindow.curAccount, cti);
                    Close();
                    MessageBox.Show(ctk, "提示");
                }
            }
        }

        private void window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void btu_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        void RefreshTaskInfo()
        {
            if (taskInfo != null)
            {
                string ext = Path.GetExtension(taskInfo.name);
                if (ext.Length >= 1)
                {
                    ext = ext.Substring(1, ext.Length - 1);
                }
                tbx_filename.Text = taskInfo.name;
                tbx_size.Text = UtilHelper.ConvertToSizeString(Convert.ToUInt64(taskInfo.size));
                tbx_format.Text = ext;
                grid_info.Opacity = 1;
                grid_info.IsEnabled = true;
            }
        }
        private async void btu_urlResolve_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.curAccount != null)
            {
                taskInfo = await UrlResolve(MainWindow.curAccount, tbx_url.Text.Trim());
                RefreshTaskInfo();
            }
        }

        private void btu_urlResolveTorrent_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ofd.Filter = "种子文件(*.torrent)|*.torrent";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string filename = ofd.FileName;
                    var MonoTorrent = Torrent.Load(filename);
                    var url = "magnet:?xt=urn:btih:" + BitConverter.ToString(MonoTorrent.InfoHash.ToArray()).Replace("-", "");
                    tbx_url.Text = url;
                    MessageBox.Show("打开种子成功！", "提示");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打开种子失败！" + ex.Message, "提示");
                }
            }
        }
    }
}
