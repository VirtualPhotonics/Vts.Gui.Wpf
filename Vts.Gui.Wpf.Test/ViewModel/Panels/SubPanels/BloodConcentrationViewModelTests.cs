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
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new BloodConcentrationViewModel();
            Assert.AreEqual(viewModel.HbO2.Concentration, 30.0);
            Assert.AreEqual(viewModel.Hb.Concentration, 10.0);
            Assert.AreEqual(viewModel.StO2, 0.75); 
            Assert.AreEqual(viewModel.TotalHb, 40.0);
        }

    }
}
