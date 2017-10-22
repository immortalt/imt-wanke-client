using imt_wankeyun_client.Entities;
using imt_wankeyun_client.Entities.Account;
using imt_wankeyun_client.Entities.Control.RemoteDL;
using imt_wankeyun_client.Helpers;
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
                        MessageBox.Show("错误的资源！无法下载", $"错误(r.rtn)");
                        Debug.WriteLine($"UrlResolve-获取数据出错{r.rtn}:{r.rtn}");
                    }
                    return null;
                default:
                    Debug.WriteLine("UrlResolve-网络异常错误！");
                    return null;
            }
        }
        async Task<int> CreateTask(string phone, CreateTaskInfo cti)
        {
            HttpMessage resp = await ApiHelper.CreateTask(phone, cti);
            switch (resp.statusCode)
            {
                case HttpStatusCode.OK:
                    if (resp.data == null)
                    {
                        return -1;
                    }
                    var r = resp.data as CreateTaskResponse;
                    if (r.rtn == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        Debug.WriteLine($"CreateTask-出错({r.rtn})");
                    }
                    return r.rtn;
                default:
                    Debug.WriteLine("CreateTask-网络异常错误！");
                    return -1;
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
                        path = "/media/sda1/onecloud/tddownload",
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
                    if (ctk == 0)
                    {
                        Close();
                        MessageBox.Show("添加成功！", "提示");
                    }
                    else
                    {
                        Close();
                        MessageBox.Show("添加失败！", $"错误({ctk})");
                    }
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
                tbx_size.Text = UtilHelper.ConvertToSizeString(taskInfo.size);
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
    }
}
