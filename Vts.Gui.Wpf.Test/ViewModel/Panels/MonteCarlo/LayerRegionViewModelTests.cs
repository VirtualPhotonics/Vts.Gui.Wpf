using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo
{
    /// <summary>
    /// Summary description for LayerRegionViewModelTests
    /// </summary>
    [TestFixture]
    public class LayerRegionViewModelTests
    {
        /// <summary>
        /// Verifies that LayerRegionModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new LayerRegionViewModel();
            Assert.AreEqual(viewModel.Name, " (Tissue)");
            Assert.IsFalse(viewModel.IsEllipsoid);
            Assert.IsTrue(viewModel.IsLayer);
            Assert.IsTrue(viewModel.OpticalPropertyVM != null);
            Assert.IsTrue(viewModel.OpticalPropertyVM != null);
        }
        
    }
}
