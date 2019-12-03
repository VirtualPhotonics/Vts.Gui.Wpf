using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using Vts.Extensions;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.Gui.Wpf.View;

namespace Vts.Gui.Wpf.ViewModel
{
    public class MapViewModel : BindableObject
    {
        private byte _a, _r, _g, _b;
        private bool _autoScale;
        private Colormap _colormap;

        private OptionViewModel<ColormapType> _colormapTypeOptionVm;

        private MapData _mapData;
        private int _mapViewId;
        private double _maxValue;
        private double _minValue;
        private OptionViewModel<ScalingType> _scalingTypeOptionVm;
        private Thickness _zMax;

        public MapViewModel(int MapViewId = 0)
        {
            _mapViewId = MapViewId;
            MinValue = 1E-9;
            MaxValue = 1.0;
            AutoScale = false;

            ScalingTypeOptionVm =
                new OptionViewModel<ScalingType>(StringLookup.GetLocalizedString("Label_ScalingType") + _mapViewId,
                    false);
            ScalingTypeOptionVm.PropertyChanged += (sender, args) => UpdateImages();

            ColormapTypeOptionVm =
                new OptionViewModel<ColormapType>(StringLookup.GetLocalizedString("Label_ColormapType"));
            ColormapTypeOptionVm.PropertyChanged += (sender, args) =>
            {
                _colormap = new Colormap(ColormapTypeOptionVm.SelectedValue);
                UpdateImages();
            };

            _colormap = new Colormap(ColormapTypeOptionVm.SelectedValue);

            PlotMap = new RelayCommand<object>(PlotMap_Executed);
            ClearMap = new RelayCommand<object>(ClearMap_Executed);

            ExportDataToTextCommand = new RelayCommand(() => Maps_ExportDataToText_Executed(null, null));
            DuplicateWindowCommand = new RelayCommand(() => Map_DuplicateWindow_Executed(null, null));
        }

        public RelayCommand<object> PlotMap { get; set; }
        public RelayCommand<object> ClearMap { get; set; }
        public RelayCommand DuplicateWindowCommand { get; set; }
        public RelayCommand ExportDataToTextCommand { get; set; }

        public WriteableBitmap Bitmap { get; private set; }
        public WriteableBitmap ColorBar { get; private set; }
        public double YExpectationValue { get; private set; }

        // todo: ready for updating to automatic properties and IAutoNotifyProperty changed
        public double MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                OnPropertyChanged("MinValue");
            }
        }

        public double MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                OnPropertyChanged("MaxValue");
            }
        }

        public bool ManualScale
        {
            get => !_autoScale;
            set
            {
                _autoScale = !value;
                OnPropertyChanged("ManualScale");
                OnPropertyChanged("AutoScale");
            }
        }

        public bool AutoScale
        {
            get => _autoScale;
            set
            {
                _autoScale = value;
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
            get => _zMax;
            set
            {
                _zMax = value;
                OnPropertyChanged("ZMax");
            }
        }

        public OptionViewModel<ScalingType> ScalingTypeOptionVm
        {
            get => _scalingTypeOptionVm;
            set
            {
                _scalingTypeOptionVm = value;
                OnPropertyChanged("ScalingTypeOptionVm");
            }
        }

        public OptionViewModel<ColormapType> ColormapTypeOptionVm
        {
            get => _colormapTypeOptionVm;
            set
            {
                _colormapTypeOptionVm = value;
                OnPropertyChanged("ColormapTypeOptionVm");
            }
        }

        public MapViewModel Clone()
        {
            return Clone(this);
        }

        public static MapViewModel Clone(MapViewModel mapToClone)
        {
            var output = new MapViewModel(mapToClone._mapViewId + 1);
            mapToClone._mapViewId += 1;

            output._minValue = mapToClone._minValue;
            output._maxValue = mapToClone._maxValue;
            output._autoScale = mapToClone._autoScale;

            output._mapData = mapToClone._mapData; // need to clone
            output._colormap = mapToClone._colormap; // need to clone

            output.Bitmap = mapToClone.Bitmap != null ? new WriteableBitmap(mapToClone.Bitmap) : null;
            output.ColorBar = mapToClone.ColorBar != null ? new WriteableBitmap(mapToClone.ColorBar) : null;

            output.YExpectationValue = mapToClone.YExpectationValue;

            output._colormapTypeOptionVm.Options[mapToClone._colormapTypeOptionVm.SelectedValue].IsSelected = true;
            output._scalingTypeOptionVm.Options[mapToClone._scalingTypeOptionVm.SelectedValue].IsSelected = true;

            return output;
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

        private void ClearMap_Executed(object sender)
        {
            if (Bitmap == null) return;
            Bitmap = null;
            OnPropertyChanged("Bitmap");
        }

        private void PlotMap_Executed(object sender)
        {
            if (sender is MapData mapData)
            {
                SetBitmapData(mapData);
                UpdateImages(); // why is this called separately?
            }
        }

        private void Map_DuplicateWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = Clone();
            MainWindow.Current.Main_DuplicateMapView_Executed(vm);
        }

        private void Maps_ExportDataToText_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_mapData != null && _mapData.RawData != null && _mapData.XValues != null && _mapData.YValues != null)
            {
                // Create SaveFileDialog 
                var dialog = new SaveFileDialog
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
                var stream = new FileStream(filename, FileMode.Create);
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

            var width = _mapData.Width;
            var height = _mapData.Height;

            ZMax = new Thickness(0, height, 0, 0);

            var bytesPerPixel = 4;
            var stride = width*bytesPerPixel;
            var imageData = new byte[width*height*bytesPerPixel];
            switch (ScalingTypeOptionVm.SelectedValue)
            {
                case ScalingType.Linear:
                    for (var i = 0; i < _mapData.RawData.Length; i++)
                    {
                        var pixel = GetGrayscaleColor(_mapData.RawData[i], _minValue, _maxValue);
                        var buffer = BitConverter.GetBytes(pixel);
                        imageData[i*4 + 0] = buffer[0];
                        imageData[i*4 + 1] = buffer[1];
                        imageData[i*4 + 2] = buffer[2];
                        imageData[i*4 + 3] = buffer[3];
                    }
                    break;
                default:
                    if (_minValue <= 0.0) _minValue = 10E-9;
                    if (_maxValue <= 0.0) _maxValue = 10E2;
                    for (var i = 0; i < _mapData.RawData.Length; i++)
                    {
                        if (_mapData.RawData[i] >= 0)
                        {
                            var pixel = GetGrayscaleColor(Math.Log10(_mapData.RawData[i]), Math.Log10(_minValue),
                                Math.Log10(_maxValue));
                            var buffer = BitConverter.GetBytes(pixel);
                            imageData[i*4 + 0] = buffer[0];
                            imageData[i*4 + 1] = buffer[1];
                            imageData[i*4 + 2] = buffer[2];
                            imageData[i*4 + 3] = buffer[3];
                        }
                        else // clamp to Log10(min) if the value goes negative
                        {
                            var pixel = GetGrayscaleColor(Math.Log10(_minValue), Math.Log10(_minValue),
                                Math.Log10(_maxValue));
                            var buffer = BitConverter.GetBytes(pixel);
                            imageData[i*4 + 0] = buffer[0];
                            imageData[i*4 + 1] = buffer[1];
                            imageData[i*4 + 2] = buffer[2];
                            imageData[i*4 + 3] = buffer[3];
                        }
                    }
                    break;
            }
            var source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, imageData, stride);
            Bitmap = new WriteableBitmap(source);

            ColorBar = new WriteableBitmap(256, 1, 96, 96, PixelFormats.Bgra32, BitmapPalettes.WebPalette);
            var colorMapData = new byte[256*1*bytesPerPixel];
            stride = 256*bytesPerPixel;
            for (var i = 0; i < ColorBar.PixelWidth; i++)
            {
                var pixel = GetGrayscaleColor(i, 0, ColorBar.PixelWidth - 1);
                var buffer = BitConverter.GetBytes(pixel);
                colorMapData[i*4 + 0] = buffer[0];
                colorMapData[i*4 + 1] = buffer[1];
                colorMapData[i*4 + 2] = buffer[2];
                colorMapData[i*4 + 3] = buffer[3];
            }
            source = BitmapSource.Create(256, 1, 96, 96, PixelFormats.Bgra32, null, colorMapData, stride);
            ColorBar = new WriteableBitmap(source);

            YExpectationValue = _mapData.YExpectationValue;

            OnPropertyChanged("Bitmap");
            OnPropertyChanged("ColorBar");
            OnPropertyChanged("YExpectationValue");
        }

        /// <summary>
        ///     This will return an int that represents a color of a pixel. Each byte in an int represents
        ///     a different part of a color "vector". Any color is represented as {blue, green, red, alpha}
        ///     with {0, 0, 0, 255} being the deepest black
        /// </summary>
        /// <param name="input"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private int GetGrayscaleColor(double input, double min, double max)
        {
            if (input > max) input = max;
            if (input < min) input = min;

            var factor = max > min ? (input - min)/(max - min) : 0.0;

            var value = (int) Math.Floor(factor*255);

            _a = 255;
            _r = _colormap.RedByte[value];
            _g = _colormap.GreenByte[value];
            _b = _colormap.BlueByte[value];

            return _a << 24 | _r << 16 | _g << 8 | _b;
        }

        /// <summary>
        ///     An internal class that separates out the providing of sample (test) data.
        /// </summary>
        /// <remarks> Helps to separate desired behavior of above class from any data-specific concerns. </remarks>
        private static class SampleBitmapDataProvider
        {
            private static double _xPhase;
            private static double _yPhase = Math.PI;

            public static MapData GetSampleBitmapData()
            {
                var tempData = new double[600, 600];

                _xPhase -= 21/180.0*Math.PI;
                _yPhase += 7/180.0*Math.PI;

                var width = tempData.GetLength(0);
                var height = tempData.GetLength(1);
                for (var col = 0; col < height; col++)
                {
                    for (var row = 0; row < width; row++)
                    {
                        var x = .01*col;
                        var y = .02*row;

                        tempData[row, col] =
                            (Math.Sin(Math.Pow(_yPhase/Math.PI*x, 2) - Math.Pow(_xPhase/Math.PI*y, 2)) + _xPhase +
                             _yPhase)*
                            Math.Cos(x + _xPhase)*Math.Sin(y + _yPhase);
                    }
                }

                return MapData.Create(tempData,
                    Enumerable.Range(0, width).Select(i => (double) i).ToArray(),
                    Enumerable.Range(0, height).Select(i => (double) i).ToArray(),
                    Enumerable.Range(0, width).Select(i => 1D).ToArray(),
                    Enumerable.Range(0, height).Select(i => 1D).ToArray());
            }
        }
    }
}