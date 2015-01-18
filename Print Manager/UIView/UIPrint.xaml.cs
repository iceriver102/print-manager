using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Print_Manager.UIView
{
    /// <summary>
    /// Interaction logic for UIPrint.xaml
    /// </summary>
    public partial class UIPrint : UserControl
    {
        public BitmapImage Image
        {
            get
            {
                return this.UIImage.Source as BitmapImage;
            }
            set
            {
                this.UIImage.Source = value;
                this.UIImage1.Source = value;
            }
        }
        public double Top
        {
            get
            {
                return Canvas.GetTop(this.UIImage);
            }
            set
            {
                Canvas.SetTop(this.UIImage, value);
                Canvas.SetTop(this.UIImage1, value);
            }
        }
        public double Left
        {
            get
            {
                return Canvas.GetLeft(this.UIImage);
            }
            set
            {
                Canvas.SetLeft(this.UIImage, value);
            }
        }
        public double Right
        {
            get
            {
                return Canvas.GetLeft(this.UIImage1);
            }
            set
            {
                Canvas.SetLeft(this.UIImage1, value);
            }
        }
        public UIPrint()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.UIRoot.RenderTransform = new ScaleTransform(this.Width / 1703, this.Height/ 2409);
        }
        public string saveImage(string path, FileTypeImage type = FileTypeImage.PNG)
        {
            return this.UIRoot.saveCanvasToFile(path, type);
        }
        public Stretch @Stretch
        {
            get
            {
                return this.UIImage.Stretch;
            }
            set
            {
                this.UIImage.Stretch = value;
            }
        }
    }
}
