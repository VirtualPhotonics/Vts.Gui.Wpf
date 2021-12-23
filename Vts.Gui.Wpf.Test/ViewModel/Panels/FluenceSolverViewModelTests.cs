using System.Globalization;
using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
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
        public void Verify_default_constructor_sets_properties_correctly()
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
        public void Verify_ExecuteFluenceSolverCommand_returns_correct_values()
        {
            // WindowViewModel needs to be instantiated for default constructor
            var windowViewModel = new WindowViewModel();
            var viewModel = windowViewModel.FluenceSolverVM;
            viewModel.ForwardSolverTypeOptionVM.SelectedValue = ForwardSolverType.PointSourceSDA;
            viewModel.FluenceSolutionDomainTypeOptionVM.SelectedValue = FluenceSolutionDomainType.FluenceOfRhoAndZ;
            var result = viewModel.GetMapData();
            result.Wait();
            // ExecuteForwardSolver default settings
            var plotViewModel = windowViewModel.PlotVM;
            Assert.AreEqual(0, plotViewModel.Labels.Count);
            Assert.IsNull(plotViewModel.Title);
            var textOutputViewModel = windowViewModel.TextOutputVM;
            const double d1 = 0.01;
            const int i1 = 1;
            const double g = 0.8;
            const double n = 1.4;
            var s1 = StringLookup.GetLocalizedString("Label_FluenceSolver") +
                     StringLookup.GetLocalizedString("Label_MuA") + "=" +
                     d1.ToString(CultureInfo.CurrentCulture) + " " +
                     StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                     i1.ToString(CultureInfo.CurrentCulture) + " g=" +
                     g.ToString(CultureInfo.CurrentCulture) + " n=" +
                     n.ToString(CultureInfo.CurrentCulture) + "; " +
                     StringLookup.GetLocalizedString("Label_Units") + " = 1/mm\r";
            Assert.AreEqual(s1, textOutputViewModel.Text);
        }
    }
}
