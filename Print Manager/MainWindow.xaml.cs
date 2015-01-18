using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Alta.Class;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using Print_Manager.UIView;

namespace Print_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread CheckThread;
        Thread PrintThread;
        System.IO.DirectoryInfo @Directory;
        bool isAutoPrint = false;
        public MainWindow()
        {
            InitializeComponent();
            CheckThread = new Thread(FunctionThread);
            CheckThread.IsBackground = true;
            PrintThread = new Thread(PrintThreadFunction);
            PrintThread.SetApartmentState(ApartmentState.STA);
            PrintThread.IsBackground = true;
        }

        private void PrintThreadFunction()
        {
            while (true)
            {
                if (isAutoPrint)
                {
                    for (int i = 0; i < App.Datas.Count; i++)
                    {
                        FileData f = App.Datas.get(i);
                        if (!f.isPrint)
                        {
                            App.Datas.Print(f);
                            FileInfo inf = new FileInfo(f.FullName);
                            if (App.setting.Template)
                            {
                                BitmapImage bit = new BitmapImage();
                                bit.BeginInit();
                                bit.UriSource = inf.toUri();
                                bit.EndInit();
                                UIPrint Print = new UIPrint();
                                Print.Width = 3508;
                                Print.Height = 2480;
                                Print.Image = bit;
                                Print.Left = App.setting.LeftMargin;
                                Print.Right = App.setting.RightMargin;
                                Print.Top = App.setting.TopMargin;
                                Print.Stretch = App.setting.Fill;
                                FileInfo temp = new FileInfo(Print.saveImage(App.setting.CacheFolder));
                                temp.PrintImageDefault();
                                Print = null;
                            }
                            else
                            {
                                inf.PrintImageDefault();
                            }
                            this.updateListView(App.Datas.Datas);
                            break;
                        }
                    }
                }
                Thread.Sleep(App.setting.TimePrint);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Directory = new System.IO.DirectoryInfo(App.setting.DataFolder);
            this.updateListView(App.Datas.Datas);
            CheckThread.Start();
            PrintThread.Start();
        }
        void updateListView(List<FileData> files)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                 delegate ()
                 {
                     this.listView.Items.Clear();
                     if (files == null || files.Count == 0)
                         return;
                     for (int i = 0; i < files.Count; i++)
                     {
                         this.listView.Items.Add(files[i]);
                     }
                 }));
        }
        void FunctionThread()
        {
            while (true)
            {
                bool isChange = false;
                isChange=!App.Datas.Optimize();
                int count = App.Datas.Count;
                try
                {
                    var tmp = this.Directory.getAllFiles(".jpg", ".png");

                    if (tmp != null)
                    {
                        FileInfo[] files = tmp.ToArray();
                        for (int i = 0; i < files.Length; i++)
                        {
                            if (count > 0)
                            {
                                bool isNew = true;
                                for (int j = 0; j < count; j++)
                                {
                                    if (files[i].Name.CompareTo(App.Datas.get(j).Name) == 0)
                                    {
                                        isNew = false;
                                        break;
                                    }
                                }
                                if (isNew)
                                {
                                    isChange = true;
                                    App.Datas.Add(new FileData() { Name = files[i].Name, FullName = files[i].FullName });
                                }
                            }
                            else
                            {
                                isChange = true;
                                App.Datas.Add(new FileData() { Name = files[i].Name, FullName = files[i].FullName });

                            }
                        }

                    }
                }
                catch (Exception)
                {

                }
                if (isChange)
                    this.updateListView(App.Datas.Datas);

                Thread.Sleep(App.setting.CountTime);
            }
        }


        private void Print_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            FileData f = b.CommandParameter as FileData;
            App.Datas.Print(f);
            FileInfo inf = new FileInfo(f.FullName);
            if (App.setting.Template)
            {
                UIPrint Print = new UIPrint();
                Print.Width = 3508;
                Print.Height = 2480;
                Print.Left = App.setting.LeftMargin;
                Print.Right = App.setting.RightMargin;
                Print.Top = App.setting.TopMargin;
                BitmapImage bit = new BitmapImage();
                bit.BeginInit();
                bit.UriSource = inf.toUri();
                bit.EndInit();
                Print.Image = bit;
                Print.Stretch = App.setting.Fill;
                FileInfo temp = new FileInfo(Print.saveImage(App.setting.CacheFolder,FileTypeImage.JPG));
                temp.PrintImageDefault();
                Print = null;
            }
            else
            {
                inf.PrintImageDefault();
            }

            this.updateListView(App.Datas.Datas);
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            FileData f = b.CommandParameter as FileData;
            Process photoViewer = new Process();
            photoViewer.StartInfo.FileName = f.FullName;
            photoViewer.Start();
        }

        private void UICheckAuto_Checked(object sender, RoutedEventArgs e)
        {
            this.isAutoPrint = this.UICheckAuto.IsChecked.Value;
        }

        private void UICheckAuto_Unchecked(object sender, RoutedEventArgs e)
        {
            this.isAutoPrint = this.UICheckAuto.IsChecked.Value;
        }
    }
}
