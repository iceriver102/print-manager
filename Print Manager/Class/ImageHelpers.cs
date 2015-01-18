using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Alta.Class
{
    public enum FileTypeImage
    {
        JPG, PNG
    }
    public static class ImageHelpers
    {

        private const int MaxDepthDistance = 4000;
        private const int MinDepthDistance = 850;
        private const int MaxDepthDistanceOffset = 3150;

        public static byte CalculateIntensityFromDistance(int distance)
        {
            // This will map a distance value to a 0 - 255 range
            // for the purposes of applying the resulting value
            // to RGB pixels.
            int newMax = distance - MinDepthDistance;
            if (newMax > 0)
                return (byte)(255 - (255 * newMax
                / (MaxDepthDistanceOffset)));
            else
                return (byte)255;
        }


        public static System.Drawing.Bitmap ToBitmap(this BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                // from System.Media.BitmapImage to System.Drawing.Bitmap
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
                return bitmap;
            }
        }


        [System.Runtime.InteropServices.DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        private static BitmapImage toBitmapImage(this System.Drawing.Bitmap bitmap, FileTypeImage Type= FileTypeImage.JPG)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();

            try
            {
                var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
                MemoryStream memoryStream = new MemoryStream();
                BitmapImage bImg = new BitmapImage();
                if (Type == FileTypeImage.JPG)
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(memoryStream);
                  
                }
                else
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder(); 
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(memoryStream);
                    
                }
                bImg.BeginInit();
                bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
                bImg.EndInit();

                memoryStream.Close();

                return bImg;
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            
        }
        /*
        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// </summary>
        /// <param name="image">The Emgu CV Image</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource ToBitmapSource(this Emgu.CV.IImage image)
        {
            try
            {
                using (System.Drawing.Bitmap source = image.Bitmap)
                {
                    IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                    BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        ptr,
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    DeleteObject(ptr);
                    return bs;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        */
        public static BitmapImage getbitmapImage(String path)
        {
            BitmapImage Dummy = new BitmapImage();
            try
            {

                Uri UriTarget = new Uri(AppDomain.CurrentDomain.BaseDirectory + path);
                Dummy = new BitmapImage();
                Dummy.BeginInit();
                Dummy.UriSource = UriTarget;
                Dummy.EndInit();
                return Dummy;
            }
            catch (Exception ex)
            {
                //   CommonUtilities.showMsgDebug(ex.Message, "Loi load file");
                return null;
            }
        }
        public static BitmapSource ToWpfBitmap(this System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
        public static BitmapImage convertCanvasToImage(this System.Windows.Controls.Canvas cvLayout, FileTypeImage type = FileTypeImage.JPG)
        {
            cvLayout.Measure(new System.Windows.Size(cvLayout.Width, cvLayout.Height));
            cvLayout.Arrange(new System.Windows.Rect(new System.Windows.Point() { X = 0, Y = 0 }, new System.Windows.Size(cvLayout.Width, cvLayout.Height)));
            var rtb = new RenderTargetBitmap(
                (int)cvLayout.Width, //width 
                (int)cvLayout.Height, //height 
                96d, //dpi x 
                96d, //dpi y 
                System.Windows.Media.PixelFormats.Pbgra32 // pixelformat 
                );
            rtb.Render(cvLayout);
            rtb.Render(cvLayout);

            var image = new PngBitmapEncoder();
            image.Frames.Add(BitmapFrame.Create(rtb));
            MemoryStream stream = new MemoryStream();
            image.Save(stream);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(stream);
            stream.Close();
            bitmap = CropWhiteSpace(bitmap, System.Drawing.Color.Black);
            return CropWhiteSpace(bitmap, System.Drawing.Color.Transparent).toBitmapImage(type);
        }
        public static String saveImageToJPG(this System.Drawing.Image img, String path)
        {

            DateTime currentDate = DateTime.Now;
            string savePath = path + @"\AM_" + currentDate.Ticks + ".jpg";

            using (System.Drawing.Bitmap b = new System.Drawing.Bitmap(img.Width, img.Height))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b))
                {
                    g.Clear(System.Drawing.Color.White);
                    g.DrawImageUnscaled(img, 0, 0);
                    img.Save(savePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return savePath;
                }
            }

        }

        public static System.Drawing.Image convertBitmapImgToImg(BitmapImage bitmapImage)
        {
            var ms = new MemoryStream();
            var encoder = new PngBitmapEncoder(); // With this we also respect transparency.
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(ms);
            var bmp = new System.Drawing.Bitmap(ms);
            return System.Drawing.Image.FromStream(ms);
        }
        public static System.Drawing.Bitmap CropWhiteSpace(System.Drawing.Bitmap bmp, System.Drawing.Color color)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int white = 0;


            Func<int, bool> allWhiteRow = r =>
            {
                for (int i = 0; i < w; ++i)
                    if ((bmp.GetPixel(i, r).A) != white)
                        return false;
                return true;
            };

            Func<int, bool> allWhiteColumn = c =>
            {
                for (int i = 0; i < h; ++i)
                    if ((bmp.GetPixel(c, i).A) != white)
                        return false;
                return true;
            };

            int topmost = 0;
            for (int row = 0; row < h; ++row)
            {
                if (!allWhiteRow(row))
                    break;
                topmost = row;
            }

            int bottommost = 0;
            for (int row = h - 1; row >= 0; --row)
            {
                if (!allWhiteRow(row))
                    break;
                bottommost = row;
            }

            int leftmost = 0, rightmost = 0;
            for (int col = 0; col < w; ++col)
            {
                if (!allWhiteColumn(col))
                    break;
                leftmost = col;
            }

            for (int col = w - 1; col >= 0; --col)
            {
                if (!allWhiteColumn(col))
                    break;
                rightmost = col;
            }

            if (rightmost == 0) rightmost = w; // As reached left
            if (bottommost == 0) bottommost = h; // As reached top.

            int croppedWidth = rightmost - leftmost;
            int croppedHeight = bottommost - topmost;

            if (croppedWidth == 0) // No border on left or right
            {
                leftmost = 0;
                croppedWidth = w;
            }

            if (croppedHeight == 0) // No border on top or bottom
            {
                topmost = 0;
                croppedHeight = h;
            }

            try
            {
                var target = new System.Drawing.Bitmap(croppedWidth, croppedHeight);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(target))
                {
                    g.DrawImage(bmp,
                      new System.Drawing.RectangleF(0f, 0f, croppedWidth, croppedHeight),
                      new System.Drawing.RectangleF(leftmost, topmost, croppedWidth, croppedHeight),
                      System.Drawing.GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Values are topmost={0} btm={1} left={2} right={3} croppedWidth={4} croppedHeight={5}", topmost, bottommost, leftmost, rightmost, croppedWidth, croppedHeight),
                  ex);
            }
        }
        public static String saveCanvasToFile(this System.Windows.Controls.Canvas canvas, string pathDirectory = @"Data", FileTypeImage type = FileTypeImage.JPG)
        {
            if (!Directory.Exists(pathDirectory))
            {
                Directory.CreateDirectory(pathDirectory);
            }
            DateTime currentDate = DateTime.Now;
            string path = "";
            if (type == FileTypeImage.PNG)
            {
                path = pathDirectory + @"\AM_" + currentDate.Ticks + ".png";
            }
            else
            {
                path = pathDirectory + @"\AM_" + currentDate.Ticks + ".jpg";
            }
            canvas.Measure(new System.Windows.Size(canvas.Width, canvas.Height));
            canvas.Arrange(new System.Windows.Rect(new System.Windows.Size(canvas.Width, canvas.Height)));
            SaveImage(canvas, (int)canvas.Width, (int)canvas.Height, path,type);
            return path;
        }
        public static void SaveImage(System.Windows.Media.Visual visual, int width, int height, string filePath, FileTypeImage type = FileTypeImage.JPG)
        {
            var RenderBitmap = new RenderTargetBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            RenderBitmap.Render(visual);
            PngBitmapEncoder image = new PngBitmapEncoder();
            image.Frames.Add(BitmapFrame.Create(RenderBitmap));
            MemoryStream stream = new MemoryStream();
            image.Save(stream);
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(stream);

            if (type == FileTypeImage.PNG)
            {
                bitmap = CropWhiteSpace(bitmap, System.Drawing.Color.Transparent);
                bitmap.Save(filePath);               
            }
            else
            {           
               // bitmap = CropWhiteSpace(bitmap, System.Drawing.Color.Black);
                bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            stream.Close();

        }
        public static BitmapSource ConvertToBitmapSource(UIElement element)
        {
            var target = new RenderTargetBitmap((int)(element.RenderSize.Width), (int)(element.RenderSize.Height), 96, 96, PixelFormats.Pbgra32);
            var brush = new VisualBrush(element);
            var visual = new DrawingVisual();
            var drawingContext = visual.RenderOpen();
            drawingContext.DrawRectangle(brush, null, new Rect(new Point(0, 0), new Point(element.RenderSize.Width, element.RenderSize.Height)));
            drawingContext.Close();
            target.Render(visual);
            return target;
        }
        public static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
        public static byte[] BufferFromImage(this System.Windows.Controls.Canvas canvas)
        {
            canvas.Measure(new System.Windows.Size(canvas.Width, canvas.Height));
            canvas.Arrange(new System.Windows.Rect(new System.Windows.Size(canvas.Width, canvas.Height)));
            MemoryStream ms = new MemoryStream();
            var bitmap = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            bitmap.Render(canvas);
            var image = new PngBitmapEncoder();
            image.Frames.Add(BitmapFrame.Create(bitmap));
            image.Save(ms);
            return ms.ToArray();
        }
        public static string ImageToBase64(System.Drawing.Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
        public static string BitmapToBase64(BitmapImage bi)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bi));
                encoder.Save(ms);
                byte[] bitmapdata = ms.ToArray();
                return Convert.ToBase64String(bitmapdata);
            }
            catch (Exception)
            {
                return String.Empty;
            }

        }
        public static BitmapSource ToWpfBitmap(this String base64)
        {
            return ToWpfBitmap(Base64StringToBitmap(base64));
        }
        public static System.Drawing.Bitmap Base64StringToBitmap(this string base64String)
        {
            if (base64String == "")
                return null;
            System.Drawing.Bitmap bmpReturn = null;
            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);
            memoryStream.Position = 0;
            bmpReturn = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
            memoryStream.Close();
            memoryStream = null;
            byteBuffer = null;
            return bmpReturn;

        }
        public static String WriteJpeg(this BitmapSource bmp, int quality, string pathDirectory = @"Data")
        {
            DateTime currentDate = DateTime.Now;
            string fileName = pathDirectory + @"\AM_" + currentDate.Ticks + ".jpg";
            //string fileName=
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            BitmapFrame outputFrame = BitmapFrame.Create(bmp);
            encoder.Frames.Add(outputFrame);
            encoder.QualityLevel = quality;
            using (FileStream file = File.OpenWrite(fileName))
            {
                encoder.Save(file);
            }
            return fileName;
        }
        public static void SaveBase64ToFile(String base64String, String filePath, FileTypeImage type = FileTypeImage.JPG)
        {
            if (base64String == "")
                return;
            System.Drawing.Bitmap bmpReturn = null;

            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);
            memoryStream.Position = 0;

            bmpReturn = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
            if (type == FileTypeImage.JPG)
                bmpReturn.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            else
                bmpReturn.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Close();
            memoryStream = null;
            byteBuffer = null;
        }
        /*

        internal static String SaveImage(this Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> ImageFrame, string pathDirectory = @"Data")
        {
            DateTime currentDate = DateTime.Now;
            string fileName = pathDirectory + @"\AM_" + currentDate.Ticks + ".jpg";
            ImageFrame.Save(fileName);
            return fileName;
        }
        */
    }
}