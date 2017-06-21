using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Vts.Gui.Wpf.ViewModel.Helpers
{
    /// <summary>
    ///     Contains static helper methods to generate images of UIElements
    /// </summary>
    public static class ImageTools
    {
        public static void SaveUIElementToPngImage(Visual element)
        {
            // Create SaveFileDialog 
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".png",
                Filter = "PNG Files (*.png)|*.png"
            };

            // Display OpenFileDialog by calling ShowDialog method 
            var result = dialog.ShowDialog();

            // if the file dialog returns true - file was selected 
            var filename = "";
            if (result == true)
            {
                // Get the filename from the dialog 
                filename = dialog.FileName;
            }
            if (filename == "") return;

            var width = (int) (double) element.GetValue(FrameworkElement.ActualWidthProperty);
            var height = (int) (double) element.GetValue(FrameworkElement.ActualHeightProperty);
            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var vRect = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = Brushes.White
            };
            vRect.Arrange(new Rect(0, 0, vRect.Width, vRect.Height));

            bitmap.Render(vRect);
            bitmap.Render(element);
            var png = new PngBitmapEncoder();

            png.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = File.Create(filename))
            {
                png.Save(stream);
            }
        }
    }
}