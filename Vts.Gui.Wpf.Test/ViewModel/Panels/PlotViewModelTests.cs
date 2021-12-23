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

        /// <summary>
        /// ClearPlotSingleCommand changes DataSeriesCollection which is private so not tested
        /// ExportDataToTextCommand brings up Dialog window so not tested
        /// DuplicateWindowCommand - not sure if can test 
        /// </summary>

    }
}