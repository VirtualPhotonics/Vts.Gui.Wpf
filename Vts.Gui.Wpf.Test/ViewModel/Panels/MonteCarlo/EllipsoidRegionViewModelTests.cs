using NUnit.Framework;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.ViewModel;
using Vts.MonteCarlo.Extensions;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels.MonteCarlo
{
    /// <summary>
    /// Summary description for EllipsoidRegionViewModelTests
    /// </summary>
    [TestFixture]
    public class EllipsoidRegionViewModelTests
    {
        /// <summary>
        /// Verifies that EllipsoidRegionModel default constructor instantiates sub ViewModels
        /// </summary>
        [Test]
        public void Verify_default_constructor_sets_properties_correctly()
        {
            var viewModel = new EllipsoidRegionViewModel();
            Assert.AreEqual(StringLookup.GetLocalizedString("Label_Tissue"), viewModel.Name);
            Assert.IsTrue(viewModel.IsEllipsoid);
            Assert.IsFalse(viewModel.IsLayer);
            Assert.AreEqual(StringLookup.GetLocalizedString("Measurement_mm"), viewModel.Units);
            Assert.IsTrue(viewModel.OpticalPropertyVm != null);
        }

        [Test]
        public void Verify_constructor_sets_properties_correctly()
        {
            var viewModel = new EllipsoidRegionViewModel(new EllipsoidTissueRegion(), "TestEllipsiodTissue");
            Assert.AreEqual("TestEllipsiodTissue" + StringLookup.GetLocalizedString("Label_Tissue"), viewModel.Name);
            Assert.IsTrue(viewModel.IsEllipsoid);
            Assert.IsFalse(viewModel.IsLayer);
            Assert.AreEqual(StringLookup.GetLocalizedString("Measurement_mm"),viewModel.Units);
            Assert.IsTrue(viewModel.OpticalPropertyVm != null);
        }

        [Test]
        public void Verify_region_is_air()
        {
            var ellisoidTissueRegion = new EllipsoidTissueRegion {RegionOP = {G = 0.8, Mua = 0D, Mus = 1E-10}};
            ellisoidTissueRegion.RegionOP.G = 1.4;
            var viewModel = new EllipsoidRegionViewModel(ellisoidTissueRegion, "TestEllipsiodTissue");
            Assert.IsTrue(ellisoidTissueRegion.IsAir());
            Assert.AreEqual("TestEllipsiodTissue" + StringLookup.GetLocalizedString("Label_Air"), viewModel.Name);
        }
    }
}
