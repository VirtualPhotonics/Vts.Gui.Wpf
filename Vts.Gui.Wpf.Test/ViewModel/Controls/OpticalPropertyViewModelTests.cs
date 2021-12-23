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
            var opticalPropertyVM = new OpticalPropertyViewModel(
                new OpticalProperties(0.1, 1.1, 0.9, 1.3),
                IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(),
                "title",
                true,
                true,
                false,
                false);
            Assert.AreEqual(0.1, opticalPropertyVM.Mua);
            Assert.AreEqual(1.1, opticalPropertyVM.Musp);
            Assert.AreEqual(0.9, opticalPropertyVM.G);
            Assert.AreEqual(1.3, opticalPropertyVM.N);
            Assert.AreEqual(IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(), opticalPropertyVM.Units);
            Assert.IsTrue(opticalPropertyVM.EnableMua);
            Assert.IsTrue(opticalPropertyVM.EnableMusp);
            Assert.IsFalse(opticalPropertyVM.EnableG);
            Assert.IsFalse(opticalPropertyVM.EnableN);
        }

    }
}
