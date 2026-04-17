using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel.Panels.MonteCarlo;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo;

/// <summary>
/// Summary description for SimulationOptionsViewModelTests
/// </summary>
[TestFixture]
public class SimulationOptionsViewModelTests
{
    /// <summary>
    /// Verifies that SimulationOptionsModel default constructor instantiates sub view models
    /// </summary>
    [Test]
    public void Verify_default_constructor_sets_properties_correctly()
    {
        var viewModel = new SimulationOptionsViewModel();
        Assert.That(viewModel.AbsorptionWeightingTypeVm != null, Is.True);
        Assert.That(viewModel.PhaseFunctionTypeVm != null, Is.True);
        Assert.That(viewModel.RandomNumberGeneratorTypeVm != null, Is.True);
        Assert.That(viewModel.RandomNumberGeneratorTypeVm != null, Is.True);
    }
}
