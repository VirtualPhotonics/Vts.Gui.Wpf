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
            Assert.AreEqual(30.0, viewModel.HbO2.Concentration);
            Assert.AreEqual(10.0, viewModel.Hb.Concentration);
            Assert.AreEqual(0.75, viewModel.StO2); 
            Assert.AreEqual(40.0, viewModel.TotalHb);
        }

    }
}
