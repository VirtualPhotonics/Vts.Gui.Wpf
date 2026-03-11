using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;
using Vts.MonteCarlo;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo;

/// <summary>
/// Summary description for SimulationInputViewModelTests
/// </summary>
[TestFixture]
public class SimulationInputViewModelTests
{
    /// <summary>
    /// Verifies that SimulationInputModel default constructor instantiates sub ViewModels
    /// </summary>
    [Test]
    public void Verify_default_constructor_sets_properties_correctly()
    {
        var viewModel = new SimulationInputViewModel();
        Assert.That(viewModel.SimulationOptionsVM != null, Is.True);
        Assert.That(viewModel.TissueInputVM != null, Is.True);
        Assert.That(viewModel.TissueTypeVM != null, Is.True);
    }

    [Test]
    public void Verify_set_simulation_input()
    {
        var viewModel = new SimulationInputViewModel();
        var input = SimulationInputProvider.PointSourceOneLayerTissueAllDetectors();
        viewModel.SimulationInput = input;
        Assert.That(viewModel.SimulationOptionsVM != null, Is.True);
        Assert.That(viewModel.TissueInputVM != null, Is.True);
        Assert.That(viewModel.TissueTypeVM != null, Is.True);
        Assert.That(viewModel.SimulationInput, Is.EqualTo(input));
        Assert.That(viewModel.N, Is.EqualTo(input.N));
    }

    [Test]
    public void Verify_constructor_with_input_sets_properties_correctly()
    {
        var input = SimulationInputProvider.PointSourceOneLayerTissueAllDetectors();
        var viewModel = new SimulationInputViewModel(input);
        Assert.That(viewModel.SimulationOptionsVM != null, Is.True);
        Assert.That(viewModel.TissueInputVM != null, Is.True);
        Assert.That(viewModel.TissueTypeVM != null, Is.True);
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
            SimulationOptionsVM = new SimulationOptionsViewModel(new SimulationOptions(1, RandomNumberGeneratorType.MersenneTwister, AbsorptionWeightingType.Analog))
        };
        Assert.That(viewModel.SimulationOptionsVM != null, Is.True);
        Assert.That(viewModel.TissueInputVM != null, Is.True);
        Assert.That(viewModel.TissueTypeVM != null, Is.True);
        Assert.That(viewModel.N, Is.EqualTo(10000));
        Assert.That(viewModel.SimulationOptionsVM.SimulationOptions.Seed, Is.EqualTo(1));
        Assert.That(viewModel.SimulationOptionsVM.SimulationOptions.RandomNumberGeneratorType, Is.EqualTo(RandomNumberGeneratorType.MersenneTwister));
        Assert.That(viewModel.SimulationOptionsVM.SimulationOptions.AbsorptionWeightingType, Is.EqualTo(AbsorptionWeightingType.Analog));
    }
}