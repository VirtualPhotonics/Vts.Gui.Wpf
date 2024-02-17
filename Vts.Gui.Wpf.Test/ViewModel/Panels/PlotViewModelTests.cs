using NSubstitute;
using NUnit.Framework;
using OxyPlot.Legends;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows;
using Vts.Common;
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
            Assert.AreEqual("Title", dataPointCollection.Title);
            Assert.AreEqual(0, dataPointCollection.DataPoints[0].X);
            Assert.AreEqual(1, dataPointCollection.DataPoints[1].X);
            Assert.AreEqual("HSV", dataPointCollection.ColorTag);
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
            Assert.IsInstanceOf<List<string>>(plotPointCollection.ColorTags);
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
            Assert.AreEqual("white",plotPointCollection.ColorTags[1]);
            Assert.AreEqual(2, plotPointCollection.Count);
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
            Assert.AreEqual("black", plotPointCollection.ColorTags[0]);
            Assert.AreEqual(1, plotPointCollection.Count);
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
            Assert.AreEqual(3, plotPointCollection.Count);
            Assert.AreEqual("blue", plotPointCollection.ColorTags[1]);
            Assert.AreEqual("yellow", plotPointCollection.ColorTags[2]);
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
            Assert.AreEqual(plotPointCollection.ColorTags[0], plotPointCollectionClone.ColorTags[0]);
            Assert.AreEqual(plotPointCollectionClone.Count, plotPointCollection.Count);
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
            Assert.IsInstanceOf<OptionViewModel<ScalingType>>(viewModel.XAxisSpacingOptionVm);
            Assert.IsInstanceOf<OptionViewModel<ScalingType>>(viewModel.YAxisSpacingOptionVm);
            Assert.IsInstanceOf<OptionViewModel<PlotToggleType>>(viewModel.PlotToggleTypeOptionVm);
            Assert.IsInstanceOf<OptionViewModel<PlotNormalizationType>>(viewModel.PlotNormalizationTypeOptionVm);
            Assert.AreEqual("", viewModel.CustomPlotLabel);
            Assert.AreEqual(ReflectancePlotType.ForwardSolver, viewModel.PlotType);
            Assert.AreEqual("", viewModel.PlotModel.Title);
            Assert.AreEqual(viewModel.PlotModel.Legends[0].LegendPlacement, LegendPlacement.Outside);
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
            Assert.AreEqual(0, viewModel.Labels.Count);
            Assert.IsNull(viewModel.Title);
            // UpdatePlotSeries settings
            Assert.AreEqual(0, viewModel.PlotModel.Series.Count);
            Assert.AreEqual(0, viewModel.PlotSeriesCollection.Count);
            Assert.IsFalse(viewModel.ShowComplexPlotToggle);
            Assert.IsTrue(viewModel.PlotModel.IsLegendVisible);
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
            Assert.AreEqual($"dependent [units] versus independent [units]", viewModel.Title);
            Assert.AreEqual($"t = {0.05.ToString(Thread.CurrentThread.CurrentCulture)} ns", viewModel.AdditionalPlotValue);
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
            Assert.AreEqual($"dependent [units] versus independent [units]", viewModel.Title);
            Assert.AreEqual($"t = {0.05.ToString(Thread.CurrentThread.CurrentCulture)} ns\rz = {0.1.ToString(Thread.CurrentThread.CurrentCulture)} mm", viewModel.AdditionalPlotValue);
        }

        [Test]
        public void Verify_Plot_SetCustomPlotLabel_Executed_is_correct()
        {
            var viewModel = new PlotViewModel();
            viewModel.SetCustomPlotLabel.Execute("customPlotLabel");
            Assert.AreEqual("customPlotLabel", viewModel.CustomPlotLabel);
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
            Assert.AreEqual(1e-9, viewModel.MinXValue);
            Assert.AreEqual(1.0, viewModel.MaxXValue);
            Assert.AreEqual(1e-9, viewModel.MinYValue);
            Assert.AreEqual(1.0, viewModel.MaxXValue);
            // AddValuesToPlot settings 
            // Note cannot validate plotted data because DataSeriesCollection is private
            var data = new[,] { { 1.0, 2.0 }, { 3.0, 4.0 } };
            viewModel.CustomPlotLabel = "customPlotLabel";
            viewModel.PlotValues.Execute(data);
            Assert.AreEqual("customPlotLabel", viewModel.CustomPlotLabel);
            Assert.IsFalse(viewModel.ShowComplexPlotToggle);
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
            Assert.AreEqual(1, viewModel.PlotModel.Series.Count);
            viewModel.PlotValues.Execute(_plotData);
            Assert.AreEqual(2, viewModel.PlotModel.Series.Count);
            viewModel.HoldOn = false;
            viewModel.PlotValues.Execute(_plotData);
            Assert.AreEqual(1, viewModel.PlotModel.Series.Count);
        }

        [Test]
        public void Verify_auto_scale_false()
        {
            var viewModel = new PlotViewModel();
            viewModel.PlotValues.Execute(_plotData);
            viewModel.AutoScaleX = true;
            viewModel.AutoScaleY = false;
            Assert.AreEqual(1, viewModel.PlotModel.Series.Count);
            Assert.AreEqual(0.5, viewModel.MinXValue);
            Assert.AreEqual(9.0, viewModel.MaxXValue);
            Assert.AreEqual(0.5, viewModel.MinYValue);
            Assert.AreEqual(9.0, viewModel.MaxYValue);
            viewModel.PlotValues.Execute(_plotData);
            viewModel.AutoScaleX = false;
            viewModel.AutoScaleY = true;
            Assert.AreEqual(0.5, viewModel.MinXValue);
            Assert.AreEqual(9.0, viewModel.MaxXValue);
            Assert.AreEqual(0.5, viewModel.MinYValue);
            Assert.AreEqual(9.0, viewModel.MaxYValue);
        }

        [Test]
        public void Verify_clone()
        {
            var viewModel = new PlotViewModel();
            var viewModelClone = viewModel.Clone();

            Assert.AreEqual(viewModel.Title, viewModelClone.Title);
            Assert.AreEqual(viewModel.PlotTitles.Count, viewModelClone.PlotTitles.Count);
            Assert.AreEqual(viewModel.PlotType, viewModelClone.PlotType);
            Assert.AreEqual(viewModel.HoldOn, viewModelClone.HoldOn);

            Assert.AreEqual(viewModel.Labels.Count, viewModelClone.Labels.Count);

            Assert.AreEqual(viewModel.CustomPlotLabel, viewModelClone.CustomPlotLabel);
            Assert.IsFalse(viewModelClone.ShowInPlotView);

            Assert.AreEqual(viewModel.HideKey, viewModelClone.HideKey);
            Assert.AreEqual(viewModel.ShowAxes, viewModelClone.ShowAxes);

            // when we clone a plot view model without any plots, the min and max values will be infinity
            Assert.AreEqual(double.PositiveInfinity, viewModelClone.MinXValue);
            Assert.AreEqual(double.NegativeInfinity, viewModelClone.MaxXValue);
            Assert.AreEqual(double.PositiveInfinity, viewModelClone.MinYValue);
            Assert.AreEqual(double.NegativeInfinity, viewModelClone.MaxYValue);

            Assert.AreEqual(viewModel.AutoScaleX, viewModelClone.AutoScaleX);
            Assert.AreEqual(viewModel.AutoScaleY, viewModelClone.AutoScaleY);

            Assert.AreEqual(viewModel.CurrentIndependentVariableAxis, viewModelClone.CurrentIndependentVariableAxis);

            Assert.IsTrue(viewModelClone.PlotNormalizationTypeOptionVm.Options[viewModelClone.PlotNormalizationTypeOptionVm.SelectedValue].IsSelected);
            Assert.IsTrue(viewModelClone.PlotToggleTypeOptionVm.Options[viewModelClone.PlotToggleTypeOptionVm.SelectedValue].IsSelected);
            Assert.IsTrue(viewModelClone.XAxisSpacingOptionVm.Options[viewModelClone.XAxisSpacingOptionVm.SelectedValue].IsSelected);
            Assert.IsTrue(viewModelClone.YAxisSpacingOptionVm.Options[viewModelClone.YAxisSpacingOptionVm.SelectedValue].IsSelected);
        }

        [Test]
        public void Verify_clone_with_plot_data()
        {
            var viewModelClone = _plotViewModel.Clone();

            Assert.AreEqual(_plotViewModel.Title, viewModelClone.Title);
            Assert.AreEqual(_plotViewModel.PlotTitles.Count, viewModelClone.PlotTitles.Count);
            Assert.AreEqual(_plotViewModel.PlotTitles[0], viewModelClone.PlotTitles[0]);
            Assert.AreEqual(_plotViewModel.PlotType, viewModelClone.PlotType);
            Assert.AreEqual(_plotViewModel.HoldOn, viewModelClone.HoldOn);

            Assert.AreEqual(_plotViewModel.Labels.Count, viewModelClone.Labels.Count);
            Assert.AreEqual(_plotViewModel.Labels[0], viewModelClone.Labels[0]);

            Assert.AreEqual(_plotViewModel.CustomPlotLabel, viewModelClone.CustomPlotLabel);
            Assert.IsFalse(viewModelClone.ShowInPlotView);

            Assert.AreEqual(_plotViewModel.HideKey, viewModelClone.HideKey);
            Assert.AreEqual(_plotViewModel.ShowAxes, viewModelClone.ShowAxes);

            Assert.AreEqual(_plotViewModel.MinXValue, viewModelClone.MinXValue);
            Assert.AreEqual(_plotViewModel.MaxXValue, viewModelClone.MaxXValue);
            Assert.AreEqual(_plotViewModel.MinYValue, viewModelClone.MinYValue);
            Assert.AreEqual(_plotViewModel.MaxYValue, viewModelClone.MaxYValue);

            Assert.AreEqual(_plotViewModel.AutoScaleX, viewModelClone.AutoScaleX);
            Assert.AreEqual(_plotViewModel.AutoScaleY, viewModelClone.AutoScaleY);

            Assert.AreEqual(_plotViewModel.CurrentIndependentVariableAxis, viewModelClone.CurrentIndependentVariableAxis);

            Assert.IsTrue(viewModelClone.PlotNormalizationTypeOptionVm.Options[viewModelClone.PlotNormalizationTypeOptionVm.SelectedValue].IsSelected);
            Assert.IsTrue(viewModelClone.PlotToggleTypeOptionVm.Options[viewModelClone.PlotToggleTypeOptionVm.SelectedValue].IsSelected);
            Assert.IsTrue(viewModelClone.XAxisSpacingOptionVm.Options[viewModelClone.XAxisSpacingOptionVm.SelectedValue].IsSelected);
            Assert.IsTrue(viewModelClone.YAxisSpacingOptionVm.Options[viewModelClone.YAxisSpacingOptionVm.SelectedValue].IsSelected);
        }

        [Test]
        public void Verify_manual_scales()
        {
            var viewModel = new PlotViewModel();
            Assert.IsFalse(viewModel.ManualScaleX);
            Assert.IsFalse(viewModel.ManualScaleY);
            viewModel.ManualScaleX = true;
            viewModel.ManualScaleY = true;
            Assert.IsTrue(viewModel.ManualScaleX);
            Assert.IsTrue(viewModel.ManualScaleY);
            Assert.IsFalse(viewModel.AutoScaleX);
            Assert.IsFalse(viewModel.AutoScaleY);
        }

        [Test]
        public void Verify_clear_plot_single()
        {
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(_plotData);
            plotViewModel.PlotValues.Execute(_plotData);
            Assert.AreEqual(2, plotViewModel.PlotModel.Series.Count);
            plotViewModel.ClearPlotSingleCommand.Execute(null);
            Assert.AreEqual(1, plotViewModel.PlotModel.Series.Count);
        }

        [Test]
        public void Verify_clear_plot_all()
        {
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(_plotData);
            plotViewModel.PlotValues.Execute(_plotData);
            Assert.AreEqual(2, plotViewModel.PlotModel.Series.Count);
            plotViewModel.ClearPlotCommand.Execute(null);
            Assert.AreEqual(0, plotViewModel.PlotModel.Series.Count);
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
            Assert.AreEqual(2, plotViewModel.PlotModel.Series.Count);
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = PlotToggleType.Phase;
            Assert.AreEqual(1, plotViewModel.PlotModel.Series.Count);
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = PlotToggleType.Amp;
            Assert.AreEqual(1, plotViewModel.PlotModel.Series.Count);
            plotViewModel.PlotToggleTypeOptionVm.SelectedValue = PlotToggleType.Complex;
            Assert.AreEqual(2, plotViewModel.PlotModel.Series.Count);
            // add in value verification?
        }

        [Test]
        public void Verify_max_normalization()
        {
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(_plotData);
            plotViewModel.PlotValues.Execute(_plotData);
            Assert.AreEqual(2, plotViewModel.PlotModel.Series.Count);
            plotViewModel.PlotNormalizationTypeOptionVm.SelectedValue = PlotNormalizationType.RelativeToMax;
            var plotSeries = plotViewModel.PlotSeriesCollection[0];
            var i = 0;
            foreach (var point in plotSeries)
            {
                Assert.IsInstanceOf<Point>(point);
                i++;
            }
            Assert.AreEqual(10, i);
        }

        [Test]
        public void Verify_curve_normalization()
        {
            var windowViewModel = new WindowViewModel();
            var plotViewModel = windowViewModel.PlotVM;
            plotViewModel.PlotValues.Execute(_plotData);
            plotViewModel.PlotValues.Execute(_plotData);
            Assert.AreEqual(2, plotViewModel.PlotModel.Series.Count);
            plotViewModel.PlotNormalizationTypeOptionVm.SelectedValue = PlotNormalizationType.RelativeToCurve;
            var plotSeries = plotViewModel.PlotSeriesCollection[0];
            var i = 0;
            foreach (var point in plotSeries)
            {
                Assert.IsInstanceOf<Point>(point);
                i++;
            }
            Assert.AreEqual(10, i);
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
            Assert.AreEqual(toggleType == PlotToggleType.Complex ? 4 : 2, plotViewModel.PlotModel.Series.Count);
            plotViewModel.PlotNormalizationTypeOptionVm.SelectedValue = PlotNormalizationType.RelativeToMax;
            var plotSeries = plotViewModel.PlotSeriesCollection[0];
            var i = 0;
            foreach (var point in plotSeries)
            {
                Assert.IsInstanceOf<Point>(point);
                i++;
            }
            Assert.AreEqual(10, i);
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
            Assert.AreEqual(4, plotViewModel.PlotModel.Series.Count);
            plotViewModel.PlotNormalizationTypeOptionVm.SelectedValue = PlotNormalizationType.RelativeToCurve;
            var plotSeries = plotViewModel.PlotSeriesCollection[0];
            var i = 0;
            foreach (var point in plotSeries)
            {
                Assert.IsInstanceOf<Point>(point);
                i++;
            }
            Assert.AreEqual(10, i);
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
            // can't call WindowViewModel because not fixed yet
            //var windowViewModel = new WindowViewModel();
            //var plotViewModel = windowViewModel.PlotVM;
            //plotViewModel.PlotValues.Execute(_plotData);

            // mock the IOpenFileDialog
            var openFileDialogMock = Substitute.For<PlotViewModel.IOpenFileDialog>();
            openFileDialogMock.FileName.Returns(exportFilename);
            openFileDialogMock.ShowDialog().Returns(true);
            // mock IFileSystem
            var fileSystemMock = Substitute.For<PlotViewModel.IFileSystem>();
            fileSystemMock.WriteExportedData(exportFilename, Encoding.UTF8);

            _plotViewModel = new PlotViewModel(0, openFileDialogMock, fileSystemMock);
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
                Assert.AreEqual(t.X.ToString(CultureInfo.InvariantCulture), data[0]);
                // check y value
                Assert.AreEqual(t.Y.ToString(CultureInfo.InvariantCulture), data[1]);
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
            var openFileDialogMock = Substitute.For<PlotViewModel.IOpenFileDialog>();
            openFileDialogMock.FileName.Returns(exportFilename);
            openFileDialogMock.ShowDialog().Returns(true);
            // mock IFileSystem
            var fileSystemMock = Substitute.For<PlotViewModel.IFileSystem>();
            fileSystemMock.WriteExportedData(exportFilename, Encoding.UTF8);

            _plotViewModel = new PlotViewModel(0, openFileDialogMock, fileSystemMock);
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
                Assert.AreEqual(t.X.ToString(CultureInfo.InvariantCulture), data[0]);
                // check real value
                Assert.AreEqual(t.Y.Real.ToString(CultureInfo.InvariantCulture), data[1]);
                // check imaginary value
                Assert.AreEqual(t.Y.Imaginary.ToString(CultureInfo.InvariantCulture), data[1]);

            }
        }

    }
}