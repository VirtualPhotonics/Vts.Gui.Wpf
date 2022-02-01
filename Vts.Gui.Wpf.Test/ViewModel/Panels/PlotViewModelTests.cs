using System.Collections.Generic;
using System.Windows;
using NUnit.Framework;
using OxyPlot;
using Vts.Gui.Wpf.Model;
// todo: Once the popout bug is fixed and we update OxyPlot, uncomment this using:
//using OxyPlot.Legends;
using Vts.Gui.Wpf.ViewModel;

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
            Assert.AreEqual(viewModel.PlotModel.LegendPlacement, LegendPlacement.Outside);
            // todo: Once the popout bug is fixed and we update OxyPlot, replace the above line with this one:
            //Assert.AreEqual(viewModel.PlotModel.Legends[0].LegendPlacement, LegendPlacement.Outside);
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
            viewModel.SetAxesLabels.Execute(null);
            // the following validation values were determined by prior running of unit test
            Assert.AreEqual(IndependentVariableAxis.Rho, viewModel.CurrentIndependentVariableAxis);
            Assert.IsNull(viewModel.Title);
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
            var data = new double[,] { { 1.0, 2.0 }, { 3.0, 4.0 } };
            viewModel.CustomPlotLabel = "customPlotLabel";
            viewModel.PlotValues.Execute(data);
            Assert.AreEqual("customPlotLabel", viewModel.CustomPlotLabel);
            Assert.IsFalse(viewModel.ShowComplexPlotToggle);
        }

        [Test]
        public void Verify_clone()
        {
            var viewModel = new PlotViewModel();
            var viewModelClone = viewModel.Clone();
            // when we clone a plot view model without any plots, the min and max values will be infinity
            Assert.AreEqual(double.PositiveInfinity, viewModelClone.MinXValue);
            Assert.AreEqual(double.NegativeInfinity, viewModelClone.MaxXValue);
            Assert.AreEqual(double.PositiveInfinity, viewModelClone.MinYValue);
            Assert.AreEqual(double.NegativeInfinity, viewModelClone.MaxYValue);
            Assert.AreEqual(viewModel.CustomPlotLabel, viewModelClone.CustomPlotLabel);
            Assert.AreEqual(viewModel.PlotType, viewModelClone.PlotType);
            Assert.AreEqual(viewModel.PlotModel.Title, viewModelClone.PlotModel.Title);
            Assert.AreEqual(viewModel.PlotModel.LegendPlacement, viewModelClone.PlotModel.LegendPlacement);
        }

        /// <summary>
        /// ClearPlotSingleCommand changes DataSeriesCollection which is private so not tested
        /// ExportDataToTextCommand brings up Dialog window so not tested
        /// DuplicateWindowCommand - not sure if can test 
        /// </summary>

    }
}