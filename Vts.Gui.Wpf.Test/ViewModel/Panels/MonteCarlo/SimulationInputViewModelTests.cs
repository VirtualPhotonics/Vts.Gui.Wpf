using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel.Panels.MonteCarlo;
using Vts.MonteCarlo;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo;

/// <summary>
/// Summary description for SimulationInputViewModelTests
/// </summary>
[TestFixture]
public class SimulationInputViewModelTests
{
    /// <summary>
    /// Verifies that SimulationInputModel default constructor instantiates sub View Models
    /// </summary>
    [Test]
    public void Verify_default_constructor_sets_properties_correctly()
    {
        var viewModel = new SimulationInputViewModel();
        Assert.That(viewModel.SimulationOptionsVm != null, Is.True);
        Assert.That(viewModel.TissueInputVm != null, Is.True);
        Assert.That(viewModel.TissueTypeVm != null, Is.True);
    }

    [Test]
    public void Verify_set_simulation_input()
    {
        var viewModel = new SimulationInputViewModel();
        var input = SimulationInputProvider.PointSourceOneLayerTissueAllDetectors();
        viewModel.SimulationInput = input;
        Assert.That(viewModel.SimulationOptionsVm != null, Is.True);
        Assert.That(viewModel.TissueInputVm != null, Is.True);
        Assert.That(viewModel.TissueTypeVm != null, Is.True);
        Assert.That(viewModel.SimulationInput, Is.EqualTo(input));
        Assert.That(viewModel.N, Is.EqualTo(input.N));
    }

    [Test]
    public void Verify_constructor_with_input_sets_properties_correctly()
    {
        var input = SimulationInputProvider.PointSourceOneLayerTissueAllDetectors();
        var viewModel = new SimulationInputViewModel(input);
        Assert.That(viewModel.SimulationOptionsVm != null, Is.True);
        Assert.That(viewModel.TissueInputVm != null, Is.True);
        Assert.That(viewModel.TissueTypeVm != null, Is.True);
        Assert.That(viewModel.SimulationInput, Is.EqualTo(input));
        Assert.That(viewModel.N, Is.EqualTo(input.N));
    }

    [Test]
    public void Verify_update_values()
    {
        var input = SimulationInputProvider.PointSourceOneLayerTissueAllDetectors();
        var viewModel = new SimulationInputViewModel(input)
        {
            N = 10000,
            SimulationOptionsVm = new SimulationOptionsViewModel(new SimulationOptions(1, RandomNumberGeneratorType.MersenneTwister, AbsorptionWeightingType.Analog))
        };
        Assert.That(viewModel.SimulationOptionsVm != null, Is.True);
        Assert.That(viewModel.TissueInputVm != null, Is.True);
        Assert.That(viewModel.TissueTypeVm != null, Is.True);
        Assert.That(viewModel.N, Is.EqualTo(10000));
        Assert.That(viewModel.SimulationOptionsVm.SimulationOptions.Seed, Is.EqualTo(1));
        Assert.That(viewModel.SimulationOptionsVm.SimulationOptions.RandomNumberGeneratorType, Is.EqualTo(RandomNumberGeneratorType.MersenneTwister));
        Assert.That(viewModel.SimulationOptionsVm.SimulationOptions.AbsorptionWeightingType, Is.EqualTo(AbsorptionWeightingType.Analog));
    }
}