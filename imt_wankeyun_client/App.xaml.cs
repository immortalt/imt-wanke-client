using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace imt_wankeyun_client
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        System.Threading.Mutex mutex;
        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
        }
        void App_Startup(object sender, StartupEventArgs e)
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, "imt-wanke-client", out ret);
            if (!ret)
            {
                MessageBox.Show("已有一个程序实例运行", "提示");
                Environment.Exit(0);
            }

        }
    }
}
