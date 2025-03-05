using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;
using Vts.MonteCarlo;

namespace Vts.Gui.Wpf.Test.ViewModel.Controls
{
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
            var optionVM = new OptionViewModel<ColormapType>("Label_Test");
            Assert.That(optionVM.GroupName, Is.EqualTo("Label_Test"));
            Assert.That(optionVM.ShowTitle, Is.True);
        }

        [Test]
        public void Verify_constructor_with_groupName_and_showTitle_parameters_sets_properties_correctly()
        {
            var optionVM = new OptionViewModel<ColormapType>("Label_Test", false);
            Assert.That(optionVM.GroupName, Is.EqualTo("Label_Test"));
            Assert.That(optionVM.ShowTitle, Is.False);
        }

        [Test]
        public void Verify_constructor_with_groupName_and_initialValue_parameters_sets_properties_correctly()
        {
            var simulationInput = new SimulationInput();
            var optionVM = new OptionViewModel<string>("Label_Test", simulationInput.TissueInput.TissueType);
            Assert.That(optionVM.GroupName, Is.EqualTo("Label_Test"));
            Assert.That(optionVM.ShowTitle, Is.True);
        }

        [Test]
        public void Verify_constructor_with_all_parameter_sets_properties_correctly()
        {
            var simulationInput = new SimulationInput();
            var optionVM = new OptionViewModel<string>("Tissue Type:", true, simulationInput.TissueInput.TissueType,
                new[]
                {
                    "MultiLayer",
                    "SingleEllipsoid",
                    "SingleVoxel"
                });
            optionVM.EnableMultiSelect = false;
            Assert.That(optionVM.GroupName, Is.EqualTo("Tissue Type:"));
            Assert.That(optionVM.ShowTitle, Is.True);
            Assert.That(optionVM.SelectedValue, Is.EqualTo(simulationInput.TissueInput.TissueType));
        }

        [Test]
        public void Verify_constructor_with_all_parameter_and_multi_select()
        {
            var optionViewModel = new OptionViewModel<string>("String List:", true, "SecondValue",
                new[]
                {
                    "FirstValue",
                    "SecondValue",
                    "ThirdValue",
                    "FourthValue"
                })
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
}
