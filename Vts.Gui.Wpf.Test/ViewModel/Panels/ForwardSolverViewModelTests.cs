using System.Globalization;
using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels
{
    /// <summary>
    /// Summary description for ForwardSolverViewModelTests
    /// </summary>
    [TestFixture]
    public class ForwardSolverViewModelTests
    {
        /// <summary>
        /// Verifies that ForwardSolverViewModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            // WindowViewModel needs to be instantiated for default constructor
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.ForwardSolverVM;
            Assert.IsTrue(viewModel.ForwardSolverTypeOptionVM != null);
            Assert.IsTrue(viewModel.SolutionDomainTypeOptionVM != null);
            Assert.IsTrue(viewModel.ForwardAnalysisTypeOptionVM != null);
        }

        // The following tests verify the Relay Commands
        /// <summary>
        /// Verifies that the ExecuteForwardSolverCommand returns correct values
        /// </summary>
        [Test]
        public void verify_ExecuteForwardSolverCommand_returns_correct_values()
        {
            // WindowViewModel needs to be instantiated for default constructor
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.ForwardSolverVM;
            viewModel.ForwardSolverTypeOptionVM.SelectedValue = ForwardSolverType.PointSourceSDA;
            viewModel.SolutionDomainTypeOptionVM.SelectedValue = SolutionDomainType.ROfRho;
            viewModel.ExecuteForwardSolverCommand.Execute(null);
            // ExecuteForwardSolver default settings
            PlotViewModel plotViewModel = windowViewModel.PlotVM;
            double d1 = 0.01;
            int i1 = 1;
            double g = 0.8;
            double n = 1.4;
            double d2 = 0.0100;
            double d3 = 1.0000;
            var s1 = StringLookup.GetLocalizedString("Label_ForwardSolver") +
                     StringLookup.GetLocalizedString("Label_MuA") + "=" +
                     d1.ToString(CultureInfo.CurrentCulture) + " " +
                     StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                     i1.ToString(CultureInfo.CurrentCulture) + " g=" +
                     g.ToString(CultureInfo.CurrentCulture) + " n=" +
                     n.ToString(CultureInfo.CurrentCulture) + "; " +
                     StringLookup.GetLocalizedString("Label_Units") + " = 1/mm\r";
            var s2 = StringLookup.GetLocalizedString("Label_ROfRho") + " [mm-2] " +
                     StringLookup.GetLocalizedString("Label_Versus") + " ρ [mm]";
            var s3 = "\r" + StringLookup.GetLocalizedString("Label_ModelSDA") + "\r" +
                     StringLookup.GetLocalizedString("Label_MuA") + "=" +
                     d2.ToString("N4", CultureInfo.CurrentCulture) + " \r" +
                     StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                     d3.ToString("N4", CultureInfo.CurrentCulture);
            Assert.AreEqual(plotViewModel.Labels[0], s3);
            Assert.AreEqual(plotViewModel.Title, s2);
            TextOutputViewModel textOutputViewModel = windowViewModel.TextOutputVM;
            Assert.AreEqual(textOutputViewModel.Text, s1);
        }
        /// <summary>
        /// Verifies that ForwardSolverViewModel disallows spectral panel inputs for TwoLayerSDA selection
        /// </summary>
        [Test]
        public void verify_TwoLayerSDA_selection_does_not_display_usespectralpanelinputs_checkbox()
        {
            // WindowViewModel needs to be instantiated for default constructor
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.ForwardSolverVM;
            // set forward solver to TwoLayerSDA
            viewModel.ForwardSolverTypeOptionVM.SelectedValue = ForwardSolverType.TwoLayerSDA;
            Assert.IsFalse(viewModel.SolutionDomainTypeOptionVM.AllowMultiAxis);
            Assert.IsFalse(viewModel.SolutionDomainTypeOptionVM.UseSpectralInputs);
            Assert.IsFalse(viewModel.SolutionDomainTypeOptionVM.EnableMultiAxis);
            Assert.IsFalse(viewModel.SolutionDomainTypeOptionVM.EnableSpectralPanelInputs);
        }
        /// <summary>
        /// Verifies that ForwardSolverViewModel disallows time-dependent solution domain options
        /// for DistributedGaussianSourceSDA selection
        /// </summary>
        [Test]
        public void verify_DistributedGaussianSourceSDA_selection_does_not_display_time_dependent_solution_domain_options()
        {
            // WindowViewModel needs to be instantiated for default constructor
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.ForwardSolverVM;
            // set forward solver to DistributedGaussianSourceSDA
            viewModel.ForwardSolverTypeOptionVM.SelectedValue = ForwardSolverType.DistributedGaussianSourceSDA;

            Assert.IsFalse(viewModel.SolutionDomainTypeOptionVM.IsROfRhoAndTimeEnabled);
            Assert.IsFalse(viewModel.SolutionDomainTypeOptionVM.IsROfRhoAndFtEnabled);
            Assert.IsFalse(viewModel.SolutionDomainTypeOptionVM.IsROfFxAndTimeEnabled);
            Assert.IsFalse(viewModel.SolutionDomainTypeOptionVM.IsROfFxAndFtEnabled);
        }
    }
}
