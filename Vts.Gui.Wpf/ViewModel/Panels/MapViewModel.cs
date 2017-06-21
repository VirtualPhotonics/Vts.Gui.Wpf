using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using Vts;
using Vts.Extensions;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Input;
using Vts.Gui.Wpf.Model;
using Vts.Gui.Wpf.View;
using Vts.IO;

namespace Vts.Gui.Wpf.ViewModel
{
    public class MapViewModel : BindableObject
    {
        private int _mapViewId;
        private double _MinValue;
        private double _MaxValue;
        private bool _AutoScale;
        private Thickness _ZMax;

        private OptionViewModel<ColormapType> _ColormapTypeOptionVM;
        private OptionViewModel<ScalingType> _ScalingTypeOptionVM;

        private MapData _mapData;
        private Colormap _colormap;

        public MapViewModel(int MapViewId = 0)
        {
            _mapViewId = MapViewId;
            MinValue = 1E-9;
            MaxValue = 1.0;
            AutoScale = false;

            ScalingTypeOptionVM = new OptionViewModel<ScalingType>(StringLookup.GetLocalizedString("Label_ScalingType") + _mapViewId, false);
            ScalingTypeOptionVM.PropertyChanged += (sender, args) => UpdateImages();

            ColormapTypeOptionVM = new OptionViewModel<ColormapType>(StringLookup.GetLocalizedString("Label_ColormapType"));
            ColormapTypeOptionVM.PropertyChanged += (sender, args) =>
            {
                _colormap = new Colormap(ColormapTypeOptionVM.SelectedValue);
                UpdateImages();
            };

            _colormap = new Colormap(ColormapTypeOptionVM.SelectedValue);

            //Commands.Maps_PlotMap.Executed += Maps_PlotMap_Executed;
            PlotMap = new RelayCommand<object>(PlotMap_Executed);

            ExportDataToTextCommand = new RelayCommand(() => Maps_ExportDataToText_Executed(null, null));
            DuplicateWindowCommand = new RelayCommand(() => Map_DuplicateWindow_Executed(null, null));
        }

        public RelayCommand<object> PlotMap { get; set; }
        public RelayCommand DuplicateWindowCommand { get; set; }
        public RelayCommand ExportDataToTextCommand { get; set; }

        public WriteableBitmap Bitmap { get; private set; }
        public WriteableBitmap ColorBar { get; private set; }
        public double YExpectationValue { get; private set; }

        public string NegRhoMaxLabel { get { return StringLookup.GetLocalizedString("Label_NegRhoMax"); } }
        public string PosRhoMaxLabel { get { return StringLookup.GetLocalizedString("Label_PosRhoMax"); } }
        public string Z0Label { get { return StringLookup.GetLocalizedString("Label_Z0"); } }
        public string ZMaxLabel { get { return StringLookup.GetLocalizedString("Label_ZMax"); } }
        public string MeanDepthLabel { get { return StringLookup.GetLocalizedString("Label_MeanDepth"); } }
        public string MeasurementLabel { get { return StringLookup.GetLocalizedString("Measurement_mm"); } }
        public string PlotTypeLabel { get { return StringLookup.GetLocalizedString("Label_PlotType"); } }
        public string AutoScaleLabel { get { return StringLookup.GetLocalizedString("Label_AutoScale"); } }
        public string MinLabel { get { return StringLookup.GetLocalizedString("Label_Min"); } }
        public string MaxLabel { get { return StringLookup.GetLocalizedString("Label_Max"); } }
        public string ExportImageButtonLabel { get { return StringLookup.GetLocalizedString("Button_ExportImage"); } }
        public string ExportDataButtonLabel { get { return StringLookup.GetLocalizedString("Button_ExportData"); } }

        public MapViewModel Clone()
        {
            return Clone(this);
        }

        public static MapViewModel Clone(MapViewModel mapToClone)
        {
            var output = new MapViewModel(mapToClone._mapViewId + 1);
            mapToClone._mapViewId += 1;

            //Commands.Maps_PlotMap.Executed -= output.Maps_PlotMap_Executed;

            output._MinValue = mapToClone._MinValue;
            output._MaxValue = mapToClone._MaxValue;
            output._AutoScale = mapToClone._AutoScale;

            output._mapData = mapToClone._mapData; // need to clone
            output._colormap = mapToClone._colormap; // need to clone

            output.Bitmap = mapToClone.Bitmap != null ? new WriteableBitmap(mapToClone.Bitmap) : null;
            output.ColorBar = mapToClone.ColorBar != null ? new WriteableBitmap(mapToClone.ColorBar) : null;

            output.YExpectationValue = mapToClone.YExpectationValue;

            output._ColormapTypeOptionVM.Options[mapToClone._ColormapTypeOptionVM.SelectedValue].IsSelected = true;
            output._ScalingTypeOptionVM.Options[mapToClone._ScalingTypeOptionVM.SelectedValue].IsSelected = true;

            return output;
        }

        // todo: ready for updating to automatic properties and IAutoNotifyProperty changed
        public double MinValue
        {
            get { return _MinValue; }
            set
            {
                _MinValue = value;
                this.OnPropertyChanged("MinValue");
            }
        }

        public double MaxValue
        {
            get { return _MaxValue; }
            set
            {
                _MaxValue = value;
                this.OnPropertyChanged("MaxValue");
            }
        }

        public bool ManualScale
        {
            get { return !_AutoScale; }
            set
            {
                _AutoScale = !value;
                this.OnPropertyChanged("ManualScale");
                this.OnPropertyChanged("AutoScale");
            }
        }

        public bool AutoScale
        {
            get { return _AutoScale; }
            set
            {
                _AutoScale = value;
                if (!AutoScale)
                {
                    MinValue = 1E-9;
                    MaxValue = 1.0;
                }
                OnPropertyChanged("MinValue");
                OnPropertyChanged("MaxValue"); 
                OnPropertyChanged("AutoScale");
                OnPropertyChanged("ManualScale");
            }
        }

        public Thickness ZMax
        {
            get { return _ZMax; }
            set
            {
                _ZMax = value;
                OnPropertyChanged("ZMax");
            }
        }

        public OptionViewModel<ScalingType> ScalingTypeOptionVM
        {
            get { return _ScalingTypeOptionVM; }
            set
            {
                _ScalingTypeOptionVM = value;
                this.OnPropertyChanged("ScalingTypeOptionVM");
            }
        }

        public OptionViewModel<ColormapType> ColormapTypeOptionVM
        {
            get { return _ColormapTypeOptionVM; }
            set
            {
                _ColormapTypeOptionVM = value;
                this.OnPropertyChanged("ColormapTypeOptionVM");
            }
        }

        protected override void AfterPropertyChanged(string propertyName)
        {
            if (propertyName == "MinValue" || propertyName == "MaxValue")
            {
                UpdateImages();
            }
            else if (propertyName == "AutoScale")
            {
                UpdateStats();
                UpdateImages();
            }
        }

        private void PlotMap_Executed(object sender)
        {
            var mapData = sender as MapData;
            if (mapData != null)
            {
                SetBitmapData(mapData);
                UpdateImages(); // why is this called separately?
            }
        }

        private void Map_DuplicateWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = this.Clone();
            MainWindow.Current.Main_DuplicateMapView_Executed(vm);
            //Commands.Main_DuplicateMapView.Execute(vm, vm);
        }

        private void Maps_ExportDataToText_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_mapData != null && _mapData.RawData != null && _mapData.XValues != null && _mapData.YValues != null)
            {
                // Create SaveFileDialog 
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    DefaultExt = ".txt",
                    Filter = "Text Files (*.txt)|*.txt"
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
                using (var stream = new FileStream(filename, FileMode.Create))
                {
                    using (var sw = new StreamWriter(stream))
                    {
                        sw.Write("% X Values:\t");
                        _mapData.XValues.ForEach(x => sw.Write(x + "\t"));
                        sw.WriteLine();
                        sw.Write("% Y Values:\t");
                        _mapData.YValues.ForEach(y => sw.Write(y + "\t"));
                        sw.WriteLine();
                        sw.Write("% Map Values:\t");
                        _mapData.RawData.ForEach(val => sw.Write(val + "\t"));
                        sw.WriteLine();
                    }
                }
                //using (var stream = StreamFinder.GetLocalFilestreamFromSaveFileDialog("txt"))
                //{
                //    if (stream != null)
                //    {
                //        using (StreamWriter sw = new StreamWriter(stream))
                //        {
                //            sw.Write("% X Values:\t");
                //            _mapData.XValues.ForEach(x => sw.Write(x + "\t"));
                //            sw.WriteLine();
                //            sw.Write("% Y Values:\t");
                //            _mapData.YValues.ForEach(y => sw.Write(y + "\t"));
                //            sw.WriteLine();
                //            sw.Write("% Map Values:\t");
                //            _mapData.RawData.ForEach(val => sw.Write(val + "\t"));
                //            sw.WriteLine();
                //        }
                //    }
                //}
            }
        }

        public void SetBitmapData(MapData bitmapData)
        {
            _mapData = bitmapData;
            UpdateStats();
        }

        private void UpdateStats()
        {
            if (AutoScale)
            {
                MinValue = _mapData.Min; // this is slow, i know. will fix later. -dc
                MaxValue = _mapData.Max; // this is slow, i know. will fix later. -dc
            }
        }

        public void UpdateImages()
        {
            if (_mapData == null) return;

            int width = _mapData.Width;
            int height = _mapData.Height;

            ZMax = new Thickness(0, height, 0, 0);

            var bytesPerPixel = 4;
            var stride = width * bytesPerPixel;
            byte[] imageData = new byte[width * height * bytesPerPixel];
            switch (ScalingTypeOptionVM.SelectedValue)
            {
                case ScalingType.Linear:
                    for (int i = 0; i < _mapData.RawData.Length; i++)
                    {
                        var pixel = GetGrayscaleColor(_mapData.RawData[i], _MinValue, _MaxValue);
                        byte[] buffer = BitConverter.GetBytes(pixel);
                        imageData[i * 4 + 0] = buffer[0];
                        imageData[i * 4 + 1] = buffer[1];
                        imageData[i * 4 + 2] = buffer[2];
                        imageData[i * 4 + 3] = buffer[3];
                    }
                    break;
                case ScalingType.Log:
                default:
                    if (_MinValue <= 0.0) _MinValue = 10E-9;
                    if (_MaxValue <= 0.0) _MaxValue = 10E2;
                    for (int i = 0; i < _mapData.RawData.Length; i++)
                    {
                        if (_mapData.RawData[i] >= 0)
                        {
                            var pixel = GetGrayscaleColor(Math.Log10(_mapData.RawData[i]), Math.Log10(_MinValue), Math.Log10(_MaxValue));
                            byte[] buffer = BitConverter.GetBytes(pixel);
                            imageData[i * 4 + 0] = buffer[0];
                            imageData[i * 4 + 1] = buffer[1];
                            imageData[i * 4 + 2] = buffer[2];
                            imageData[i * 4 + 3] = buffer[3];
                        }
                        else // clamp to Log10(min) if the value goes negative
                        {
                            var pixel = GetGrayscaleColor(Math.Log10(_MinValue), Math.Log10(_MinValue), Math.Log10(_MaxValue));
                            byte[] buffer = BitConverter.GetBytes(pixel);
                            imageData[i * 4 + 0] = buffer[0];
                            imageData[i * 4 + 1] = buffer[1];
                            imageData[i * 4 + 2] = buffer[2];
                            imageData[i * 4 + 3] = buffer[3];
                        }
                    }
                    break;
            }
            var source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, imageData, stride);
            Bitmap = new WriteableBitmap(source);

            ColorBar = new WriteableBitmap(256, 1, 96, 96, PixelFormats.Bgra32, BitmapPalettes.WebPalette);
            byte[] colorMapData = new byte[256 * 1 * bytesPerPixel];
            stride = 256*bytesPerPixel;
            for (int i = 0; i < ColorBar.PixelWidth; i++)
            {
                var pixel = GetGrayscaleColor(i, 0, ColorBar.PixelWidth - 1);
                byte[] buffer = BitConverter.GetBytes(pixel);
                colorMapData[i * 4 + 0] = buffer[0];
                colorMapData[i * 4 + 1] = buffer[1];
                colorMapData[i * 4 + 2] = buffer[2];
                colorMapData[i * 4 + 3] = buffer[3];
            }
            source = BitmapSource.Create(256, 1, 96, 96, PixelFormats.Bgra32, null, colorMapData, stride);
            ColorBar = new WriteableBitmap(source);

            YExpectationValue = _mapData.YExpectationValue;

            this.OnPropertyChanged("Bitmap");
            this.OnPropertyChanged("ColorBar");
            this.OnPropertyChanged("YExpectationValue");

        }

        /// <summary>
        /// This will return an int that represents a color of a pixel. Each byte in an int represents 
        /// a different part of a color "vector". Any color is represented as {blue, green, red, alpha} 
        /// with {0, 0, 0, 255} being the deepest black
        /// </summary>
        /// <param name="input"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private int GetGrayscaleColor(double input, double min, double max)
        {
            if (input > max) input = max;
            if (input < min) input = min;

            double factor = max > min ? (input - min) / (max - min) : 0.0;

            int value = (int)(Math.Floor(factor * 255));

            a = 255;
            r = _colormap.RedByte[value];
            g = _colormap.GreenByte[value];
            b = _colormap.BlueByte[value];

            //r = (byte)value;
            //g = (byte)value;
            //b = (byte)value;

            return a << 24 | r << 16 | g << 8 | b;
        }
        private static byte a = 0, r = 0, g = 0, b = 0;

        /// <summary>
        /// An internal class that separates out the providing of sample (test) data.
        /// </summary>
        /// <remarks> Helps to separate desired behavior of above class from any data-specific concerns. </remarks>
        private static class SampleBitmapDataProvider
        {
            private static double _xPhase = 0.0;
            private static double _yPhase = Math.PI;

            public static MapData GetSampleBitmapData()
            {
                double[,] tempData = new double[600, 600];

                _xPhase -= 21 / 180.0 * Math.PI;
                _yPhase += 7 / 180.0 * Math.PI;

                int width = tempData.GetLength(0);
                int height = tempData.GetLength(1);
                for (int col = 0; col < height; col++)
                {
                    for (int row = 0; row < width; row++)
                    {
                        double x = .01 * col;
                        double y = .02 * row;

                        tempData[row, col] =
                            (Math.Sin(Math.Pow(_yPhase / Math.PI * x, 2) - Math.Pow(_xPhase / Math.PI * y, 2)) + _xPhase + _yPhase) *
                            Math.Cos(x + _xPhase) * Math.Sin(y + _yPhase);
                    }
                }

                return MapData.Create(tempData,
                    Enumerable.Range(0, width).Select(i => (double)i).ToArray(),
                    Enumerable.Range(0, height).Select(i => (double)i).ToArray(),
                    Enumerable.Range(0, width).Select(i => 1D).ToArray(),
                    Enumerable.Range(0, height).Select(i =>1D).ToArray());
            }
        }
    }
}
