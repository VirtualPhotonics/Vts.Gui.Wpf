using System;
using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels
{
    /// <summary>
    /// Summary description for InverseSolverViewModelTests
    /// </summary>
    [TestFixture]
    public class InverseSolverViewModelTests
    {
        /// <summary>
        /// Verifies that InverseSolverViewModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            // WindowViewModel needs to be instantiated for default constructor
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.InverseSolverVM;
            Assert.IsTrue(viewModel.SolutionDomainTypeOptionVM != null);
            Assert.IsTrue(viewModel.MeasuredForwardSolverTypeOptionVM != null);
            Assert.IsTrue(viewModel.InverseForwardSolverTypeOptionVM != null);
            Assert.IsTrue(viewModel.InverseFitTypeOptionVM != null);
            Assert.IsTrue(viewModel.OptimizerTypeOptionVM != null);
            Assert.IsTrue(viewModel.MeasuredOpticalPropertyVM != null);
            Assert.IsTrue(viewModel.InitialGuessOpticalPropertyVM != null);
            Assert.IsTrue(viewModel.ResultOpticalPropertyVM != null);
            Assert.IsTrue(viewModel.AllRangeVMs != null);
            Assert.IsFalse(viewModel.SolutionDomainTypeOptionVM.EnableMultiAxis);
            Assert.IsFalse(viewModel.SolutionDomainTypeOptionVM.AllowMultiAxis);
            Assert.IsTrue(viewModel.SolutionDomainTypeOptionVM.SelectedValue == SolutionDomainType.ROfRho);
            Assert.IsTrue(viewModel.AllRangeVMs.Length == 1);
            Assert.IsTrue(Math.Abs(viewModel.AllRangeVMs[0].Start - 1) < 1e-6);
            Assert.IsTrue(Math.Abs(viewModel.AllRangeVMs[0].Stop - 6) < 1e-6);
            Assert.AreEqual(viewModel.AllRangeVMs[0].Number, 60);
        }

        // The following tests verify the Relay Commands
        /// <summary>
        /// Verifies that all commands, SimulateMeasuredCommand, CalculateInitialGuessCommand,
        /// and SolveInverseCommand returns correct values
        /// </summary>
        [Test]
        public void verify_SimulateMeasuredCommand_returns_correct_values()
        {
            // WindowViewModel needs to be instantiated for default constructor
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.InverseSolverVM;
            viewModel.MeasuredForwardSolverTypeOptionVM.SelectedValue = ForwardSolverType.Nurbs;
            viewModel.InverseForwardSolverTypeOptionVM.SelectedValue = ForwardSolverType.PointSourceSDA;
            viewModel.SolutionDomainTypeOptionVM.SelectedValue = SolutionDomainType.ROfRho;
            viewModel.AllRangeVMs[0].Start = 1.0;
            viewModel.AllRangeVMs[0].Stop = 10.0;
            viewModel.AllRangeVMs[0].Number = 10;
            //viewModel.AllRangeVMs<IndependentVariableAxis>.
            viewModel.PercentNoise = 0;
            // SimulateMeasuredDataCommand
            viewModel.SimulateMeasuredDataCommand.Execute(null);
            PlotViewModel plotViewModel = windowViewModel.PlotVM;
            Assert.AreEqual(plotViewModel.Labels[0], "\nSimulated:\rModel - nurbs\rμa=0.0100 \rμs'=1.0000");
            Assert.AreEqual(plotViewModel.Title, "R(ρ) [mm-2] versus ρ [mm]");
            TextOutputViewModel textOutputViewModel = windowViewModel.TextOutputVM;
            Assert.AreEqual(textOutputViewModel.Text, "Simulated Measured Data: μa=0.01 μs'=1 g=0.8 n=1.4; Units = 1/mm\r");
            // CalculateInitialGuessCommand
            viewModel.CalculateInitialGuessCommand.Execute(null);
            Assert.AreEqual(plotViewModel.Labels[1], "\nGuess:\rModel - SDA\rμa=0.0100 \rμs'=1.0000");
            Assert.AreEqual(textOutputViewModel.Text,
                "Simulated Measured Data: μa=0.01 μs'=1 g=0.8 n=1.4; Units = 1/mm\rInitial Guess: μa=0.01 μs'=1 g=0.8 n=1.4; Units = 1/mm \r");
            // SolveInverseCommand
            viewModel.SolveInverseCommand.Execute(null);
            Assert.AreEqual(plotViewModel.Labels[2], "\nCalculated:\rModel - nurbs\rμa=0.0129 \rμs'=0.9255");
            Assert.AreEqual(textOutputViewModel.Text,
                "Simulated Measured Data: μa=0.01 μs'=1 g=0.8 n=1.4; Units = 1/mm\rInitial Guess: μa=0.01 μs'=1 g=0.8 n=1.4; Units = 1/mm \rInverse Solution Results: \r   Optimization parameter(s): MuaMusp \r   Initial Guess: μa=0.01 μs'=1 g=0.8 n=1.4; Units = 1/mm \r   Exact: μa=0.01 μs'=1 g=0.8 n=1.4; Units = 1/mm \r   At Converged Values: μa=0.0129 μs'=0.9255 g=0.8 n=1.4; Units = 1/mm \r   Percent Error: μa = 28.9%  μs' = 7.45% \r");
        }
    }
}
