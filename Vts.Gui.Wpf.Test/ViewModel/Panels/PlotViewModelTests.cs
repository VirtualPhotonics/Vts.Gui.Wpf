using NUnit.Framework;
using OxyPlot;
// todo: Once the popout bug is fixed and we update OxyPlot, uncomment this using:
//using OxyPlot.Legends;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels
{
    /// <summary>
    /// Summary description for PlotViewModelTests
    /// </summary>
    [TestFixture]
    public class PlotViewModelTests
    {
        /// <summary>
        /// setup and tear down
        /// </summary>
        [OneTimeSetUp]
        public void setup()
        {
            //clear_folders_and_files();

        }

        [OneTimeTearDown]
        public void clear_folders_and_files()
        {
            //foreach (var folder in listOfInfileFolders)
            //{
            //    if (Directory.Exists(folder))
            //    {
            //        Directory.Delete(folder);
            //    }
            //}

        }

        /// <summary>
        /// Verifies that PlotViewModel default constructor 
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new PlotViewModel();
            Assert.IsTrue(viewModel.XAxisSpacingOptionVm != null);
            Assert.IsTrue(viewModel.YAxisSpacingOptionVm != null);
            Assert.IsTrue(viewModel.PlotToggleTypeOptionVm != null);
            Assert.IsTrue(viewModel.PlotNormalizationTypeOptionVm != null);
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
        public void verify_ClearPlotCommand_returns_correct_values()
        {
            var viewModel = new PlotViewModel();
            viewModel.ClearPlotCommand.Execute(null);
            // ClearPlot public settings
            Assert.AreEqual(viewModel.Labels.Count, 0);
            Assert.AreEqual(viewModel.Title, null);
            // UpdatePlotSeries settings
            Assert.AreEqual(viewModel.PlotModel.Series.Count, 0);
            Assert.AreEqual(viewModel.PlotSeriesCollection.Count, 0);
            Assert.IsFalse(viewModel.ShowComplexPlotToggle);
            Assert.IsTrue(viewModel.PlotModel.IsLegendVisible);
        }

        /// <summary>
        /// Verifies that the SetAxisLabelsCommand returns correct values
        /// </summary>
        [Test]
        public void verify_SetAxesLabelsCommand_returns_correct_values()
        {
            var viewModel = new PlotViewModel();
            viewModel.SetAxesLabels.Execute(null);
            // the following validation values were determined by prior running of unit test
            Assert.AreEqual(viewModel.CurrentIndependentVariableAxis, IndependentVariableAxis.Rho);
            Assert.AreEqual(viewModel.Title, null);
        }

        // The following tests verify the Relay Commands
        /// <summary>
        /// Verifies that the PlotValues command returns correct values
        /// </summary>
        [Test]
        public void verify_PlotValues_command_returns_correct_values()
        {
            var viewModel = new PlotViewModel();
            // CalculateMinMax settings
            Assert.AreEqual(viewModel.MinXValue, 1e-9);
            Assert.AreEqual(viewModel.MaxXValue, 1.0);
            Assert.AreEqual(viewModel.MinYValue, 1e-9);
            Assert.AreEqual(viewModel.MaxXValue, 1.0);
            // AddValuesToPlot settings 
            // Note cannot validate plotted data because DataSeriesCollection is private
            var data = new double[,] { { 1.0, 2.0 }, { 3.0, 4.0 } };
            viewModel.CustomPlotLabel = "customPlotLabel";
            viewModel.PlotValues.Execute(data);
            Assert.AreEqual(viewModel.CustomPlotLabel, "customPlotLabel");
            Assert.AreEqual(viewModel.ShowComplexPlotToggle, false);
        }

        /// <summary>
        /// ClearPlotSingleCommand changes DataSeriesCollection which is private so not tested
        /// ExportDataToTextCommand brings up Dialog window so not tested
        /// DuplicateWindowCommand - not sure if can test 
        /// </summary>

    }
}