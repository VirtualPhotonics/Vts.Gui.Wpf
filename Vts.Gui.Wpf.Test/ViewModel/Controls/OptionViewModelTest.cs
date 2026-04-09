using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;
using Vts.MonteCarlo;

namespace Vts.Gui.Wpf.Test.ViewModel.Controls;

/// <summary>
/// Tests for OptionViewModel class
/// </summary>
[TestFixture]
public class OptionViewModelTests
{
    /// <summary>
    /// The following series of tests verifies that various constructors instantiate properties correctly.
    /// Unit test examples inspired from our code.
    /// </summary>
    [Test]
    public void Verify_constructor_with_GroupName_parameter_sets_properties_correctly()
    {
        // constructor overload
        var optionVm = new OptionViewModel<ColormapType>("Label_Test");
        Assert.That(optionVm.GroupName, Is.EqualTo("Label_Test"));
        Assert.That(optionVm.ShowTitle, Is.True);
    }

    [Test]
    public void Verify_constructor_with_groupName_and_showTitle_parameters_sets_properties_correctly()
    {
        var optionVm = new OptionViewModel<ColormapType>("Label_Test", false);
        Assert.That(optionVm.GroupName, Is.EqualTo("Label_Test"));
        Assert.That(optionVm.ShowTitle, Is.False);
    }

    [Test]
    public void Verify_constructor_with_groupName_and_initialValue_parameters_sets_properties_correctly()
    {
        var simulationInput = new SimulationInput();
        var optionVm = new OptionViewModel<string>("Label_Test", simulationInput.TissueInput.TissueType);
        Assert.That(optionVm.GroupName, Is.EqualTo("Label_Test"));
        Assert.That(optionVm.ShowTitle, Is.True);
    }

    [Test]
    public void Verify_constructor_with_all_parameter_sets_properties_correctly()
    {
        var simulationInput = new SimulationInput();
        var optionVm = new OptionViewModel<string>("Tissue Type:", true, simulationInput.TissueInput.TissueType,
        [
            "MultiLayer",
                "SingleEllipsoid",
                "SingleVoxel"
        ])
        {
            EnableMultiSelect = false
        };
        Assert.That(optionVm.GroupName, Is.EqualTo("Tissue Type:"));
        Assert.That(optionVm.ShowTitle, Is.True);
        Assert.That(optionVm.SelectedValue, Is.EqualTo(simulationInput.TissueInput.TissueType));
    }

    [Test]
    public void Verify_constructor_with_all_parameter_and_multi_select()
    {
        var optionViewModel = new OptionViewModel<string>("String List:", true, "SecondValue",
        [
            "FirstValue",
                "SecondValue",
                "ThirdValue",
                "FourthValue"
        ])
        {
            EnableMultiSelect = true
        };
        Assert.That(optionViewModel.GroupName, Is.EqualTo("String List:"));
        Assert.That(optionViewModel.ShowTitle, Is.True);
        Assert.That(optionViewModel.SelectedValue, Is.EqualTo("SecondValue"));
        Assert.That(optionViewModel.Options.Keys.Count, Is.EqualTo(4));
        Assert.That(optionViewModel.SelectedValues.Length, Is.EqualTo(1));
        optionViewModel.Options["FourthValue"].IsSelected = true;
        Assert.That(optionViewModel.SelectedValues.Length, Is.EqualTo(2));
    }
}
