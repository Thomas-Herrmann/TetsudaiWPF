using IronOcr;
using JishoNET.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Controls.PanAndZoom;

namespace TetsudaiWPF
{
    /// <summary>
    /// Interaction logic for Tetsudai.xaml
    /// </summary>
    public partial class Tetsudai : Window
    {
        private static readonly List<OcrLanguage> models = new List<OcrLanguage>{ OcrLanguage.JapaneseVerticalBest, OcrLanguage.JapaneseBest };
        private static readonly Regex whitespace = new Regex(@"\s+");
        private JishoClient client;
        private bool selectionHasChanged;

        public Tetsudai(CroppedBitmap screenshot)
        {
            client = new JishoClient();
            selectionHasChanged = false;

            InitializeComponent();

            screenshotPreview.Source = screenshot;
            TextViewer.Text = RunOCR(ChangePixelFormat(screenshot));
        }

        private string RunOCR(Bitmap managedBitmap)
        {
            var ocr = new IronTesseract();
            OcrResult bestResult;

            ocr.Configuration.TesseractVersion = TesseractVersion.Tesseract5;


            using (var input = new OcrInput())
            {
                input.AddImage(managedBitmap);

                bestResult = models.Select(model =>
                {
                    ocr.Language = model;

                    return ocr.Read(input);
                }).MaxBy(result => result.Confidence);
            }

            return bestResult.Text;
        }

        private Bitmap ChangePixelFormat(CroppedBitmap screenshot)
        {
            Bitmap bitmap;

            using (MemoryStream ms = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(screenshot));
                enc.Save(ms);
                bitmap = new Bitmap(ms);
            }

            Bitmap managedBitmap = new Bitmap((int)screenshot.Width, (int)screenshot.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            using (Graphics g = Graphics.FromImage(managedBitmap))
                g.DrawImage(bitmap, new System.Drawing.Rectangle(0, 0, managedBitmap.Width, managedBitmap.Height));

            bitmap.Dispose();

            return managedBitmap;
        }



        private void TextViewer_SelectionChanged(object sender, RoutedEventArgs e) => selectionHasChanged = true;

        private void TextViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => TextViewer.ReleaseMouseCapture();

        private async void TextViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!selectionHasChanged)
                return;

            var trimmed = whitespace.Replace(TextViewer.SelectedText, "");

            if (trimmed.Length == 0)
                return;

            Cursor = Cursors.Wait;
            ForceCursor = true;

            var lookup = await client.GetDefinitionAsync(trimmed);

            if (lookup.Success)
                foreach (var word in lookup.Data)
                {
                    var s = $"selection={trimmed}";

                    foreach (var reading in word.Japanese)
                        s += $" :: {reading.Reading}";

                    System.Diagnostics.Debug.WriteLine(s);
                }

            //wordViewer.ItemsSource = lookup.Data;

            Cursor = Cursors.Arrow;
            ForceCursor = false;
            selectionHasChanged = false;
            TextViewer.CaptureMouse();
        }
    }
}
