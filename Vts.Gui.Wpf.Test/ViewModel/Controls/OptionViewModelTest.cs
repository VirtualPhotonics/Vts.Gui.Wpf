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
            Assert.AreEqual("Label_Test", optionVM.GroupName);
            Assert.IsTrue(optionVM.ShowTitle);
        }

        [Test]
        public void Verify_constructor_with_groupName_and_showTitle_parameters_sets_properties_correctly()
        {
            var optionVM = new OptionViewModel<ColormapType>("Label_Test", false);
            Assert.AreEqual("Label_Test", optionVM.GroupName);
            Assert.IsFalse(optionVM.ShowTitle);
        }

        [Test]
        public void Verify_constructor_with_groupName_and_initialValue_parameters_sets_properties_correctly()
        {
            var simulationInput = new SimulationInput();
            var optionVM = new OptionViewModel<string>("Label_Test", simulationInput.TissueInput.TissueType);
            Assert.AreEqual("Label_Test", optionVM.GroupName);
            Assert.IsTrue(optionVM.ShowTitle);
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
            Assert.AreEqual("Tissue Type:", optionVM.GroupName);
            Assert.IsTrue(optionVM.ShowTitle);
            Assert.AreEqual(simulationInput.TissueInput.TissueType, optionVM.SelectedValue);
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
            Assert.AreEqual("String List:", optionViewModel.GroupName);
            Assert.IsTrue(optionViewModel.ShowTitle);
            Assert.AreEqual("SecondValue", optionViewModel.SelectedValue);
            Assert.AreEqual(4, optionViewModel.Options.Keys.Count);
            Assert.AreEqual(1, optionViewModel.SelectedValues.Length);
            optionViewModel.Options["FourthValue"].IsSelected = true;
            Assert.AreEqual(2, optionViewModel.SelectedValues.Length);
        }
    }
}
