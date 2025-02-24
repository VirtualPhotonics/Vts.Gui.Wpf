using NSubstitute;
using NUnit.Framework;
using OxyPlot.Legends;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows;
using Vts.Common;
using Vts.Gui.Wpf.FileSystem;
using Vts.Gui.Wpf.Model;
using Vts.Gui.Wpf.ViewModel;
using Vts.IO;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels
{
    /// <summary>
    /// DataPointCollection Tests
    /// </summary>
    [TestFixture]
    public class DataPointCollectionTests
    {
        [Test]
        public void Verify_data_point_collection()
        {
            var dataPointCollection = new DataPointCollection
            {
                Title = "Title",
                DataPoints = new IDataPoint[]
                {
                    new DoubleDataPoint(0, 0), 
                    new DoubleDataPoint(1, 1)
                },
                ColorTag = "HSV"
            };
            Assert.That(dataPointCollection.Title, Is.EqualTo("Title"));
            Assert.That(dataPointCollection.DataPoints[0].X, Is.EqualTo(0));
            Assert.That(dataPointCollection.DataPoints[1].X, Is.EqualTo(1));
            Assert.That(dataPointCollection.ColorTag, Is.EqualTo("HSV"));
        }
    }

    /// <summary>
    /// PlotPointCollection Tests
    /// </summary>
    [TestFixture]
    public class PlotPointCollectionTests
    {
        [Test]
        public void Verify_default_constructor()
        {
            var plotPointCollection = new PlotPointCollection();
            Assert.That(plotPointCollection.ColorTags, Is.InstanceOf<List<string>>());
        }

        [Test]
        public void Verify_constructor()
        {
            var points = new[]
            {
                new[]
                {
                    new Point(0, 0), 
                    new Point(1, 1), 
                    new Point(3, 3)
                },
                new[]
                {
                    new Point(0.5, 0.5),
                    new Point(1.5, 1.5),
                    new Point(3.5, 3.5)
                },
            };
            var colorTags = new List<string> {"red", "white"};
            var plotPointCollection = new PlotPointCollection(points, colorTags);
            Assert.That(plotPointCollection.ColorTags[1], Is.EqualTo("white"));
            Assert.That(plotPointCollection.Count, Is.EqualTo(2));
        }

        [Test]
        public void Verify_add_points()
        {
            var points = new[]
            {
                new[]
                {
                    new Point(0.5, 0.5),
                    new Point(1.5, 1.5),
                    new Point(3.5, 3.5)
                },
            };
            var colorTags = new List<string> { "black" };
            var plotPointCollection = new PlotPointCollection(points, colorTags);
            Assert.That(plotPointCollection.ColorTags[0], Is.EqualTo("black"));
            Assert.That(plotPointCollection.Count, Is.EqualTo(1));
            var pointsBlue = new[]
            {
                new Point(0, 0),
                new Point(1, 1),
                new Point(3, 3)
            };
            var pointsYellow = new[]
            {
                new Point(0, 0),
                new Point(1, 1),
                new Point(3, 3)
            };
            plotPointCollection.Add(pointsBlue, "blue");
            plotPointCollection.Add(pointsYellow, "yellow");
            Assert.That(plotPointCollection.Count, Is.EqualTo(3));
            Assert.That(plotPointCollection.ColorTags[1], Is.EqualTo("blue"));
            Assert.That(plotPointCollection.ColorTags[2], Is.EqualTo("yellow"));
        }

        [Test]
        public void Verify_clone()
        {
            var points = new[]
            {
                new[]
                {
                    new Point(0.5, 0.5),
                    new Point(1.5, 1.5),
                    new Point(3.5, 3.5)
                },
            };
            var colorTags = new List<string> { "black" };
            var plotPointCollection = new PlotPointCollection(points, colorTags);
            var plotPointCollectionClone = plotPointCollection.Clone();
            Assert.That(plotPointCollectionClone.ColorTags[0], Is.EqualTo(plotPointCollection.ColorTags[0]));
            Assert.That(plotPointCollection.Count, Is.EqualTo(plotPointCollectionClone.Count));
        }
    }

    /// <summary>
    /// Summary description for PlotViewModel Tests
    /// </summary>
    [TestFixture]
    public class PlotViewModelTests
    {
        private PlotViewModel _plotViewModel;
        private PlotData[] _plotData;

        [OneTimeSetUp]
        public void One_time_setup()
        {
            var points = new[]
            {
                new Point(0.5, 0.5),
                new Point(1, 1),
                new Point(2, 2),
                new Point(3, 3),
                new Point(4, 4),
                new Point(5, 5),
                new Point(6, 6),
                new Point(7, 7),
                new Point(8, 8),
                new Point(9, 9)
            };
            _plotData = new[] { new PlotData(points, "Diagonal Line") };
            _plotViewModel = new PlotViewModel();
            _plotViewModel.PlotValues.Execute(_plotData);
        }

        /// <summary>
        /// Verifies that PlotViewModel default constructor 
        /// </summary>
        [Test]
        public void Verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new PlotViewModel();
            Assert.That(viewModel.XAxisSpacingOptionVm, Is.InstanceOf<OptionViewModel<ScalingType>>());
            Assert.That(viewModel.YAxisSpacingOptionVm, Is.InstanceOf<OptionViewModel<ScalingType>>());
            Assert.That(viewModel.PlotToggleTypeOptionVm, Is.InstanceOf<OptionViewModel<PlotToggleType>>());
            Assert.That(viewModel.PlotNormalizationTypeOptionVm, Is.InstanceOf<OptionViewModel<PlotNormalizationType>>());
            Assert.That(viewModel.CustomPlotLabel, Is.EqualTo(""));
            Assert.That(viewModel.PlotType, Is.EqualTo(ReflectancePlotType.ForwardSolver));
            Assert.That(viewModel.PlotModel.Title, Is.EqualTo(""));
            Assert.That(LegendPlacement.Outside, Is.EqualTo(viewModel.PlotModel.Legends[0].LegendPlacement));
        }

        // The following tests verify the Relay Commands

        /// <summary>
        /// Verifies that the ClearPlotCommand returns correct values
        /// </summary>
        [Test]
        public void Verify_ClearPlotCommand_returns_correct_values()
        {
            var viewModel = new PlotViewModel();
            viewModel.ClearPlotCommand.Execute(null);
            // ClearPlot public settings
            Assert.That(viewModel.Labels.Count, Is.EqualTo(0));
            Assert.That(viewModel.Title, Is.Null);
            // UpdatePlotSeries settings
            Assert.That(viewModel.PlotModel.Series.Count, Is.EqualTo(0));
            Assert.That(viewModel.PlotSeriesCollection.Count, Is.EqualTo(0));
            Assert.That(viewModel.ShowComplexPlotToggle, Is.False);
            Assert.That(viewModel.PlotModel.IsLegendVisible, Is.True);
        }

        /// <summary>
        /// Verifies that the SetAxisLabelsCommand returns correct values
        /// </summary>
        [Test]
        public void Verify_SetAxesLabelsCommand_returns_correct_values()
        {
            var viewModel = new PlotViewModel();
            var labels = new PlotAxesLabels("dependent", "units", 
                new IndependentAxisViewModel 
                {
                    AxisLabel = "independent",
                    AxisRangeVM = new RangeViewModel(new DoubleRange(0.5, 9.5, 19), "mm", IndependentVariableAxis.Rho, "Detector Positions"),
                    AxisType = IndependentVariableAxis.Rho,
                    AxisUnits = "units"
                }, 
                new[] { new ConstantAxisViewModel
                {
                    AxisLabel = "t",
                    AxisType = IndependentVariableAxis.Time,
                    AxisUnits = "ns",
                    AxisValue = 0.05,
                    ImageHeight = 1
                }});
            viewModel.SetAxesLabels.Execute(labels);
            Assert.That(viewModel.Title, Is.EqualTo($"dependent [units] versus independent [units]"));
            Assert.That(viewModel.AdditionalPlotValue, Is.EqualTo($"t = {0.05.ToString(Thread.CurrentThread.CurrentCulture)} ns"));
            var constantAxes = new[]
            {
                new ConstantAxisViewModel
                {
                    AxisLabel = "t",
                    AxisType = IndependentVariableAxis.Time,
                    AxisUnits = "ns",
                    AxisValue = 0.05,
                    ImageHeight = 1
                },
                new ConstantAxisViewModel
                {
                    AxisLabel = "z",
                    AxisType = IndependentVariableAxis.Z,
                    AxisUnits = "mm",
                    AxisValue = 0.1,
                    ImageHeight = 1
                }
            };
            labels.ConstantAxes = constantAxes;
            viewModel.SetAxesLabels.Execute(labels);
            Assert.That(viewModel.Title, Is.EqualTo($"dependent [units] versus independent [units]"));
            Assert.That(viewModel.AdditionalPlotValue, Is.EqualTo($"t = {0.05.ToString(Thread.CurrentThread.CurrentCulture)} ns\rz = {0.1.ToString(Thread.CurrentThread.CurrentCulture)} mm"));
        }

        [Test]
        public void Verify_Plot_SetCustomPlotLabel_Executed_is_correct()
        {
            var viewModel = new PlotViewModel();
            viewModel.SetCustomPlotLabel.Execute("customPlotLabel");
            Assert.That(viewModel.CustomPlotLabel, Is.EqualTo("customPlotLabel"));
        }

        // The following tests verify the Relay Commands
        /// <summary>
        /// Verifies that the PlotValues command returns correct values
        /// </summary>
        [Test]
        public void Verify_PlotValues_command_returns_correct_values()
        {
            var viewModel = new PlotViewModel();
            // CalculateMinMax settings
            Assert.That(viewModel.MinXValue, Is.EqualTo(1e-9));
            Assert.That(viewModel.MaxXValue, Is.EqualTo(1.0));
            Assert.That(viewModel.MinYValue, Is.EqualTo(1e-9));
            Assert.That(viewModel.MaxXValue, Is.EqualTo(1.0));
            // AddValuesToPlot settings 
            // Note cannot validate plotted data because DataSeriesCollection is private
            var data = new[,] { { 1.0, 2.0 }, { 3.0, 4.0 } };
            viewModel.CustomPlotLabel = "customPlotLabel";
            viewModel.PlotValues.Execute(data);
            Assert.That(viewModel.CustomPlotLabel, Is.EqualTo("customPlotLabel"));
            Assert.That(viewModel.ShowComplexPlotToggle, Is.False);
        }

        [Test]
        public void Verify_hold_on_false()
        {
            var viewModel = new PlotViewModel();
            // AddValuesToPlot settings 
            viewModel.PlotValues.Execute(_plotData);
            viewModel.HoldOn = true;
            viewModel.AutoScaleX = false;
            viewModel.AutoScaleY = false;
            Assert.That(viewModel.PlotModel.Series.Count, Is.EqualTo(1));
            viewModel.PlotValues.Execute(_plotData);
            Assert.That(viewModel.PlotModel.Series.Count, Is.EqualTo(2));
            viewModel.HoldOn = false;
            viewModel.PlotValues.Execute(_plotData);
            Assert.That(viewModel.PlotModel.Series.Count, Is.EqualTo(1));
        }

        [Test]
        public void Verify_auto_scale_false()
        {
            var viewModel = new PlotViewModel();
            viewModel.PlotValues.Execute(_plotData);
            viewModel.AutoScaleX = true;
            viewModel.AutoScaleY = false;
            Assert.That(viewModel.PlotModel.Series.Count, Is.EqualTo(1));
            Assert.That(viewModel.MinXValue, Is.EqualTo(0.5));
            Assert.That(viewModel.MaxXValue, Is.EqualTo(9.0));
            Assert.That(viewModel.MinYValue, Is.EqualTo(0.5));
            Assert.That(viewModel.MaxYValue, Is.EqualTo(9.0));
            viewModel.PlotValues.Execute(_plotData);
            viewModel.AutoScaleX = false;
            viewModel.AutoScaleY = true;
            Assert.That(viewModel.MinXValue, Is.EqualTo(0.5));
            Assert.That(viewModel.MaxXValue, Is.EqualTo(9.0));
            Assert.That(viewModel.MinYValue, Is.EqualTo(0.5));
            Assert.That(viewModel.MaxYValue, Is.EqualTo(9.0));
        }

        [Test]
        public void Verify_clone()
        {
            var viewModel = new PlotViewModel();
            var viewModelClone = viewModel.Clone();

            Assert.That(viewModelClone.Title, Is.EqualTo(viewModel.Title));
            Assert.That(viewModelClone.PlotTitles.Count, Is.EqualTo(viewModel.PlotTitles.Count));
            Assert.That(viewModelClone.PlotType, Is.EqualTo(viewModel.PlotType));
            Assert.That(viewModelClone.HoldOn, Is.EqualTo(viewModel.HoldOn));

            Assert.That(viewModelClone.Labels.Count, Is.EqualTo(viewModel.Labels.Count));

            Assert.That(viewModelClone.CustomPlotLabel, Is.EqualTo(viewModel.CustomPlotLabel));
            Assert.That(viewModelClone.ShowInPlotView, Is.False);

            Assert.That(viewModelClone.HideKey, Is.EqualTo(viewModel.HideKey));
            Assert.That(viewModelClone.ShowAxes, Is.EqualTo(viewModel.ShowAxes));

            // when we clone a plot view model without any plots, the min and max values will be infinity
            Assert.That(viewModelClone.MinXValue, Is.EqualTo(double.PositiveInfinity));
            Assert.That(viewModelClone.MaxXValue, Is.EqualTo(double.NegativeInfinity));
            Assert.That(viewModelClone.MinYValue, Is.EqualTo(double.PositiveInfinity));
            Assert.That(viewModelClone.MaxYValue, Is.EqualTo(double.NegativeInfinity));

            Assert.That(viewModelClone.AutoScaleX, Is.EqualTo(viewModel.AutoScaleX));
            Assert.That(viewModelClone.AutoScaleY, Is.EqualTo(viewModel.AutoScaleY));

            Assert.That(viewModelClone.CurrentIndependentVariableAxis, Is.EqualTo(viewModel.CurrentIndependentVariableAxis));

            Assert.That(viewModelClone.PlotNormalizationTypeOptionVm.Options[viewModelClone.PlotNormalizationTypeOptionVm.SelectedValue].IsSelected, Is.True);
            Assert.That(viewModelClone.PlotToggleTypeOptionVm.Options[viewModelClone.PlotToggleTypeOptionVm.SelectedValue].IsSelected, Is.True);
            Assert.That(viewModelClone.XAxisSpacingOptionVm.Options[viewModelClone.XAxisSpacingOptionVm.SelectedValue].IsSelected, Is.True);
            Assert.That(viewModelClone.YAxisSpacingOptionVm.Options[viewModelClone.YAxisSpacingOptionVm.SelectedValue].IsSelected, Is.True);
        }

        [Test]
        public void Verify_clone_with_plot_data()
        {
            var viewModelClone = _plotViewModel.Clone();

            Assert.That(viewModelClone.Title, Is.EqualTo(_plotViewModel.Title));
            Assert.That(viewModelClone.PlotTitles.Count, Is.EqualTo(_plotViewModel.PlotTitles.Count));
            Assert.That(viewModelClone.PlotTitles[0], Is.EqualTo(_plotViewModel.PlotTitles[0]));
            Assert.That(viewModelClone.PlotType, Is.EqualTo(_plotViewModel.PlotType));
            Assert.That(viewModelClone.HoldOn, Is.EqualTo(_plotViewModel.HoldOn));

            Assert.That(viewModelClone.Labels.Count, Is.EqualTo(_plotViewModel.Labels.Count));
            Assert.That(viewModelClone.Labels[0], Is.EqualTo(_plotViewModel.Labels[0]));

            Assert.That(viewModelClone.CustomPlotLabel, Is.EqualTo(_plotViewModel.CustomPlotLabel));
            Assert.That(viewModelClone.ShowInPlotView, Is.False);

            Assert.That(viewModelClone.HideKey, Is.EqualTo(_plotViewModel.HideKey));
            Assert.That(viewModelClone.ShowAxes, Is.EqualTo(_plotViewModel.ShowAxes));

            Assert.That(viewModelClone.MinXValue, Is.EqualTo(_plotViewModel.MinXValue));
            Assert.That(viewModelClone.MaxXValue, Is.EqualTo(_plotViewModel.MaxXValue));
            Assert.That(viewModelClone.MinYValue, Is.EqualTo(_plotViewModel.MinYValue));
            Assert.That(viewModelClone.MaxYValue, Is.EqualTo(_plotViewModel.MaxYValue));

            Assert.That(viewModelClone.AutoScaleX, Is.EqualTo(_plotViewModel.AutoScaleX));
            Assert.That(viewModelClone.AutoScaleY, Is.EqualTo(_plotViewModel.AutoScaleY));

            Assert.That(viewModelClone.CurrentIndependentVariableAxis, Is.EqualTo(_plotViewModel.CurrentIndependentVariableAxis));

            Assert.That(viewModelClone.PlotNormalizationTypeOptionVm.Options[viewModelClone.PlotNormalizationTypeOptionVm.SelectedValue].IsSelected, Is.True);
            Assert.That(viewModelClone.PlotToggleTypeOptionVm.Options[viewModelClone.PlotToggleTypeOptionVm.SelectedValue].IsSelected, Is.True);
            Assert.That(viewModelClone.XAxisSpacingOptionVm.Options[viewModelClone.XAxisSpacingOptionVm.SelectedValue].IsSelected, Is.True);
            Assert.That(viewModelClone.YAxisSpacingOptionVm.Options[viewModelClone.YAxisSpacingOptionVm.SelectedValue].IsSelected, Is.True);
        }

        [Test]
        public void Verify_manual_scales()
        {
            var viewModel = new PlotViewModel();
            Assert.That(viewModel.ManualScaleX, Is.False);
            Assert.That(viewModel.ManualScaleY, Is.False);
            viewModel.ManualScaleX = true;
            viewModel.ManualScaleY = true;
            Assert.That(viewModel.ManualScaleX, Is.True);
            Assert.That(viewModel.ManualScaleY, Is.True);
            Assert.That(viewModel.AutoScaleX, Is.False);
            Assert.That(viewModel.AutoScaleY, Is.False);
        }

        [Test]
        public void Verify_clear_plot_single()
        {
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(_plotData);
            plotViewModel.PlotValues.Execute(_plotData);
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(2));
            plotViewModel.ClearPlotSingleCommand.Execute(null);
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(1));
        }

        [Test]
        public void Verify_clear_plot_all()
        {
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(_plotData);
            plotViewModel.PlotValues.Execute(_plotData);
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(2));
            plotViewModel.ClearPlotCommand.Execute(null);
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(0));
        }

        [Test]
        public void Verify_duplicate_window()
        {
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(_plotData);
            // there is not an instance of window so duplicate with throw an error
            Assert.Throws<NullReferenceException>(() => plotViewModel.DuplicateWindowCommand.Execute(plotViewModel));
        }

        [Test]
        public void Verify_complex_plot()
        {
            IDataPoint[] points = {
                new ComplexDataPoint(0, new Complex(0, 1)),
                new ComplexDataPoint(1, new Complex(1, 2)),
                new ComplexDataPoint(2, new Complex(2, 3)),
                new ComplexDataPoint(3, new Complex(3, 4)),
                new ComplexDataPoint(4, new Complex(4, 5)),
                new ComplexDataPoint(5, new Complex(5, 6)),
                new ComplexDataPoint(6, new Complex(6, 7)),
                new ComplexDataPoint(7, new Complex(7, 8)),
                new ComplexDataPoint(8, new Complex(8, 9)),
                new ComplexDataPoint(9, new Complex(9, 10))
            };
            var plotData = new[] { new PlotData(points, "Complex plot") };
            var plotViewModel = new PlotViewModel();
            plotViewModel.PlotValues.Execute(plotData);
            // complex plots have 2 plots
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(2));
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = PlotToggleType.Phase;
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(1));
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = PlotToggleType.Amp;
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(1));
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = PlotToggleType.Complex;
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(2));
            // value verification performed in ComplexDataPoint tests
        }

        [Test]
        public void Verify_complex_derivative_plot()
        {
            IDataPoint[] points = {
                new ComplexDerivativeDataPoint(
                    0, 
                    new Complex(0, 1),
                    new Complex(2, 3),
                    ForwardAnalysisType.dRdMua),
                new ComplexDerivativeDataPoint(
                    1,
                    new Complex(1, 2),
                    new Complex(3, 4),
                    ForwardAnalysisType.dRdMua),
                new ComplexDerivativeDataPoint(
                    2,
                    new Complex(2, 3),
                    new Complex(4, 5),
                    ForwardAnalysisType.dRdMua),
                new ComplexDerivativeDataPoint(
                    3,
                    new Complex(4, 5),
                    new Complex(5, 6),
                    ForwardAnalysisType.dRdMua),
            };
            var plotData = new[] { new PlotData(points, "Complex derivative plot") };
            var plotViewModel = new PlotViewModel();
            plotViewModel.PlotValues.Execute(plotData);
            // complex plots have 2 plots
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(2));
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = PlotToggleType.Phase;
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(1));
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = PlotToggleType.Amp;
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(1));
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = PlotToggleType.Complex;
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(2));
            // value verification performed in ComplexDerivativeDataPoint tests
        }

        [Test]
        public void Verify_max_normalization()
        {
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(_plotData);
            plotViewModel.PlotValues.Execute(_plotData);
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(2));
            plotViewModel.PlotNormalizationTypeOptionVm.SelectedValue = PlotNormalizationType.RelativeToMax;
            var plotSeries = plotViewModel.PlotSeriesCollection[0];
            var i = 0;
            foreach (var point in plotSeries)
            {
                Assert.That(point, Is.InstanceOf<Point>());
                i++;
            }
            Assert.That(i, Is.EqualTo(10));
        }

        [Test]
        public void Verify_curve_normalization()
        {
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(_plotData);
            plotViewModel.PlotValues.Execute(_plotData);
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(2));
            plotViewModel.PlotNormalizationTypeOptionVm.SelectedValue = PlotNormalizationType.RelativeToCurve;
            var plotSeries = plotViewModel.PlotSeriesCollection[0];
            var i = 0;
            foreach (var point in plotSeries)
            {
                Assert.That(point, Is.InstanceOf<Point>());
                i++;
            }
            Assert.That(i, Is.EqualTo(10));
        }

        [Test]
        [TestCase(PlotToggleType.Complex)]
        [TestCase(PlotToggleType.Phase)]
        [TestCase(PlotToggleType.Amp)]
        public void Verify_max_normalization_phase_amp_complex(PlotToggleType toggleType)
        {
            IDataPoint[] points = {
                new ComplexDataPoint(0, new Complex(0, 1)),
                new ComplexDataPoint(1, new Complex(1, 2)),
                new ComplexDataPoint(2, new Complex(2, 3)),
                new ComplexDataPoint(3, new Complex(3, 4)),
                new ComplexDataPoint(4, new Complex(4, 5)),
                new ComplexDataPoint(5, new Complex(5, 6)),
                new ComplexDataPoint(6, new Complex(6, 7)),
                new ComplexDataPoint(7, new Complex(7, 8)),
                new ComplexDataPoint(8, new Complex(8, 9)),
                new ComplexDataPoint(9, new Complex(9, 10))
            };
            var plotData = new[] { new PlotData(points, "Complex plot") };
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = toggleType;
            plotViewModel.PlotValues.Execute(plotData);
            plotViewModel.PlotValues.Execute(plotData);
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(toggleType == PlotToggleType.Complex ? 4 : 2));
            plotViewModel.PlotNormalizationTypeOptionVm.SelectedValue = PlotNormalizationType.RelativeToMax;
            var plotSeries = plotViewModel.PlotSeriesCollection[0];
            var i = 0;
            foreach (var point in plotSeries)
            {
                Assert.That(point, Is.InstanceOf<Point>());
                i++;
            }
            Assert.That(i, Is.EqualTo(10));
        }

        [Test]
        public void Verify_curve_normalization_complex()
        {
            IDataPoint[] points = {
                new ComplexDataPoint(2, new Complex(5, 1)),
                new ComplexDataPoint(1, new Complex(1, 2)),
                new ComplexDataPoint(2, new Complex(2, 3)),
                new ComplexDataPoint(3, new Complex(3, 4)),
                new ComplexDataPoint(4, new Complex(4, 5)),
                new ComplexDataPoint(5, new Complex(5, 6)),
                new ComplexDataPoint(6, new Complex(6, 7)),
                new ComplexDataPoint(7, new Complex(7, 8)),
                new ComplexDataPoint(8, new Complex(8, 9)),
                new ComplexDataPoint(9, new Complex(9, 10))
            };
            var plotData = new[] { new PlotData(points, "Complex plot") };
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(plotData);
            plotViewModel.PlotValues.Execute(plotData);
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(4));
            plotViewModel.PlotNormalizationTypeOptionVm.SelectedValue = PlotNormalizationType.RelativeToCurve;
            var plotSeries = plotViewModel.PlotSeriesCollection[0];
            var i = 0;
            foreach (var point in plotSeries)
            {
                Assert.That(point, Is.InstanceOf<Point>());
                i++;
            }
            Assert.That(i, Is.EqualTo(10));
        }

        [Test]
        [TestCase(PlotToggleType.Complex)]
        [TestCase(PlotToggleType.Phase)]
        [TestCase(PlotToggleType.Amp)]
        public void Verify_max_normalization_phase_amp_complex_derivative(PlotToggleType toggleType)
        {
            IDataPoint[] points = {
                new ComplexDerivativeDataPoint(
                    0,
                    new Complex(0, 1),
                    new Complex(2, 3),
                    ForwardAnalysisType.dRdMua),
                new ComplexDerivativeDataPoint(
                    1,
                    new Complex(1, 2),
                    new Complex(3, 4),
                    ForwardAnalysisType.dRdMua),
                new ComplexDerivativeDataPoint(
                    2,
                    new Complex(2, 3),
                    new Complex(4, 5),
                    ForwardAnalysisType.dRdMua),
                new ComplexDerivativeDataPoint(
                    3,
                    new Complex(4, 5),
                    new Complex(5, 6),
                    ForwardAnalysisType.dRdMua),
            };
            var plotData = new[] { new PlotData(points, "Complex derivative plot") };
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = toggleType;
            plotViewModel.PlotValues.Execute(plotData);
            plotViewModel.PlotValues.Execute(plotData);
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(toggleType == PlotToggleType.Complex ? 4 : 2));
            plotViewModel.PlotNormalizationTypeOptionVm.SelectedValue = PlotNormalizationType.RelativeToMax;
            var plotSeries = plotViewModel.PlotSeriesCollection[0];
            var i = 0;
            foreach (var point in plotSeries)
            {
                Assert.That(point, Is.InstanceOf<Point>());
                i++;
            }
            Assert.That(i, Is.EqualTo(toggleType == PlotToggleType.Phase ? 3 : 4));
        }

        [Test]
        public void Verify_curve_normalization_complex_derivative()
        {
            IDataPoint[] points = {
                new ComplexDerivativeDataPoint(
                    0,
                    new Complex(0, 1),
                    new Complex(2, 3),
                    ForwardAnalysisType.dRdMua),
                new ComplexDerivativeDataPoint(
                    1,
                    new Complex(1, 2),
                    new Complex(3, 4),
                    ForwardAnalysisType.dRdMua),
                new ComplexDerivativeDataPoint(
                    2,
                    new Complex(2, 3),
                    new Complex(4, 5),
                    ForwardAnalysisType.dRdMua),
                new ComplexDerivativeDataPoint(
                    3,
                    new Complex(4, 5),
                    new Complex(5, 6),
                    ForwardAnalysisType.dRdMua),
            };
            var plotData = new[] { new PlotData(points, "Complex derivative plot") };
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(plotData);
            plotViewModel.PlotValues.Execute(plotData);
            Assert.That(plotViewModel.PlotModel.Series.Count, Is.EqualTo(4));
            plotViewModel.PlotNormalizationTypeOptionVm.SelectedValue = PlotNormalizationType.RelativeToCurve;
            var plotSeries = plotViewModel.PlotSeriesCollection[0];
            var i = 0;
            foreach (var point in plotSeries)
            {
                Assert.That(point, Is.InstanceOf<Point>());
                i++;
            }
            Assert.That(i, Is.EqualTo(4));
        }

        /// <summary>
        /// ExportDataToTextCommand brings up Dialog window so not tested
        /// DuplicateWindowCommand - not sure if can test 
        /// </summary>

        /// <summary>
        /// Test to verify only linear values written if user specifies
        /// x-axis log scale and/or y-axis log scale to be plotted
        /// </summary>
        [Test]
        public void Verify_ExportDataToText_correct_when_X_and_Y_scaling_set_to_log()
        {
            // clear any prior test results
            const string exportFilename = "testexportdata.txt";
            FileIO.FileDelete(exportFilename);

            var points = new[]
            {
                new Point(1, 1),
                new Point(2, 2),
                new Point(3, 3),
                new Point(4, 4),
                new Point(5, 5),
                new Point(6, 6),
            };
            // plot the data to be saved
            var plotData = new[] { new PlotData(points, "Diagonal Line") };

            // mock the IOpenFileDialog
            var openFileDialogMock = Substitute.For<ISaveFileDialog>();
            openFileDialogMock.FileName.Returns(exportFilename);
            openFileDialogMock.ShowDialog().Returns(true);

            _plotViewModel = new PlotViewModel(0, openFileDialogMock);
            _plotViewModel.PlotValues.Execute(plotData);
            _plotViewModel.XAxisSpacingOptionVm.SelectedValue = ScalingType.Log;
            _plotViewModel.YAxisSpacingOptionVm.SelectedValue = ScalingType.Log;
            _plotViewModel.ExportDataToTextCommand.Execute(plotData);

            // verify results in file
            var stream = new FileStream(exportFilename, FileMode.Open);
            using var sw = new StreamReader(stream);
            // skip header
            for (var i = 0; i < 4; i++)
            {
                sw.ReadLine();
            }
            // read and verify data
            foreach (var t in points)
            {
                var line = sw.ReadLine();
                if (line == null) continue;
                var data = line.Split();
                // check x value
                Assert.That(data[0], Is.EqualTo(t.X.ToString(CultureInfo.InvariantCulture)));
                // check y value
                Assert.That(data[1], Is.EqualTo(t.Y.ToString(CultureInfo.InvariantCulture)));
            }
        }

        /// <summary>
        /// Test to verify only real and imaginary values written if user specifies
        /// phase or amplitude to be plotted
        /// </summary>
        [Test]
        public void Verify_ExportDataToText_correct_when_complex_data_plotted()
        {
            // clear any prior test results
            const string exportFilename = "testexportdata.txt";
            FileIO.FileDelete(exportFilename);

            IDataPoint[] points =
            {
                new ComplexDataPoint(1, new Complex(1, 1)),
                new ComplexDataPoint(2, new Complex(2, 2)),
                new ComplexDataPoint(3, new Complex(3, 3)),
                new ComplexDataPoint(4, new Complex(4, 4)),
                new ComplexDataPoint(5, new Complex(5, 5)),
                new ComplexDataPoint(6, new Complex(6, 6)),
            };
            // plot the data to be saved
            var plotData = new[] { new PlotData(points, "Real and Imaginary Lines") };

            // mock the IOpenFileDialog
            var openFileDialogMock = Substitute.For<ISaveFileDialog>();
            openFileDialogMock.FileName.Returns(exportFilename);
            openFileDialogMock.ShowDialog().Returns(true);

            _plotViewModel = new PlotViewModel(0, openFileDialogMock);
            _plotViewModel.PlotValues.Execute(plotData);
            _plotViewModel.PlotToggleTypeOptionVm.SelectedValue = PlotToggleType.Amp;
            _plotViewModel.ExportDataToTextCommand.Execute(plotData);

            // verify results in file
            var stream = new FileStream(exportFilename, FileMode.Open);
            using var sw = new StreamReader(stream);
            // skip header
            for (var i = 0; i < 4; i++)
            {
                sw.ReadLine();
            }
            // read and verify data
            foreach (var t in points.Cast<ComplexDataPoint>().ToArray())
            {
                var line = sw.ReadLine();
                if (line == null) continue;
                var data = line.Split();
                // check x value
                Assert.That(data[0], Is.EqualTo(t.X.ToString(CultureInfo.InvariantCulture)));
                // check real value
                Assert.That(data[1], Is.EqualTo(t.Y.Real.ToString(CultureInfo.InvariantCulture)));
                // check imaginary value
                Assert.That(data[1], Is.EqualTo(t.Y.Imaginary.ToString(CultureInfo.InvariantCulture)));

            }
        }

    }
}