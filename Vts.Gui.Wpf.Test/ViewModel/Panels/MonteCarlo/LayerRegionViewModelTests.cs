using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
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
            Assert.That(StringLookup.GetLocalizedString("Label_Tissue"), Is.EqualTo(viewModel.Name));
            Assert.That(viewModel.IsEllipsoid, Is.False);
            Assert.That(viewModel.IsLayer, Is.True);
            Assert.That(viewModel.OpticalPropertyVM != null, Is.True);
            Assert.That(viewModel.OpticalPropertyVM != null, Is.True);
        }
        
    }
}
