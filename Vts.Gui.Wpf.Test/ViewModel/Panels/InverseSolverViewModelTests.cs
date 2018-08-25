using System;
using System.Globalization;
using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
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
            double d1 = 0.01;
            int i1 = 1;
            double g = 0.8;
            double n = 1.4;
            double d2 = 0.0100;
            double d3 = 1.0000;
            double d4 = 0.0129;
            double d5 = 0.9255;
            double muaError = 28.9;
            double muspError = 7.45;
            var op1 = StringLookup.GetLocalizedString("Label_MuA") + "=" +
                     d1.ToString(CultureInfo.CurrentCulture) + " " +
                     StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                     i1.ToString(CultureInfo.CurrentCulture) + " g=" +
                     g.ToString(CultureInfo.CurrentCulture) + " n=" +
                     n.ToString(CultureInfo.CurrentCulture) + "; " +
                     StringLookup.GetLocalizedString("Label_Units") + " = 1/mm \r";
            var op2 = StringLookup.GetLocalizedString("Label_MuA") + "=" +
                      d4.ToString(CultureInfo.CurrentCulture) + " " +
                      StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                      d5.ToString(CultureInfo.CurrentCulture) + " g=" +
                      g.ToString(CultureInfo.CurrentCulture) + " n=" +
                      n.ToString(CultureInfo.CurrentCulture) + "; " +
                      StringLookup.GetLocalizedString("Label_Units") + " = 1/mm \r";
            var s2 = StringLookup.GetLocalizedString("Label_ROfRho") + " [mm-2] " +
                     StringLookup.GetLocalizedString("Label_Versus") + " ρ [mm]";
            var s3 = "\n" + StringLookup.GetLocalizedString("Label_Simulated") + "\r" +
                     StringLookup.GetLocalizedString("Label_ModelNurbs") + "\r" +
                     StringLookup.GetLocalizedString("Label_MuA") + "=" +
                     d2.ToString("N4", CultureInfo.CurrentCulture) + " \r" +
                     StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                     d3.ToString("N4", CultureInfo.CurrentCulture);
            var s4 = "\n" + StringLookup.GetLocalizedString("Label_Guess") + "\r" +
                     StringLookup.GetLocalizedString("Label_ModelSDA") + "\r" +
                     StringLookup.GetLocalizedString("Label_MuA") + "=" +
                     d2.ToString("N4", CultureInfo.CurrentCulture) + " \r" +
                     StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                     d3.ToString("N4", CultureInfo.CurrentCulture);
            var s5 = "\n" + StringLookup.GetLocalizedString("Label_Calculated") + "\r" +
                     StringLookup.GetLocalizedString("Label_ModelNurbs") + "\r" +
                     StringLookup.GetLocalizedString("Label_MuA") + "=" +
                     d4.ToString("N4", CultureInfo.CurrentCulture) + " \r" +
                     StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                     d5.ToString("N4", CultureInfo.CurrentCulture);
            var s6 = StringLookup.GetLocalizedString("Label_SimulatedMeasuredData") + op1;
            var s7 = StringLookup.GetLocalizedString("Label_InitialGuess") + op1;
            var s8 = StringLookup.GetLocalizedString("Label_InverseSolutionResults") + "\r   " +
                     StringLookup.GetLocalizedString("Label_OptimizationParameter") + "MuaMusp \r   " +
                     s7 + "   " + StringLookup.GetLocalizedString("Label_Exact") + ": " + op1 +
                     "   " + StringLookup.GetLocalizedString("Label_ConvergedValues") + ": " + op2 +
                     "   " + StringLookup.GetLocalizedString("Label_PercentError") + 
                     StringLookup.GetLocalizedString("Label_MuA") + " = " + 
                     muaError.ToString(CultureInfo.CurrentCulture) + "%  " + 
                     StringLookup.GetLocalizedString("Label_MuSPrime") + " = " + 
                     muspError.ToString(CultureInfo.CurrentCulture) + "% \r";
            Assert.AreEqual(plotViewModel.Labels[0], s3);
            Assert.AreEqual(plotViewModel.Title, s2);
            TextOutputViewModel textOutputViewModel = windowViewModel.TextOutputVM;
            Assert.AreEqual(textOutputViewModel.Text, s6);
            // CalculateInitialGuessCommand
            viewModel.CalculateInitialGuessCommand.Execute(null);
            Assert.AreEqual(plotViewModel.Labels[1], s4);
            Assert.AreEqual(textOutputViewModel.Text, s6 + s7);
            // SolveInverseCommand
            viewModel.SolveInverseCommand.Execute(null);
            Assert.AreEqual(plotViewModel.Labels[2], s5);
            Assert.AreEqual(textOutputViewModel.Text, s6 + s7 + s8);
        }
    }
}
