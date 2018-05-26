using System;
using NUnit.Framework;
using Vts.MonteCarlo;
using Vts.Gui.Wpf.ViewModel;

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
        public void verify_constructor_with_GroupName_parameter_sets_properties_correctly()
        {
            // constructor overload
            var optionVM = new OptionViewModel<ColormapType>("Label_Test");
            Assert.AreEqual(optionVM.GroupName, "Label_Test");
            Assert.AreEqual(optionVM.ShowTitle, true);
        }

        [Test]
        public void verify_constructor_with_GroupName_and_ShowTitle_parameters_sets_properties_correctly()
        {
            var optionVM = new OptionViewModel<ColormapType>("Label_Test", false);
            Assert.AreEqual(optionVM.GroupName, "Label_Test");
            Assert.AreEqual(optionVM.ShowTitle, false);
        }
        [Test]
        public void verify_constructor_with_all_parameter_sets_properties_correctly()
        {
            var simulationInput = new SimulationInput();
            var optionVM = new OptionViewModel<string>("Tissue Type:", true, simulationInput.TissueInput.TissueType,
                new[]
                {
                    "MultiLayer",
                    "SingleEllipsoid",
                    "SingleVoxel"
                });
            Assert.AreEqual(optionVM.GroupName, "Tissue Type:");
            Assert.AreEqual(optionVM.ShowTitle, true);
            Assert.AreEqual(optionVM.SelectedValue, simulationInput.TissueInput.TissueType);
            //Assert.AreEqual(optionVM.);
        }

    }
}
