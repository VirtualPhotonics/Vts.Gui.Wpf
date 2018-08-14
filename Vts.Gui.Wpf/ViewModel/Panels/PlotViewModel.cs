using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Vts.Extensions;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using FontWeights = OxyPlot.FontWeights;

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
        private bool _AutoScaleX;
        private bool _AutoScaleY;
        private IndependentVariableAxis _CurrentIndependentVariableAxis;
        private string _CustomPlotLabel;

        private bool _HideKey;
        private bool _HoldOn;
        private bool _IsComplexPlot;
        private IList<string> _Labels;
        private double _MaxXValue;
        private double _MaxYValue;
        private double _MinXValue;
        private double _MinYValue;
        private PlotModel _plotModel;
        private OptionViewModel<PlotNormalizationType> _PlotNormalizationTypeOptionVM;
        private PlotPointCollection _PlotSeriesCollection;
        private IList<string> _PlotTitles;
        private OptionViewModel<PlotToggleType> _PlotToggleTypeOptionVM;
        private ReflectancePlotType _PlotType;
        // change from Point to our own custom class so we can bind to color, style, etc, too

        private int _plotViewId;
        private bool _ShowAxes;
        private bool _showComplexPlotToggle;
        private bool _ShowInPlotView;
        private string _Title;
        private OptionViewModel<ScalingType> _XAxisSpacingOptionVM;
        private OptionViewModel<ScalingType> _YAxisSpacingOptionVM;

        public PlotViewModel(int plotViewId = 0)
        {
            _plotViewId = plotViewId;
            _MinYValue = 1E-9;
            _MaxYValue = 1.0;
            _MinXValue = 1E-9;
            _MaxXValue = 1.0;
            _AutoScaleX = true;
            _AutoScaleY = true;

            RealLabels = new List<string>();
            ImagLabels = new List<string>();
            PhaseLabels = new List<string>();
            AmplitudeLabels = new List<string>();
            
            Labels = new List<string>();
            PlotTitles = new List<string>();
            DataSeriesCollection = new List<DataPointCollection>();
            PlotSeriesCollection = new PlotPointCollection();
            //IsComplexPlot = false;

            PlotModel = new PlotModel
            {
                Title = "",
                LegendPlacement = LegendPlacement.Outside,
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
            PlotType = ReflectancePlotType.ForwardSolver;
            _HoldOn = true;
            _HideKey = false;
            _ShowInPlotView = true;
            _ShowAxes = false;
            _showComplexPlotToggle = false;

            XAxisSpacingOptionVM = new OptionViewModel<ScalingType>("XAxisSpacing_" + _plotViewId, false);
            XAxisSpacingOptionVM.PropertyChanged += (sender, args) => UpdatePlotSeries();

            YAxisSpacingOptionVM = new OptionViewModel<ScalingType>("YAxisSpacing_" + _plotViewId, false);
            YAxisSpacingOptionVM.PropertyChanged += (sender, args) => UpdatePlotSeries();

            PlotToggleTypeOptionVM = new OptionViewModel<PlotToggleType>("ToggleType_" + _plotViewId, false);
            PlotToggleTypeOptionVM.PropertyChanged += (sender, args) => UpdatePlotSeries();

            PlotNormalizationTypeOptionVM =
                new OptionViewModel<PlotNormalizationType>("NormalizationType_" + _plotViewId, false);
            PlotNormalizationTypeOptionVM.PropertyChanged += (sender, args) => UpdatePlotSeries();

            CustomPlotLabel = "";

            //Commands.Plot_PlotValues.Executed += Plot_Executed;
            //Commands.Plot_SetAxesLabels.Executed += Plot_SetAxesLabels_Executed;

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

        public string HideKeyLabel
        {
            get { return StringLookup.GetLocalizedString("Label_HideKey"); }
        }

        public string HoldOnLabel
        {
            get { return StringLookup.GetLocalizedString("Label_HoldOn"); }
        }

        public string ClearAllButtonLabel
        {
            get { return StringLookup.GetLocalizedString("Button_ClearAll"); }
        }

        public string ClearNewestButtonLabel
        {
            get { return StringLookup.GetLocalizedString("Button_ClearNewest"); }
        }

        public string XAxisSpacingLabel
        {
            get { return StringLookup.GetLocalizedString("Label_XAxisSpacing"); }
        }

        public string YAxisSpacingLabel
        {
            get { return StringLookup.GetLocalizedString("Label_YAxisSpacing"); }
        }

        public string PlotToggleLabel
        {
            get { return StringLookup.GetLocalizedString("Label_PlotToggle"); }
        }

        public string NormalizationLabel
        {
            get { return StringLookup.GetLocalizedString("Label_Normalization"); }
        }

        public string PlotLabel
        {
            get { return StringLookup.GetLocalizedString("Label_PlotLabel"); }
        }

        public string ExportImageButtonLabel
        {
            get { return StringLookup.GetLocalizedString("Button_ExportImage"); }
        }

        public string ExportDataButtonLabel
        {
            get { return StringLookup.GetLocalizedString("Button_ExportData"); }
        }

        public string AutoScaleXLabel
        {
            get { return StringLookup.GetLocalizedString("Label_AutoScaleX"); }
        }

        public string MinXLabel
        {
            get { return StringLookup.GetLocalizedString("Label_MinX"); }
        }

        public string MaxXLabel
        {
            get { return StringLookup.GetLocalizedString("Label_MaxX"); }
        }

        public string AutoScaleYLabel
        {
            get { return StringLookup.GetLocalizedString("Label_AutoScaleY"); }
        }

        public string MinYLabel
        {
            get { return StringLookup.GetLocalizedString("Label_MinY"); }
        }

        public string MaxYLabel
        {
            get { return StringLookup.GetLocalizedString("Label_MaxY"); }
        }

        private string XAxis { get; set; }
        private string YAxis { get; set; }

        private List<DataPointCollection> DataSeriesCollection { get; set; }
        //private IList<IList<IDataPoint>> DataSeriesCollectionToggle { get; set; }
        private IList<string> RealLabels { get; set; }
        private IList<string> ImagLabels { get; set; }
        private IList<string> PhaseLabels { get; set; }
        private IList<string> AmplitudeLabels { get; set; }

        public PlotModel PlotModel
        {
            get { return _plotModel; }
            set
            {
                _plotModel = value;
                OnPropertyChanged("PlotModel");
            }
        }

        public PlotPointCollection PlotSeriesCollection
        {
            get { return _PlotSeriesCollection; }
            set
            {
                _PlotSeriesCollection = value;
                OnPropertyChanged("PlotSeriesCollection");
            }
        }

        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                OnPropertyChanged("Title");
            }
        }

        public ReflectancePlotType PlotType
        {
            get { return _PlotType; }
            set
            {
                _PlotType = value;
                OnPropertyChanged("PlotType");
            }
        }

        public bool HoldOn
        {
            get { return _HoldOn; }
            set
            {
                _HoldOn = value;
                OnPropertyChanged("HoldOn");
            }
        }

        public bool HideKey
        {
            get { return _HideKey; }
            set
            {
                _HideKey = value;
                OnPropertyChanged("HideKey");
                UpdatePlotSeries();
            }
        }

        public bool ShowInPlotView
        {
            get { return _ShowInPlotView; }
            set
            {
                _ShowInPlotView = value;
                OnPropertyChanged("ShowInPlotView");
            }
        }

        public bool ShowAxes
        {
            get { return _ShowAxes; }
            set
            {
                _ShowAxes = value;
                OnPropertyChanged("ShowAxes");
            }
        }

        public bool ShowComplexPlotToggle
        {
            get { return _showComplexPlotToggle; }
            set
            {
                _showComplexPlotToggle = value;
                OnPropertyChanged("ShowComplexPlotToggle");
            }
        }

        public OptionViewModel<ScalingType> XAxisSpacingOptionVM
        {
            get { return _XAxisSpacingOptionVM; }
            set
            {
                _XAxisSpacingOptionVM = value;
                OnPropertyChanged("XAxisSpacingOptionVM");
            }
        }

        public OptionViewModel<ScalingType> YAxisSpacingOptionVM
        {
            get { return _YAxisSpacingOptionVM; }
            set
            {
                _YAxisSpacingOptionVM = value;
                OnPropertyChanged("YAxisSpacingOptionVM");
            }
        }

        public OptionViewModel<PlotToggleType> PlotToggleTypeOptionVM
        {
            get { return _PlotToggleTypeOptionVM; }
            set
            {
                _PlotToggleTypeOptionVM = value;
                OnPropertyChanged("PlotToggleTypeOptionVM");
            }
        }

        public IndependentVariableAxis CurrentIndependentVariableAxis
        {
            get { return _CurrentIndependentVariableAxis; }
            set
            {
                // if user switches independent variable, clear plot
                if (_CurrentIndependentVariableAxis != value && ShowInPlotView)
                {
                    ClearPlot();
                    WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(
                        StringLookup.GetLocalizedString("Message_PlotViewCleared") + "\r");
                }
                _CurrentIndependentVariableAxis = value;
                OnPropertyChanged("CurrentIndependentVariableAxis");
            }
        }

        public OptionViewModel<PlotNormalizationType> PlotNormalizationTypeOptionVM
        {
            get { return _PlotNormalizationTypeOptionVM; }
            set
            {
                _PlotNormalizationTypeOptionVM = value;
                OnPropertyChanged("PlotNormalizationTypeOptionVM");
            }
        }

        public string CustomPlotLabel
        {
            get { return _CustomPlotLabel; }
            set
            {
                _CustomPlotLabel = value;
                OnPropertyChanged("CustomPlotLabel");
            }
        }

        public IList<string> Labels
        {
            get { return _Labels; }
            set
            {
                _Labels = value;
                OnPropertyChanged("Labels");
            }
        }

        public IList<string> PlotTitles
        {
            get { return _PlotTitles; }
            set
            {
                _PlotTitles = value;
                OnPropertyChanged("PlotTitles");
            }
        }

        public bool AutoScaleX
        {
            get { return _AutoScaleX; }
            set
            {
                _AutoScaleX = value;
                OnPropertyChanged("AutoScaleX");
                OnPropertyChanged("ManualScaleX");
            }
        }

        public bool AutoScaleY
        {
            get { return _AutoScaleY; }
            set
            {
                _AutoScaleY = value;
                OnPropertyChanged("AutoScaleY");
                OnPropertyChanged("ManualScaleY");
            }
        }

        public bool ManualScaleX
        {
            get { return !_AutoScaleX; }
            set
            {
                _AutoScaleX = !value;
                OnPropertyChanged("ManualScaleX");
                OnPropertyChanged("AutoScaleX");
            }
        }

        public bool ManualScaleY
        {
            get { return !_AutoScaleY; }
            set
            {
                _AutoScaleY = !value;
                OnPropertyChanged("ManualScaleY");
                OnPropertyChanged("AutoScaleY");
            }
        }

        public double MinXValue
        {
            get { return _MinXValue; }
            set
            {
                _MinXValue = value;
                OnPropertyChanged("MinXValue");
            }
        }

        public double MaxXValue
        {
            get { return _MaxXValue; }
            set
            {
                _MaxXValue = value;
                OnPropertyChanged("MaxXValue");
            }
        }

        public double MinYValue
        {
            get { return _MinYValue; }
            set
            {
                _MinYValue = value;
                OnPropertyChanged("MinYValue");
            }
        }

        public double MaxYValue
        {
            get { return _MaxYValue; }
            set
            {
                _MaxYValue = value;
                OnPropertyChanged("MaxYValue");
            }
        }

        private void Plot_DuplicateWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = Clone();
            vm.UpdatePlotSeries();
            MainWindow.Current.Main_DuplicatePlotView_Executed(vm);
            //Commands.Main_DuplicatePlotView.Execute(vm, vm);
        }

        public PlotViewModel Clone()
        {
            return Clone(this);
        }

        public static PlotViewModel Clone(PlotViewModel plotToClone)
        {
            var output = new PlotViewModel(plotToClone._plotViewId + 1);
            plotToClone._plotViewId += 1;

            //Commands.Plot_PlotValues.Executed -= output.Plot_Executed;

            output._Title = plotToClone._Title;
            output._PlotTitles = plotToClone._PlotTitles.ToList();
            output._PlotType = plotToClone._PlotType;
            output._HoldOn = plotToClone._HoldOn;
            output._PlotSeriesCollection = new PlotPointCollection();
            output._Labels = plotToClone._Labels.ToList();
            output._CustomPlotLabel = plotToClone._CustomPlotLabel;
            output.ShowInPlotView = false;
            output._HideKey = plotToClone.HideKey;
            output._ShowAxes = plotToClone._ShowAxes;
            output._MinYValue = plotToClone._MinYValue;
            output._MaxYValue = plotToClone._MaxYValue;
            output._MinXValue = plotToClone._MinXValue;
            output._MaxXValue = plotToClone._MaxXValue;
            output._AutoScaleX = plotToClone._AutoScaleX;
            output._AutoScaleY = plotToClone._AutoScaleY;
            output._IsComplexPlot = plotToClone._IsComplexPlot;
            output._CurrentIndependentVariableAxis = plotToClone._CurrentIndependentVariableAxis;

            output.RealLabels = plotToClone.RealLabels;
            output.ImagLabels = plotToClone.ImagLabels;
            output.PhaseLabels = plotToClone.PhaseLabels;
            output.AmplitudeLabels = plotToClone.AmplitudeLabels;

            output._YAxisSpacingOptionVM.Options[output._YAxisSpacingOptionVM.SelectedValue].IsSelected = false;
            output._PlotNormalizationTypeOptionVM.Options[output._PlotNormalizationTypeOptionVM.SelectedValue]
                .IsSelected = false;
            output._PlotToggleTypeOptionVM.Options[output._PlotToggleTypeOptionVM.SelectedValue].IsSelected = false;
            output._XAxisSpacingOptionVM.Options[output._XAxisSpacingOptionVM.SelectedValue].IsSelected = false;
            output._YAxisSpacingOptionVM.Options[plotToClone._YAxisSpacingOptionVM.SelectedValue].IsSelected = true;
            output._PlotNormalizationTypeOptionVM.Options[plotToClone._PlotNormalizationTypeOptionVM.SelectedValue]
                .IsSelected = true;
            output._PlotToggleTypeOptionVM.Options[plotToClone._PlotToggleTypeOptionVM.SelectedValue].IsSelected = true;
            output._XAxisSpacingOptionVM.Options[plotToClone._XAxisSpacingOptionVM.SelectedValue].IsSelected = true;

            output.DataSeriesCollection =
                plotToClone.DataSeriesCollection.Select(
                    ds =>
                        new DataPointCollection
                        {
                            DataPoints = ds.DataPoints.Select(val => val).ToArray(),
                            ColorTag = ds.ColorTag,
                            Title = ds.Title
                        }).ToList();
            //output.DataSeriesCollectionToggle =
            //    plotToClone.DataSeriesCollectionToggle.Select(ds => (IList<IDataPoint>)ds.Select(val => val).ToList()).ToList();

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
            if (sender is PlotAxesLabels)
            {
                var labels = (PlotAxesLabels) sender;
                // set CurrentIndependtVariableAxis prior to setting Title because property
                // might ClearPlot including Title
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
            if (_Labels != null && _Labels.Count > 0 && _PlotSeriesCollection != null && _PlotSeriesCollection.Count > 0)
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
                using (var stream = new FileStream(filename, FileMode.Create))
                {
                    using (var sw = new StreamWriter(stream))
                    {
                        sw.Write("%");
                        _Labels.ForEach(label => sw.Write(label + " (X)" + "\t" + label + " (Y)" + "\t"));
                        sw.WriteLine();
                        for (var i = 0; i < _PlotSeriesCollection[0].Length; i++)
                        {
                            sw.WriteLine();
                            foreach (var t in _PlotSeriesCollection)
                            {
                                sw.Write(t[i].X + "\t" + t[i].Y + "\t");
                            }
                        }
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
            var data = arr as PlotData[];
            if (data != null)
            {
                AddValuesToPlotData(data);
            }
        }

        //static int labelCounter = 0;
        private void AddValuesToPlotData(PlotData[] plotData)
        {
            if (!_HoldOn)
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
            var normToCurve = PlotNormalizationTypeOptionVM.SelectedValue == PlotNormalizationType.RelativeToCurve &&
                              DataSeriesCollection.Count > 1;
            var normToMax = PlotNormalizationTypeOptionVM.SelectedValue == PlotNormalizationType.RelativeToMax &&
                            DataSeriesCollection.Count > 0;

            var tempPointArrayA = new List<Point>();
            var tempPointArrayB = new List<Point>();

            double x;
            double y;
            var lineSeriesA = new LineSeries();
            var lineSeriesB = new LineSeries(); //we need B for complex
            if (dataPointCollection.DataPoints[0] is ComplexDataPoint)
            {
                // normalization calculations
                var max = 1.0;
                var maxRe = 1.0;
                var maxIm = 1.0;
                if (normToMax)
                {
                    var points = dataPointCollection.DataPoints.Cast<ComplexDataPoint>().ToArray();
                    switch (PlotToggleTypeOptionVM.SelectedValue)
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
                    x = XAxisSpacingOptionVM.SelectedValue == ScalingType.Log ? Math.Log10(dp.X) : dp.X;
                    switch (PlotToggleTypeOptionVM.SelectedValue)
                    {
                        case PlotToggleType.Phase:
                            y = -(dp.Y.Phase*(180/Math.PI));
                            switch (PlotNormalizationTypeOptionVM.SelectedValue)
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
                            switch (PlotNormalizationTypeOptionVM.SelectedValue)
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
                            switch (PlotNormalizationTypeOptionVM.SelectedValue)
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
                            y = YAxisSpacingOptionVM.SelectedValue == ScalingType.Log ? Math.Log10(y) : y;
                            var p = new DataPoint(x, y);
                            if (isValidDataPoint(p) && isWithinAxes(p))
                            {
                                lineSeriesB.Points.Add(p);
                                //Add the data to the tempPointArray to add to the PlotSeriesCollection
                                tempPointArrayB.Add(new Point(x, y));
                            }
                            y = dp.Y.Imaginary;
                            //break; // handle imag within switch
                            switch (PlotNormalizationTypeOptionVM.SelectedValue)
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
                    y = YAxisSpacingOptionVM.SelectedValue == ScalingType.Log ? Math.Log10(y) : y;
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
                    x = XAxisSpacingOptionVM.SelectedValue == ScalingType.Log ? Math.Log10(dp.X) : dp.X;
                    switch (PlotNormalizationTypeOptionVM.SelectedValue)
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
                    y = YAxisSpacingOptionVM.SelectedValue == ScalingType.Log ? Math.Log10(y) : y;
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
                switch (PlotToggleTypeOptionVM.SelectedValue)
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

            foreach (var series in DataSeriesCollection)
            {
                ConstuctPlot(series);
            }
            CalculateMinMax();
            PlotModel.IsLegendVisible = !_HideKey;
            PlotModel.InvalidatePlot(true);
        }
    }
}