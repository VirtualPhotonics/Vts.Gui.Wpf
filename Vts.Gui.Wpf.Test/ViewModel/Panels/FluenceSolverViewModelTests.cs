using System.Globalization;
using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels;

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
        var viewModel = windowViewModel.FluenceSolverVm;
        Assert.That(viewModel.ForwardSolverTypeOptionVm != null, Is.True);
        Assert.That(viewModel.AbsorbedEnergySolutionDomainTypeOptionVm != null, Is.True);
        Assert.That(viewModel.FluenceSolutionDomainTypeOptionVm != null, Is.True);
        Assert.That(viewModel.PhotonHittingDensitySolutionDomainTypeOptionVm != null, Is.True);
        Assert.That(viewModel.MapTypeOptionVm != null, Is.True);
        Assert.That(viewModel.TissueInputVm != null, Is.True);
        Assert.That(viewModel.RhoRangeVm != null, Is.True);
        Assert.That(viewModel.ZRangeVm != null, Is.True);
        // default settings
        Assert.That(viewModel.FluenceSolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndTimeEnabled, Is.False);
        Assert.That(viewModel.FluenceSolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndFtEnabled, Is.True);
        Assert.That(viewModel.FluenceSolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndFtEnabled, Is.True);
        Assert.That(viewModel.AbsorbedEnergySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndTimeEnabled, Is.False);
        Assert.That(viewModel.AbsorbedEnergySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndFtEnabled, Is.True);
        Assert.That(viewModel.PhotonHittingDensitySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndTimeEnabled, Is.False);
        Assert.That(viewModel.PhotonHittingDensitySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndFtEnabled, Is.True);
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
        var viewModel = windowViewModel.FluenceSolverVm;
        viewModel.ForwardSolverTypeOptionVm.SelectedValue = ForwardSolverType.PointSourceSDA;
        viewModel.FluenceSolutionDomainTypeOptionVm.SelectedValue = FluenceSolutionDomainType.FluenceOfRhoAndZ;
        var result = viewModel.GetMapData();
        result.Wait();
        // ExecuteForwardSolver default settings
        var plotViewModel = windowViewModel.PlotVm;
        Assert.That(plotViewModel.Labels.Count, Is.EqualTo(0));
        Assert.That(plotViewModel.Title, Is.Null);
        var textOutputViewModel = windowViewModel.TextOutputVm;
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
