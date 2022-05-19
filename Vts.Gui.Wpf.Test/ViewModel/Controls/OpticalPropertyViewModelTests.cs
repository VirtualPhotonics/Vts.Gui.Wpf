using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Controls
{
    /// <summary>
    /// Tests for OpticalPropertyViewModel class
    /// </summary>
    [TestFixture]
    public class OpticalPropertyViewModelTests
    {
        /// <summary>
        /// Verifies that OpticalPropertyViewModel default constructor instantiates properties correctly
        /// </summary>
        [Test]
        public void Verify_default_constructor_sets_properties_correctly()
        {
            var opticalPropertyVM = new OpticalPropertyViewModel();
            Assert.AreEqual(0.01, opticalPropertyVM.Mua);
            Assert.AreEqual(1.0, opticalPropertyVM.Musp);
            Assert.AreEqual(0.8, opticalPropertyVM.G);
            Assert.AreEqual(1.4, opticalPropertyVM.N);
        }

        /// <summary>
        /// Verifies that TextOutputViewModel constructor with arguments instantiates properties correctly
        /// </summary>
        [Test]
        public void Verify_constructor_with_arguments_sets_properties_correctly()
        {
            var opticalPropertyVm = new OpticalPropertyViewModel(
                new OpticalProperties(0.1, 1.1, 0.9, 1.3),
                IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(),
                "title",
                true,
                true,
                false,
                false);
            Assert.AreEqual(0.1, opticalPropertyVm.Mua);
            Assert.AreEqual(1.1, opticalPropertyVm.Musp);
            Assert.AreEqual(0.9, opticalPropertyVm.G);
            Assert.AreEqual(1.3, opticalPropertyVm.N);
            Assert.AreEqual(IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(), opticalPropertyVm.Units);
            Assert.IsTrue(opticalPropertyVm.EnableMua);
            Assert.IsTrue(opticalPropertyVm.EnableMusp);
            Assert.IsFalse(opticalPropertyVm.EnableG);
            Assert.IsFalse(opticalPropertyVm.EnableN);
        }

        [Test]
        public void Verify_optical_property_values_set_correctly()
        {
            var opticalPropertyVm = new OpticalPropertyViewModel
            {
                Mua = 0.001, 
                Musp = 0.1, 
                G = 0.9, 
                N = 1.5,
                EnableMua = true,
                EnableMusp = true,
                EnableG = false,
                EnableN = true
            };
            Assert.AreEqual(0.001, opticalPropertyVm.Mua);
            Assert.AreEqual(0.1, opticalPropertyVm.Musp);
            Assert.AreEqual(0.9, opticalPropertyVm.G);
            Assert.AreEqual(1.5, opticalPropertyVm.N);
            Assert.IsTrue(opticalPropertyVm.EnableMua);
            Assert.IsTrue(opticalPropertyVm.EnableMusp);
            Assert.IsFalse(opticalPropertyVm.EnableG);
            Assert.IsTrue(opticalPropertyVm.EnableN);
        }

        [Test]
        public void Verify_optical_property_title_set_correctly()
        {
            var opticalPropertyVm = new OpticalPropertyViewModel
            {
                Title = "Optical Properties",
                Mua = 0.001,
                Musp = 0.1,
                G = 0.9,
                N = 1.5
            };
            Assert.AreEqual("Optical Properties", opticalPropertyVm.Title);
            Assert.IsTrue(opticalPropertyVm.ShowTitle);
        }
    }
}
