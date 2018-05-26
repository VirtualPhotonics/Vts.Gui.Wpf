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
        public void verify_default_constructor_sets_properties_correctly()
        {
            var opticalPropertyVM = new OpticalPropertyViewModel();
            Assert.AreEqual(opticalPropertyVM.Mua, 0.01);
            Assert.AreEqual(opticalPropertyVM.Musp, 1.0);
            Assert.AreEqual(opticalPropertyVM.G, 0.8);
            Assert.AreEqual(opticalPropertyVM.N, 1.4);
        }

        /// <summary>
        /// Verifies that TextOutputViewModel constructor with arguments instantiates properties correctly
        /// </summary>
        [Test]
        public void verify_constructor_with_arguments_sets_properties_correctly()
        {
            var opticalPropertyVM = new OpticalPropertyViewModel(
                new OpticalProperties(0.01, 1.0, 0.8, 1.4),
                IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(),
                "title",
                true,
                true,
                false,
                false);
            Assert.AreEqual(opticalPropertyVM.Mua, 0.01);
            Assert.AreEqual(opticalPropertyVM.Musp, 1.0);
            Assert.AreEqual(opticalPropertyVM.G, 0.8);
            Assert.AreEqual(opticalPropertyVM.N, 1.4);
            Assert.AreEqual(opticalPropertyVM.Units, IndependentVariableAxisUnits.InverseMM.GetInternationalizedString());
            Assert.AreEqual(opticalPropertyVM.EnableMua, true);
            Assert.AreEqual(opticalPropertyVM.EnableMusp, true);
            Assert.AreEqual(opticalPropertyVM.EnableG, false);
            Assert.AreEqual(opticalPropertyVM.EnableN, false);
        }

    }
}
