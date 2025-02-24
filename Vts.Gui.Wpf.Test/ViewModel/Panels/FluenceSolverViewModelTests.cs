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
            Assert.That(viewModel.ForwardSolverTypeOptionVM != null, Is.True);
            Assert.That(viewModel.AbsorbedEnergySolutionDomainTypeOptionVM != null, Is.True);
            Assert.That(viewModel.FluenceSolutionDomainTypeOptionVM != null, Is.True);
            Assert.That(viewModel.PhotonHittingDensitySolutionDomainTypeOptionVM != null, Is.True);
            Assert.That(viewModel.MapTypeOptionVM != null, Is.True);
            Assert.That(viewModel.TissueInputVM != null, Is.True);
            Assert.That(viewModel.RhoRangeVM != null, Is.True);
            Assert.That(viewModel.ZRangeVM != null, Is.True);
            // default settings
            Assert.That(viewModel.FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled, Is.False);
            Assert.That(viewModel.FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled, Is.True);
            Assert.That(viewModel.FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled, Is.True);
            Assert.That(viewModel.AbsorbedEnergySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled, Is.False);
            Assert.That(viewModel.AbsorbedEnergySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled, Is.True);
            Assert.That(viewModel.PhotonHittingDensitySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled, Is.False);
            Assert.That(viewModel.PhotonHittingDensitySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled, Is.True);
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
            Assert.That(plotViewModel.Labels.Count, Is.EqualTo(0));
            Assert.That(plotViewModel.Title, Is.Null);
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
            Assert.That(textOutputViewModel.Text, Is.EqualTo(s1));
        }
    }
}
