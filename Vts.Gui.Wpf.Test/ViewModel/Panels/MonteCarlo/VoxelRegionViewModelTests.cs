using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo
{
    /// <summary>
    /// Summary description for VoxelRegionViewModelTests
    /// </summary>
    [TestFixture]
    public class VoxelRegionViewModelTests
    {
        /// <summary>
        /// Verifies that VoxelRegionModel default constructor instantiates sub viewmodels
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new VoxelRegionViewModel();
            Assert.AreEqual(viewModel.Name, " (Tissue)");
            Assert.IsFalse(viewModel.IsLayer);
            Assert.AreEqual(viewModel.Units, "mm");
            Assert.IsTrue(viewModel.OpticalPropertyVM != null);
        }
        
    }
}
