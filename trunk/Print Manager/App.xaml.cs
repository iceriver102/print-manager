using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Alta.Class;
using Print_Manager.Class;

namespace Print_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string FileName="setting.xml";
        public static Config setting;
        public static FileDatas Datas;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            setting = Config.Read(FileName);
            if (setting.CacheFolder.IndexOf(":") < 0 || setting.CacheFolder.IndexOf("\\")<0)
            {
                setting.CacheFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, setting.CacheFolder);
            }
            if (!System.IO.Directory.Exists(setting.CacheFolder))
            {
                System.IO.Directory.CreateDirectory(setting.CacheFolder);
            }else
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(setting.CacheFolder);
                dir.deleteAll();
            }
            Datas = FileDatas.Read(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, setting.cacheName));
            this.MainWindow = new MainWindow();
            this.MainWindow.Show();
        }
    }
}
