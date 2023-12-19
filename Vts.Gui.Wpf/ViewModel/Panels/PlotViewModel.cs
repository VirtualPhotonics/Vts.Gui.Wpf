using CommunityToolkit.Mvvm.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Vts.Extensions;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using FontWeights = OxyPlot.FontWeights;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Vts.Gui.Wpf.ViewModel
{
    public class DataPointCollection
    {
        public IDataPoint[] DataPoints { get; set; }
        public string Title { get; set; }
        public string ColorTag { get; set; }
    }

    public class PlotPointCollection : ObservableCollection<Point[]>
    {
        public PlotPointCollection(Point[][] points, IList<string> colorTags)
            : base(points)
        {
            ColorTags = colorTags;
        }

        public PlotPointCollection()
        {
            ColorTags = new List<string>();
        }

        public IList<string> ColorTags { get; set; }

        public void Add(Point[] item, string groupName)
        {
            ColorTags.Add(groupName);
            base.Add(item);
        }

        public new void Clear()
        {
            ColorTags.Clear();
            base.Clear();
        }

        public PlotPointCollection Clone()
        {
            return new PlotPointCollection(this.Select(points => points).ToArray(),
                ColorTags.Select(name => name).ToArray());
        }
    }

    /// <summary>
    ///     View model implementing Plot panel functionality
    /// </summary>
    public class PlotViewModel : BindableObject
    {
        private bool _autoScaleX;
        private bool _autoScaleY;
        private IndependentVariableAxis _currentIndependentVariableAxis;
        private string _customPlotLabel;

        private bool _clearPlot;
        private bool _hideKey;
        private bool _holdOn;
        private bool _isComplexPlot;
        private IList<string> _labels;
        private double _maxXValue;
        private double _maxYValue;
        private double _minXValue;
        private double _minYValue;
        private PlotModel _plotModel;
        private OptionViewModel<PlotNormalizationType> _plotNormalizationTypeOptionVm;
        private PlotPointCollection _plotSeriesCollection;
        private IList<string> _plotTitles;
        private OptionViewModel<PlotToggleType> _plotToggleTypeOptionVm;
        private ReflectancePlotType _plotType;
        // change from Point to our own custom class so we can bind to color, style, etc, too

        private int _plotViewId;
        private bool _showAxes;
        private bool _showComplexPlotToggle;
        private bool _showInPlotView;
        private string _title;
        private OptionViewModel<ScalingType> _xAxisSpacingOptionVm;
        private OptionViewModel<ScalingType> _yAxisSpacingOptionVm;

        public PlotViewModel(int plotViewId = 0)
        {
            _plotViewId = plotViewId;
            _minYValue = 1E-9;
            _maxYValue = 1.0;
            _minXValue = 1E-9;
            _maxXValue = 1.0;
            _autoScaleX = true;
            _autoScaleY = true;

            RealLabels = new List<string>();
            ImagLabels = new List<string>();
            PhaseLabels = new List<string>();
            AmplitudeLabels = new List<string>();
            
            Labels = new List<string>();
            PlotTitles = new List<string>();
            DataSeriesCollection = new List<DataPointCollection>(); // raw data
            PlotSeriesCollection = new PlotPointCollection(); // raw data manipulated by the user toggles

            PlotModel = new PlotModel
            {
                Title = "",
                DefaultColors = new List<OxyColor>
                {
                    OxyColor.FromRgb(0x00, 0x80, 0x00),     // Green
                    OxyColor.FromRgb(0xD6, 0x89, 0x10),     // Dark Orange
                    OxyColor.FromRgb(0xDC, 0x14, 0x3C),     // Crimson Red
                    OxyColor.FromRgb(0x00, 0x00, 0xFF),     // Blue
                    OxyColor.FromRgb(0xC4, 0x15, 0xC4),     // Dark Magenta
                    OxyColor.FromRgb(0x00, 0xBF, 0xBF),     // Turquoise
                    OxyColor.FromRgb(0x4F, 0x4F, 0x4F),     // Dark Grey
                    OxyColor.FromRgb(0x33, 0x99, 0xFF),     // Light Blue
                    OxyColor.FromRgb(0x80, 0x00, 0x00),     // Maroon
                    OxyColor.FromRgb(0x00, 0x80, 0x80),     // Teal
                    OxyColor.FromRgb(0x00, 0x00, 0x80),     // Navy Blue
                    OxyColor.FromRgb(0x99, 0x99, 0x00),     // Olive Green
                }
            };
            var legend = new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                IsLegendVisible = true
            };
            PlotModel.Legends.Add(legend);
            PlotType = ReflectancePlotType.ForwardSolver;
            _holdOn = true;
            _hideKey = false;
            _showInPlotView = true;
            _showAxes = false;
            _showComplexPlotToggle = false;

            XAxisSpacingOptionVm = new OptionViewModel<ScalingType>("XAxisSpacing_" + _plotViewId, false);
            XAxisSpacingOptionVm.PropertyChanged += (sender, args) => UpdatePlotSeries();

            YAxisSpacingOptionVm = new OptionViewModel<ScalingType>("YAxisSpacing_" + _plotViewId, false);
            YAxisSpacingOptionVm.PropertyChanged += (sender, args) => UpdatePlotSeries();

            PlotToggleTypeOptionVm = new OptionViewModel<PlotToggleType>("ToggleType_" + _plotViewId, false);
            PlotToggleTypeOptionVm.PropertyChanged += (sender, args) => UpdatePlotSeries();

            PlotNormalizationTypeOptionVm =
                new OptionViewModel<PlotNormalizationType>("NormalizationType_" + _plotViewId, false);
            PlotNormalizationTypeOptionVm.PropertyChanged += (sender, args) => UpdatePlotSeries();

            CustomPlotLabel = "";

            PlotValues = new RelayCommand<Array>(Plot_Executed);
            SetAxesLabels = new RelayCommand<object>(Plot_SetAxesLabels_Executed);
            ClearPlotCommand = new RelayCommand(() => Plot_Cleared(null, null));
            ClearPlotSingleCommand = new RelayCommand(() => Plot_ClearedSingle(null, null));
            ExportDataToTextCommand = new RelayCommand(() => Plot_ExportDataToText_Executed(null, null));
            DuplicateWindowCommand = new RelayCommand(() => Plot_DuplicateWindow_Executed(null, null));
        }

        public RelayCommand<Array> PlotValues { get; set; }
        public RelayCommand<object> SetAxesLabels { get; set; }
        public RelayCommand ClearPlotCommand { get; set; }
        public RelayCommand ClearPlotSingleCommand { get; set; }
        public RelayCommand ExportDataToTextCommand { get; set; }
        public RelayCommand DuplicateWindowCommand { get; set; }

        
        private string XAxis { get; set; }
        private string YAxis { get; set; }

        private List<DataPointCollection> DataSeriesCollection { get; set; }
        private IList<string> RealLabels { get; set; }
        private IList<string> ImagLabels { get; set; }
        private IList<string> PhaseLabels { get; set; }
        private IList<string> AmplitudeLabels { get; set; }

        public PlotModel PlotModel
        {
            get => _plotModel;
            set
            {
                _plotModel = value;
                OnPropertyChanged("PlotModel");
            }
        }

        public PlotPointCollection PlotSeriesCollection
        {
            get => _plotSeriesCollection;
            set
            {
                _plotSeriesCollection = value;
                OnPropertyChanged("PlotSeriesCollection");
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public ReflectancePlotType PlotType
        {
            get => _plotType;
            set
            {
                _plotType = value;
                OnPropertyChanged("PlotType");
            }
        }

        public bool HoldOn
        {
            get => _holdOn;
            set
            {
                _holdOn = value;
                OnPropertyChanged("HoldOn");
            }
        }

        public bool HideKey
        {
            get => _hideKey;
            set
            {
                _hideKey = value;
                OnPropertyChanged("HideKey");
                UpdatePlotSeries();
            }
        }

        public bool ShowInPlotView
        {
            get => _showInPlotView;
            set
            {
                _showInPlotView = value;
                OnPropertyChanged("ShowInPlotView");
            }
        }

        public bool ShowAxes
        {
            get => _showAxes;
            set
            {
                _showAxes = value;
                OnPropertyChanged("ShowAxes");
            }
        }

        public bool ShowComplexPlotToggle
        {
            get => _showComplexPlotToggle;
            set
            {
                _showComplexPlotToggle = value;
                OnPropertyChanged("ShowComplexPlotToggle");
            }
        }

        public OptionViewModel<ScalingType> XAxisSpacingOptionVm
        {
            get => _xAxisSpacingOptionVm;
            set
            {
                _xAxisSpacingOptionVm = value;
                OnPropertyChanged("XAxisSpacingOptionVm");
            }
        }

        public OptionViewModel<ScalingType> YAxisSpacingOptionVm
        {
            get => _yAxisSpacingOptionVm;
            set
            {
                _yAxisSpacingOptionVm = value;
                OnPropertyChanged("YAxisSpacingOptionVm");
            }
        }

        public OptionViewModel<PlotToggleType> PlotToggleTypeOptionVm
        {
            get => _plotToggleTypeOptionVm;
            set
            {
                _plotToggleTypeOptionVm = value;
                OnPropertyChanged("PlotToggleTypeOptionVm");
            }
        }

        public IndependentVariableAxis CurrentIndependentVariableAxis
        {
            get => _currentIndependentVariableAxis;
            set
            {
                // if user switches independent variable, clear plot
                if (_clearPlot && ShowInPlotView)
                {
                    ClearPlot();
                    WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(
                        StringLookup.GetLocalizedString("Message_PlotViewCleared") + "\r");
                }
                _currentIndependentVariableAxis = value;
                OnPropertyChanged("CurrentIndependentVariableAxis");
            }
        }

        public OptionViewModel<PlotNormalizationType> PlotNormalizationTypeOptionVm
        {
            get => _plotNormalizationTypeOptionVm;
            set
            {
                _plotNormalizationTypeOptionVm = value;
                OnPropertyChanged("PlotNormalizationTypeOptionVm");
            }
        }

        public string CustomPlotLabel
        {
            get => _customPlotLabel;
            set
            {
                _customPlotLabel = value;
                OnPropertyChanged("CustomPlotLabel");
            }
        }

        public IList<string> Labels
        {
            get => _labels;
            set
            {
                _labels = value;
                OnPropertyChanged("Labels");
            }
        }

        public IList<string> PlotTitles
        {
            get => _plotTitles;
            set
            {
                _plotTitles = value;
                OnPropertyChanged("PlotTitles");
            }
        }

        public bool AutoScaleX
        {
            get => _autoScaleX;
            set
            {
                _autoScaleX = value;
                OnPropertyChanged("AutoScaleX");
                OnPropertyChanged("ManualScaleX");
            }
        }

        public bool AutoScaleY
        {
            get => _autoScaleY;
            set
            {
                _autoScaleY = value;
                OnPropertyChanged("AutoScaleY");
                OnPropertyChanged("ManualScaleY");
            }
        }

        public bool ManualScaleX
        {
            get => !_autoScaleX;
            set
            {
                _autoScaleX = !value;
                OnPropertyChanged("ManualScaleX");
                OnPropertyChanged("AutoScaleX");
            }
        }

        public bool ManualScaleY
        {
            get => !_autoScaleY;
            set
            {
                _autoScaleY = !value;
                OnPropertyChanged("ManualScaleY");
                OnPropertyChanged("AutoScaleY");
            }
        }

        public double MinXValue
        {
            get => _minXValue;
            set
            {
                _minXValue = value;
                OnPropertyChanged("MinXValue");
            }
        }

        public double MaxXValue
        {
            get => _maxXValue;
            set
            {
                _maxXValue = value;
                OnPropertyChanged("MaxXValue");
            }
        }

        public double MinYValue
        {
            get => _minYValue;
            set
            {
                _minYValue = value;
                OnPropertyChanged("MinYValue");
            }
        }

        public double MaxYValue
        {
            get => _maxYValue;
            set
            {
                _maxYValue = value;
                OnPropertyChanged("MaxYValue");
            }
        }

        private void Plot_DuplicateWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var plotViewModel = Clone();
            plotViewModel.UpdatePlotSeries();
            MainWindow.Current.Main_DuplicatePlotView_Executed(plotViewModel);
        }

        public PlotViewModel Clone()
        {
            return Clone(this);
        }

        public static PlotViewModel Clone(PlotViewModel plotToClone)
        {
            var output = new PlotViewModel(plotToClone._plotViewId + 1);
            plotToClone._plotViewId += 1;

            output.Title = plotToClone.Title;
            output.PlotTitles = plotToClone.PlotTitles.ToList();
            output.PlotType = plotToClone.PlotType;
            output.HoldOn = plotToClone.HoldOn;
            output.PlotSeriesCollection = new PlotPointCollection();
            output.Labels = plotToClone._labels.ToList();
            output.CustomPlotLabel = plotToClone.CustomPlotLabel;
            output.ShowInPlotView = false;
            output.ShowAxes = plotToClone.ShowAxes;
            output.MinYValue = plotToClone.MinYValue;
            output.MaxYValue = plotToClone.MaxYValue;
            output.MinXValue = plotToClone.MinXValue;
            output.MaxXValue = plotToClone.MaxXValue;
            output.AutoScaleX = plotToClone.AutoScaleX;
            output.AutoScaleY = plotToClone.AutoScaleY;
            output._isComplexPlot = plotToClone._isComplexPlot;
            output.CurrentIndependentVariableAxis = plotToClone.CurrentIndependentVariableAxis;

            output.RealLabels = plotToClone.RealLabels;
            output.ImagLabels = plotToClone.ImagLabels;
            output.PhaseLabels = plotToClone.PhaseLabels;
            output.AmplitudeLabels = plotToClone.AmplitudeLabels;

            output.YAxisSpacingOptionVm.Options[output._yAxisSpacingOptionVm.SelectedValue].IsSelected = false;
            output.PlotNormalizationTypeOptionVm.Options[output._plotNormalizationTypeOptionVm.SelectedValue]
                .IsSelected = false;
            output.PlotToggleTypeOptionVm.Options[output._plotToggleTypeOptionVm.SelectedValue].IsSelected = false;
            output.XAxisSpacingOptionVm.Options[output._xAxisSpacingOptionVm.SelectedValue].IsSelected = false;
            output.YAxisSpacingOptionVm.Options[plotToClone._yAxisSpacingOptionVm.SelectedValue].IsSelected = true;
            output.PlotNormalizationTypeOptionVm.Options[plotToClone._plotNormalizationTypeOptionVm.SelectedValue]
                .IsSelected = true;
            output.PlotToggleTypeOptionVm.Options[plotToClone._plotToggleTypeOptionVm.SelectedValue].IsSelected = true;
            output.XAxisSpacingOptionVm.Options[plotToClone._xAxisSpacingOptionVm.SelectedValue].IsSelected = true;

            output.DataSeriesCollection =
                plotToClone.DataSeriesCollection.Select(
                    ds =>
                        new DataPointCollection
                        {
                            DataPoints = ds.DataPoints.Select(val => val).ToArray(),
                            ColorTag = ds.ColorTag,
                            Title = ds.Title
                        }).ToList();
            // add a property at the end that will call UpdatePlotSeries so the plot data is in the clone
            output.HideKey = plotToClone.HideKey;
            return output;
        }

        protected override void AfterPropertyChanged(string propertyName)
        {
            if ((!AutoScaleX && (propertyName == "MinXValue" || propertyName == "MaxXValue")) ||
                (!AutoScaleY && (propertyName == "MinYValue" || propertyName == "MaxYValue")) ||
                propertyName == "AutoScaleX" ||
                propertyName == "AutoScaleY")
            {
                UpdatePlotSeries();
            }
        }

        private void Plot_SetAxesLabels_Executed(object sender)
        {
            if (sender is PlotAxesLabels labels)
            {
                _clearPlot = false;
                // set CurrentIndependtVariableAxis prior to setting Title because property
                // might ClearPlot including Title
                if (CurrentIndependentVariableAxis != labels.IndependentAxis.AxisType)
                {
                    _clearPlot = true;
                }

                if (_isComplexPlot != labels.IsComplexPlot)
                {
                    _clearPlot = true;
                    _isComplexPlot = labels.IsComplexPlot;
                }
                CurrentIndependentVariableAxis = labels.IndependentAxis.AxisType;
                // set the x and y axis labels
                XAxis = labels.IndependentAxis.AxisLabel + " [" + labels.IndependentAxis.AxisUnits + "]";
                YAxis = labels.DependentAxisName + " [" + labels.DependentAxisUnits + "]";

                Title = labels.DependentAxisName + " [" + labels.DependentAxisUnits + "] " + StringLookup.GetLocalizedString("Label_Versus") + " " +
                        labels.IndependentAxis.AxisLabel + " [" + labels.IndependentAxis.AxisUnits + "]";

                if (labels.ConstantAxes.Length > 0)
                {
                    Title += " at " + labels.ConstantAxes[0].AxisLabel + " = " + labels.ConstantAxes[0].AxisValue + " " +
                             labels.ConstantAxes[0].AxisUnits;
                }
                if (labels.ConstantAxes.Length > 1)
                {
                    Title += " and " + labels.ConstantAxes[1].AxisLabel + " = " + labels.ConstantAxes[1].AxisValue + " " +
                             labels.ConstantAxes[1].AxisUnits;
                }
            }
        }

        /// <summary>
        ///     Writes tab-delimited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Plot_ExportDataToText_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_labels == null || _labels.Count <= 0 || DataSeriesCollection == null ||
                DataSeriesCollection.Count <= 0) return;

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
            using var sw = new StreamWriter(stream);
            sw.Write("%");

            if (DataSeriesCollection[0].DataPoints[0] is DoubleDataPoint)
            {
                for (var i = 0; i < _labels.Count; i++)
                {
                    sw.Write(PlotTitles[i]);
                    sw.WriteLine(_labels[i] + " (X)" + "\t" + _labels[i] + " (Y)" + "\t\n");
                }
                //_labels.ForEach(label => sw.WriteLine(label + " (X)" + "\t" + " (Y)" + "\t"));
            }
            else // ComplexDataPoint
            {
                // output plot titles and labels together
                for (var i = 0; i < _labels.Count; i++)
                {
                    sw.Write(PlotTitles[i]);
                    sw.WriteLine(_labels[i] + " (X)" + "\t" + _labels[i] + " (Real)" + "\t" + " (Imag)" + "\t\n");
                }
                //_labels.ForEach(label => sw.WriteLine(label + " (X)" + "\t" + label + " (Real)" + "\t" + " (Imag)" + "\t"));
            }
            // the following assumes that the data plotted is all doubles or all Complex 
            sw.WriteLine();
            foreach (var dataSet in DataSeriesCollection)
            {
                sw.WriteLine(); 
                if (DataSeriesCollection[0].DataPoints[0] is DoubleDataPoint) // doubles
                {
                    foreach (var t in dataSet.DataPoints.Cast<DoubleDataPoint>().ToArray())
                    {
                        sw.WriteLine(t.X + "\t" + t.Y + "\t");
                    }
                }
                else  // Complex
                { 
                    foreach (var t in dataSet.DataPoints.Cast<ComplexDataPoint>().ToArray())
                    {
                        sw.WriteLine(t.X + "\t" + t.Y.Real + "\t" + t.Y.Imaginary + "\t");
                    }
                }
            }
        }

        private void Plot_Cleared(object sender, ExecutedRoutedEventArgs e)
        {
            ClearPlot();
            UpdatePlotSeries();
        }

        private void Plot_ClearedSingle(object sender, ExecutedRoutedEventArgs e)
        {
            ClearPlotSingle();
            UpdatePlotSeries();
        }

        private void Plot_Executed(Array arr)
        {
            if (arr is PlotData[] data)
            {
                AddValuesToPlotData(data);
            }
        }

        private void AddValuesToPlotData(PlotData[] plotData)
        {
            if (!_holdOn)
            {
                ClearPlot();
            }

            var customLabel = CustomPlotLabel.Length > 0 ? "[" + CustomPlotLabel + "] " : "";
            foreach (var t in plotData)
            {
                var points = t.Points;
                var title = customLabel + t.Title;

                Labels.Add(title + customLabel);
                DataSeriesCollection.Add(new DataPointCollection
                {
                    DataPoints = points,
                    Title = title,
                    ColorTag = "ColorTag"
                });
                PlotTitles.Add(Title);
            }
            UpdatePlotSeries();
        }

        private void ClearPlot()
        {
            DataSeriesCollection.Clear();
            PlotSeriesCollection.Clear();
            Labels.Clear();
            PlotTitles.Clear();
            PlotModel.Title = "";
            foreach (var axis in PlotModel.Axes)
            {
                axis.Reset();
            }
        }

        private void ClearPlotSingle()
        {
            if (DataSeriesCollection.Any())
            {
                DataSeriesCollection.RemoveAt(DataSeriesCollection.Count - 1);
                //Clear the PlotSeriesCollection, it will be recreated with the plot
                PlotSeriesCollection.Clear();
                Labels.RemoveAt(Labels.Count - 1);
                PlotTitles.RemoveAt(PlotTitles.Count - 1);
                PlotModel.Title = "";
            }
        }

        private void CalculateMinMax()
        {
            // get min and max values for reference
            if (!AutoScaleX && !AutoScaleY) return;
            var minX = double.PositiveInfinity;
            var maxX = double.NegativeInfinity;
            var minY = double.PositiveInfinity;
            var maxY = double.NegativeInfinity;
            foreach (var point in PlotModel.Series.Cast<LineSeries>().SelectMany(series => series.Points))
            {
                if (AutoScaleX)
                {
                    if (point.X > maxX)
                    {
                        maxX = point.X;
                    }
                    if (point.X < minX)
                    {
                        minX = point.X;
                    }
                }
                if (!AutoScaleY) continue;
                if (point.Y > maxY)
                {
                    maxY = point.Y;
                }
                if (point.Y < minY)
                {
                    minY = point.Y;
                }
            }
            if (AutoScaleX)
            {
                MinXValue = minX;
                MaxXValue = maxX;
            }
            if (!AutoScaleY) return;
            MinYValue = minY;
            MaxYValue = maxY;
        }

        private void ConstuctPlot(DataPointCollection dataPointCollection)
        {
            // function to filter the results if we're not auto-scaling
            Func<DataPoint, bool> isWithinAxes =
                p =>
                    (AutoScaleX || (p.X <= MaxXValue && p.X >= MinXValue)) &&
                    (AutoScaleY || (p.Y <= MaxYValue && p.Y >= MinYValue));

            // function to filter out any invalid data points
            Func<DataPoint, bool> isValidDataPoint =
                p =>
                    !double.IsInfinity(Math.Abs(p.X)) && !double.IsNaN(p.X) && !double.IsInfinity(Math.Abs(p.Y)) &&
                    !double.IsNaN(p.Y);

            //check if any normalization is selected 
            var normToCurve = PlotNormalizationTypeOptionVm.SelectedValue == PlotNormalizationType.RelativeToCurve &&
                              DataSeriesCollection.Count > 1;
            var normToMax = PlotNormalizationTypeOptionVm.SelectedValue == PlotNormalizationType.RelativeToMax &&
                            DataSeriesCollection.Count > 0;

            var tempPointArrayA = new List<Point>();
            var tempPointArrayB = new List<Point>();

            double x;
            double y;
            var lineSeriesA = new LineSeries();
            var lineSeriesB = new LineSeries(); //we need B for complex
            if (dataPointCollection.DataPoints[0] is ComplexDataPoint)
            {
                _isComplexPlot = true;
                // normalization calculations
                var max = 1.0;
                var maxRe = 1.0;
                var maxIm = 1.0;
                if (normToMax)
                {
                    var points = dataPointCollection.DataPoints.Cast<ComplexDataPoint>().ToArray();
                    switch (PlotToggleTypeOptionVm.SelectedValue)
                    {
                        case PlotToggleType.Phase:
                            max = points.Select(p => p.Y.Phase * (-180/Math.PI)).Max();
                            break;
                        case PlotToggleType.Amp:
                            max = points.Select(p => p.Y.Magnitude).Max();
                            break;
                        case PlotToggleType.Complex:
                            maxRe = points.Select(p => p.Y.Real).Max();
                            maxIm = points.Select(p => p.Y.Imaginary).Max();
                            break;
                    }
                }
                
                double[] tempAmp = null;
                double[] tempPh = null;
                double[] tempRe = null;
                double[] tempIm = null;
                if (normToCurve)
                {
                    tempAmp = (from ComplexDataPoint dp in DataSeriesCollection[0].DataPoints
                        select dp.Y.Magnitude).ToArray();
                    tempPh = (from ComplexDataPoint dp in DataSeriesCollection[0].DataPoints
                        select dp.Y.Phase*(-180/Math.PI)).ToArray();
                    tempRe = (from ComplexDataPoint dp in DataSeriesCollection[0].DataPoints
                        select dp.Y.Real).ToArray();
                    tempIm = (from ComplexDataPoint dp in DataSeriesCollection[0].DataPoints
                        select dp.Y.Imaginary).ToArray();
                }

                var curveIndex = 0;
                foreach (var dp in dataPointCollection.DataPoints.Cast<ComplexDataPoint>())
                {
                    x = XAxisSpacingOptionVm.SelectedValue == ScalingType.Log ? Math.Log10(dp.X) : dp.X;
                    switch (PlotToggleTypeOptionVm.SelectedValue)
                    {
                        case PlotToggleType.Phase:
                            y = -(dp.Y.Phase*(180/Math.PI));
                            // force phase to be between 0 and 360
                            if (y < 0)
                            {
                                y += 360;
                            }
                            switch (PlotNormalizationTypeOptionVm.SelectedValue)
                            {
                                case PlotNormalizationType.RelativeToCurve:
                                    var curveY = normToCurve && tempPh != null ? tempPh[curveIndex] : 1.0;
                                    y = y / curveY;
                                    break;
                                case PlotNormalizationType.RelativeToMax:
                                    y = y / max;
                                    break;
                            }
                            break;
                        case PlotToggleType.Amp:
                            y = dp.Y.Magnitude;
                            switch (PlotNormalizationTypeOptionVm.SelectedValue)
                            {
                                case PlotNormalizationType.RelativeToCurve:
                                    var curveY = normToCurve && tempAmp != null ? tempAmp[curveIndex] : 1.0;
                                    y = y / curveY;
                                    break;
                                case PlotNormalizationType.RelativeToMax:
                                    y = y / max;
                                    break;
                            }
                            break;
                        default: // case PlotToggleType.Complex:
                            y = dp.Y.Real;
                            switch (PlotNormalizationTypeOptionVm.SelectedValue)
                            {
                                case PlotNormalizationType.RelativeToCurve:
                                    var curveY = normToCurve && tempRe != null ? tempRe[curveIndex] : 1.0;                                   
                                    y = y/curveY;
                                    break;
                                case PlotNormalizationType.RelativeToMax:
                                    max = maxRe;
                                    y = y/max;
                                    break;
                            }
                            y = YAxisSpacingOptionVm.SelectedValue == ScalingType.Log ? Math.Log10(y) : y;
                            var p = new DataPoint(x, y);
                            if (isValidDataPoint(p) && isWithinAxes(p))
                            {
                                lineSeriesB.Points.Add(p);
                                //Add the data to the tempPointArray to add to the PlotSeriesCollection
                                tempPointArrayB.Add(new Point(x, y));
                            }
                            y = dp.Y.Imaginary;
                            //break; // handle imag within switch
                            switch (PlotNormalizationTypeOptionVm.SelectedValue)
                            {
                                case PlotNormalizationType.RelativeToCurve:
                                    var curveY = normToCurve && tempIm != null ? tempIm[curveIndex] : 1.0;
                                    y = y / curveY;
                                    break;
                                case PlotNormalizationType.RelativeToMax:
                                    max = maxIm;
                                    y = y / max;
                                    break;
                            }
                            break;
                    }
                    // ckh 8/13/18 code does not need to repeat here since inside switch above now for all cases
                    //switch (PlotNormalizationTypeOptionVM.SelectedValue)
                    //{
                    //    case PlotNormalizationType.RelativeToCurve:
                    //        var curveY = normToCurve && tempAmp != null ? tempAmp[curveIndex] : 1.0;
                    //        y = y/curveY;
                    //        break;
                    //    case PlotNormalizationType.RelativeToMax:
                    //        y = y/max;
                    //        break;
                    //}
                    y = YAxisSpacingOptionVm.SelectedValue == ScalingType.Log ? Math.Log10(y) : y;
                    var point = new DataPoint(x, y);
                    if (isValidDataPoint(point) && isWithinAxes(point))
                    {
                        lineSeriesA.Points.Add(point);
                        //Add the data to the tempPointArray to add to the PlotSeriesCollection
                        tempPointArrayA.Add(new Point(x, y));
                    }
                    curveIndex += 1;
                }

                ShowComplexPlotToggle = true; // right now, it's all or nothing - assume all plots are ComplexDataPoints
            }
            else
            {
                // normalization calculations
                var max = 1.0;
                if (normToMax)
                {
                    var points = dataPointCollection.DataPoints.Cast<DoubleDataPoint>().ToArray();
                    max = points.Select(p => p.Y).Max();
                }
                double[] tempY = null;
                if (normToCurve)
                {
                    tempY = (from DoubleDataPoint dp in DataSeriesCollection[0].DataPoints select dp.Y).ToArray();
                }

                var curveIndex = 0;
                foreach (var dp in dataPointCollection.DataPoints.Cast<DoubleDataPoint>())
                {
                    x = XAxisSpacingOptionVm.SelectedValue == ScalingType.Log ? Math.Log10(dp.X) : dp.X;
                    switch (PlotNormalizationTypeOptionVm.SelectedValue)
                    {
                        case PlotNormalizationType.RelativeToCurve:
                            var curveY = normToCurve && tempY != null ? tempY[curveIndex] : 1.0;
                            y = dp.Y/curveY;
                            break;
                        case PlotNormalizationType.RelativeToMax:
                            y = dp.Y/max;
                            break;
                        default:
                            y = dp.Y;
                            break;
                    }
                    y = YAxisSpacingOptionVm.SelectedValue == ScalingType.Log ? Math.Log10(y) : y;
                    var point = new DataPoint(x, y);
                    if (isValidDataPoint(point) && isWithinAxes(point))
                    {
                        lineSeriesA.Points.Add(point);
                        //Add the data to the tempPointArray to add to the PlotSeriesCollection
                        tempPointArrayA.Add(new Point(x, y));
                    }
                    curveIndex += 1;
                }
            }
            if (ShowComplexPlotToggle)
            {
                switch (PlotToggleTypeOptionVm.SelectedValue)
                {
                    case PlotToggleType.Complex:
                        lineSeriesA.Title = dataPointCollection.Title + StringLookup.GetLocalizedString("Label_Imaginary");
                        lineSeriesB.Title = dataPointCollection.Title + StringLookup.GetLocalizedString("Label_Real");
                        lineSeriesB.MarkerType = MarkerType.Circle;
                        PlotModel.Series.Add(lineSeriesB);
                        PlotSeriesCollection.Add(tempPointArrayB.ToArray());
                        break;
                    case PlotToggleType.Phase:
                        lineSeriesA.Title = dataPointCollection.Title + StringLookup.GetLocalizedString("Label_Phase");
                        break;
                    case PlotToggleType.Amp:
                        lineSeriesA.Title = dataPointCollection.Title + StringLookup.GetLocalizedString("Label_Amplitude");
                        break;
                }
                lineSeriesA.MarkerType = MarkerType.Circle;
                PlotModel.Series.Add(lineSeriesA);
                PlotModel.Title = PlotTitles[PlotTitles.Count - 1];
                PlotSeriesCollection.Add(tempPointArrayA.ToArray());
            }
            else
            {
                lineSeriesA.Title = dataPointCollection.Title;
                lineSeriesA.MarkerType = MarkerType.Circle;
                PlotModel.Series.Add(lineSeriesA);
                PlotModel.Title = PlotTitles[PlotTitles.Count - 1];
                PlotSeriesCollection.Add(tempPointArrayA.ToArray());
            }
            PlotModel.Axes.Clear();
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = XAxis, TitleFontWeight = FontWeights.Bold });
            PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = YAxis, TitleFontWeight = FontWeights.Bold });
        }

        /// <summary>
        ///     Updates the plot.
        /// </summary>
        private void UpdatePlotSeries()
        {
            PlotModel.Series.Clear();
            PlotSeriesCollection.Clear(); //clear the PlotSeriesCollection because it will recreate each time
            ShowComplexPlotToggle = false; // do not show the complex toggle until a complex plot is plotted
            _isComplexPlot = false;

            foreach (var series in DataSeriesCollection)
            {
                ConstuctPlot(series);
            }
            CalculateMinMax();
            PlotModel.IsLegendVisible = !_hideKey;
            PlotModel.InvalidatePlot(true);
        }
    }
}