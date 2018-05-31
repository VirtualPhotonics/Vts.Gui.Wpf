using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels
{
    /// <summary>
    /// Summary description for FluenceSolverViewModelTests
    /// </summary>
    [TestFixture]
    public class FluenceSolverViewModelTests
    {
        /// <summary>
        /// Verifies that FluenceSolverViewModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            // WindowViewModel needs to be instantiated for default constructor
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.FluenceSolverVM;
            Assert.IsTrue(viewModel.ForwardSolverTypeOptionVM != null);
            Assert.IsTrue(viewModel.AbsorbedEnergySolutionDomainTypeOptionVM != null);
            Assert.IsTrue(viewModel.FluenceSolutionDomainTypeOptionVM != null);
            Assert.IsTrue(viewModel.PhotonHittingDensitySolutionDomainTypeOptionVM != null);
            Assert.IsTrue(viewModel.MapTypeOptionVM != null);
            Assert.IsTrue(viewModel.TissueInputVM != null);
            Assert.IsTrue(viewModel.RhoRangeVM != null);
            Assert.IsTrue(viewModel.ZRangeVM != null);
            // default settings
            Assert.IsFalse(viewModel.FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled);
            Assert.IsTrue(viewModel.FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled);
            Assert.IsTrue(viewModel.FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled);
            Assert.IsFalse(viewModel.AbsorbedEnergySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled);
            Assert.IsTrue(viewModel.AbsorbedEnergySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled);
            Assert.IsFalse(viewModel.PhotonHittingDensitySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled);
            Assert.IsTrue(viewModel.PhotonHittingDensitySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled);
        }

        // The following tests verify the Relay Commands
        /// <summary>
        /// Verifies that the ExecuteFluenceSolverCommand returns correct values
        /// </summary>
        [Test]
        public void verify_ExecuteFluenceSolverCommand_returns_correct_values()
        {
            // WindowViewModel needs to be instantiated for default constructor
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.FluenceSolverVM;
            viewModel.ForwardSolverTypeOptionVM.SelectedValue = ForwardSolverType.PointSourceSDA;
            viewModel.FluenceSolutionDomainTypeOptionVM.SelectedValue = FluenceSolutionDomainType.FluenceOfRhoAndZ;
            viewModel.ExecuteFluenceSolverCommand.Execute(null);
            // ExecuteForwardSolver default settings
            PlotViewModel plotViewModel = windowViewModel.PlotVM;
            Assert.AreEqual(plotViewModel.Labels.Count, 0);
            Assert.AreEqual(plotViewModel.Title, null);
            TextOutputViewModel textOutputViewModel = windowViewModel.TextOutputVM;
            Assert.AreEqual(textOutputViewModel.Text, "Fluence Solver: μa=0.01 μs'=1 g=0.8 n=1.4; Units = 1/mm\r");
        }
    }
}
