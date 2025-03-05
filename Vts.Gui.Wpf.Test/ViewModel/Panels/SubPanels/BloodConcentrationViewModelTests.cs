using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.SubPanels
{
    /// <summary>
    /// Summary description for BloodConcentrationViewModelTests
    /// </summary>
    [TestFixture]
    public class BloodConcentrationViewModelTests
    {
        /// <summary>
        /// Verifies that BloodConcentrationModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void Verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new BloodConcentrationViewModel();
            Assert.That(viewModel.HbO2.Concentration, Is.EqualTo(30.0));
            Assert.That(viewModel.Hb.Concentration, Is.EqualTo(10.0));
            Assert.That(viewModel.StO2, Is.EqualTo(0.75)); 
            Assert.That(viewModel.TotalHb, Is.EqualTo(40.0));
        }

    }
}
