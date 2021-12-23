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
        public void Verify_constructor_with_GroupName_and_ShowTitle_parameters_sets_properties_correctly()
        {
            var optionVM = new OptionViewModel<ColormapType>("Label_Test", false);
            Assert.AreEqual("Label_Test", optionVM.GroupName);
            Assert.IsFalse(optionVM.ShowTitle);
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
            Assert.AreEqual("Tissue Type:", optionVM.GroupName);
            Assert.IsTrue(optionVM.ShowTitle);
            Assert.AreEqual(simulationInput.TissueInput.TissueType, optionVM.SelectedValue);
        }

    }
}
