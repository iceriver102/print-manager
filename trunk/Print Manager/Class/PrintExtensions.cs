using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Alta.Class
{
    public static class PrintExtensions
    {
        private static FileInfo fileInf;
        public static void PrintImageDefault(this FileInfo file)
        {
            if (file.Exists)
            {
                if (file.Extension.ToLower() == ".png" || file.Extension.ToLower() == ".jpg")
                {
                    fileInf = file;
                    PrintDocument pd = new PrintDocument();
                    pd.DocumentName = file.Name;
                    pd.PrintPage += new PrintPageEventHandler(PrintImageHandler);
                    pd.Print();
                }
            }
        }

        private static void PrintImageHandler(object sender, PrintPageEventArgs e)
        {
            
            MemoryStream ms = new MemoryStream();
            System.Windows.Media.Imaging.BmpBitmapEncoder bbe = new BmpBitmapEncoder();
            bbe.Frames.Add(BitmapFrame.Create(new Uri(fileInf.ToString(), UriKind.RelativeOrAbsolute)));
            bbe.Save(ms);
            System.Drawing.Image newImage = System.Drawing.Image.FromStream(ms);

            float marginX = 0;
            float marginY = 0;
            float oldPrinterW = e.PageBounds.Width;
            float oldPrinterH = e.PageBounds.Height;
            // MessageBox.Show(string.Format("W={0}, H={1}", oldPrinterW,oldPrinterH));
            float rateW = oldPrinterW / newImage.Width;
            float rateH = oldPrinterH / newImage.Height;
            int typeImage = 0;
            if (rateW < rateH)
            {
                typeImage = 1;
            }
            else
            {
                typeImage = 2;
            }

            float w = 0;
            float h = 0;
            switch (typeImage)
            {
                case 1:
                    w = newImage.Width * rateH;
                    h = oldPrinterH;
                    marginX = (oldPrinterW - w) / 2;
                    e.Graphics.DrawImage(newImage, marginX, marginY, w, h);
                    break;
                case 2:
                    w = oldPrinterW;
                    h = newImage.Height * rateW;
                    marginY = (oldPrinterH - h) / 2;
                    e.Graphics.DrawImage(newImage, marginX, marginY, w, h);
                    break;
            }
        }

        public static bool PrintImages(this FileInfo file, int id)
        {
            if (file.Exists)
            {
                string extensions= file.Extension.ToLower();
                if (extensions == ".png" || extensions == ".jpg" || extensions == ".bmp")
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.UriSource = new Uri(file.ToString(), UriKind.RelativeOrAbsolute);
                    bi.EndInit();
                    DrawingVisual vis = new DrawingVisual();
                    DrawingContext dc = vis.RenderOpen();
                    PrintQueue queue = new LocalPrintServer().GetPrintQueue(PrinterSettings.InstalledPrinters[id]);
                    PrintDialog dialog = new PrintDialog();
                    dialog.PrintQueue = queue;
                    dc.DrawImage(bi, new Rect { Width = dialog.PrintableAreaWidth, Height = dialog.PrintableAreaHeight});
                    dc.Close();
                    dialog.PrintVisual(vis, file.Name);
                    return true;
                }
            } 
            return false;
        }
       
        public static bool PrintImage(this FileInfo file, int id)
        {
            if (file.Exists)
            {
                string extensions= file.Extension.ToLower();
                if (extensions == ".png" || extensions == ".jpg" || extensions == ".bmp")
                {
                    System.Drawing.Bitmap bi;
                    using (Stream s = File.OpenRead(file.ToString()))
                    {
                        bi = new System.Drawing.Bitmap(s);
                    }
                    string name = PrinterSettings.InstalledPrinters[id];
                    PrintQueue queue = new LocalPrintServer().GetPrintQueue(name);
                    PrintDialog printDialog = new PrintDialog();
                    printDialog.PrintQueue = queue;

                    System.Printing.PrintCapabilities capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
                    double dpiScale = 300.0 / 96.0;
                    FixedDocument document = new FixedDocument();
                    document.DocumentPaginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

                    // break the bitmap down into pages
                    int pageBreak = 0;
                    int previousPageBreak = 0;
                    int pageHeight =
                        Convert.ToInt32(capabilities.PageImageableArea.ExtentHeight * dpiScale);
                    while (pageBreak < bi.Height - pageHeight)
                    {
                        pageBreak += pageHeight;  // Where we thing the end of the page should be

                        // Keep moving up a row until we find a good place to break the page
                        while (!IsRowGoodBreakingPoint(bi, pageBreak))
                            pageBreak--;

                        PageContent pageContent = generatePageContent(bi, previousPageBreak,
                          pageBreak, document.DocumentPaginator.PageSize.Width,
                          document.DocumentPaginator.PageSize.Height, capabilities);
                        document.Pages.Add(pageContent);
                        previousPageBreak = pageBreak;
                    }
                    // Last Page
                    PageContent lastPageContent = generatePageContent(bi, previousPageBreak,
                      bi.Height, document.DocumentPaginator.PageSize.Width,
                      document.DocumentPaginator.PageSize.Height, capabilities);
                    document.Pages.Add(lastPageContent);
                    printDialog.PrintDocument(document.DocumentPaginator, file.Name);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private static bool IsRowGoodBreakingPoint(System.Drawing.Bitmap bmp, int row)
        {
            double maxDeviationForEmptyLine = 1627500;
            bool goodBreakingPoint = false;
            try
            {
                if (rowPixelDeviation(bmp, row) < maxDeviationForEmptyLine)
                    goodBreakingPoint = true;

            }
            catch (Exception ex)
            {
                goodBreakingPoint = false;
            }
            return goodBreakingPoint;
        }
        private static double rowPixelDeviation(System.Drawing.Bitmap bmp, int row)
        {
            int count = 0;
            double total = 0;
            double totalVariance = 0;
            double standardDeviation = 0;
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, 
                   bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            int stride = bmpData.Stride;
            IntPtr firstPixelInImage = bmpData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)firstPixelInImage;
                p += stride * row;  // find starting pixel of the specified row
                for (int column = 0; column < bmp.Width; column++)
                {
                    count++;  
                    byte blue = p[0];
                    byte green = p[1];
                    byte red = p[3];

                    int pixelValue = System.Drawing.Color.FromArgb(0, red, green, blue).ToArgb();
                    total += pixelValue;
                    double average = total / count;
                    totalVariance += Math.Pow(pixelValue - average, 2);
                    standardDeviation = Math.Sqrt(totalVariance / count);
                    p += 3;
                }
            }
            bmp.UnlockBits(bmpData);

            return standardDeviation;
        }
        private static PageContent generatePageContent(System.Drawing.Bitmap bmp, int top,
         int bottom, double pageWidth, double PageHeight,
         System.Printing.PrintCapabilities capabilities)
        {
            FixedPage printDocumentPage = new FixedPage();
            printDocumentPage.Width = pageWidth;
            printDocumentPage.Height = PageHeight;

            int newImageHeight = bottom - top;
            System.Drawing.Bitmap bmpPage = bmp.Clone(new System.Drawing.Rectangle(0, top,
                   bmp.Width, newImageHeight), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            // Create a new bitmap for the contents of this page
            Image pageImage = new Image();
            BitmapSource bmpSource =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bmpPage.GetHbitmap(),
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, newImageHeight));

            pageImage.Source = bmpSource;
            pageImage.VerticalAlignment = VerticalAlignment.Top;

            // Place the bitmap on the page
            printDocumentPage.Children.Add(pageImage);

            PageContent pageContent = new PageContent();
            ((System.Windows.Markup.IAddChild)pageContent).AddChild(printDocumentPage);

            FixedPage.SetLeft(pageImage, capabilities.PageImageableArea.OriginWidth);
            FixedPage.SetTop(pageImage, capabilities.PageImageableArea.OriginHeight);

            pageImage.Width = capabilities.PageImageableArea.ExtentWidth;
            pageImage.Height = capabilities.PageImageableArea.ExtentHeight;
            return pageContent;
        }

    }
}
