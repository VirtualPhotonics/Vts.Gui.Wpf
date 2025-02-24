using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
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
            Assert.That(StringLookup.GetLocalizedString("Label_Tissue"), Is.EqualTo(viewModel.Name));
            Assert.That(viewModel.IsLayer, Is.False);
            Assert.That(StringLookup.GetLocalizedString("Measurement_mm"), Is.EqualTo(viewModel.Units));
            Assert.That(viewModel.OpticalPropertyVM != null, Is.True);
        }
        
    }
}
