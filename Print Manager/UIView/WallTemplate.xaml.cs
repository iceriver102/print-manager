using Alta.Class;
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

namespace Print_Manager.UIView
{
    /// <summary>
    /// Interaction logic for WallTemplate.xaml
    /// </summary>
    public partial class WallTemplate : UserControl
    {
        public double Top
        {
            get
            {
                return Canvas.GetTop(this.UICrop);
            }
            set
            {

                Canvas.SetTop(this.UICrop, value);
            }
        }
        public BitmapImage Image
        {
            get
            {
                return this.UIImage.Source as BitmapImage;
            }
            set
            {
                this.UIImage.Source = value;
            }
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
        public double Left
        {
            get
            {
                return Canvas.GetLeft(this.UICrop) ;
            }
            set
            {
                Canvas.SetLeft(this.UICrop, value );
                //this.UIImage.Width = this.UIImage.Width - 2 * value;
            }
        }
        public double Right
        {
            get;
            set;
        }

        public string saveImage(string path, FileTypeImage type = FileTypeImage.PNG)
        {
            return this.UIRoot.saveCanvasToFile(path, type);
        }
        public WallTemplate()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.UIRoot.RenderTransform = new ScaleTransform(this.Width / 768, this.Height / 1152);
        }
    }
}
