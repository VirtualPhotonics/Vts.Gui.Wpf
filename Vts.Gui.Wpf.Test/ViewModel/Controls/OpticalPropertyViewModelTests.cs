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
            Assert.That(opticalPropertyVM.Mua, Is.EqualTo(0.01));
            Assert.That(opticalPropertyVM.Musp, Is.EqualTo(1.0));
            Assert.That(opticalPropertyVM.G, Is.EqualTo(0.8));
            Assert.That(opticalPropertyVM.N, Is.EqualTo(1.4));
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
            Assert.That(opticalPropertyVm.Mua, Is.EqualTo(0.1));
            Assert.That(opticalPropertyVm.Musp, Is.EqualTo(1.1));
            Assert.That(opticalPropertyVm.G, Is.EqualTo(0.9));
            Assert.That(opticalPropertyVm.N, Is.EqualTo(1.3));
            Assert.That(opticalPropertyVm.Units, Is.EqualTo(IndependentVariableAxisUnits.InverseMM.GetInternationalizedString()));
            Assert.That(opticalPropertyVm.EnableMua, Is.True);
            Assert.That(opticalPropertyVm.EnableMusp, Is.True);
            Assert.That(opticalPropertyVm.EnableG, Is.False);
            Assert.That(opticalPropertyVm.EnableN, Is.False);
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
            Assert.That(opticalPropertyVm.Mua, Is.EqualTo(0.001));
            Assert.That(opticalPropertyVm.Musp, Is.EqualTo(0.1));
            Assert.That(opticalPropertyVm.G, Is.EqualTo(0.9));
            Assert.That(opticalPropertyVm.N, Is.EqualTo(1.5));
            Assert.That(opticalPropertyVm.EnableMua, Is.True);
            Assert.That(opticalPropertyVm.EnableMusp, Is.True);
            Assert.That(opticalPropertyVm.EnableG, Is.False);
            Assert.That(opticalPropertyVm.EnableN, Is.True);
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
            Assert.That(opticalPropertyVm.Title, Is.EqualTo("Optical Properties"));
            Assert.That(opticalPropertyVm.ShowTitle, Is.True);
        }
    }
}
