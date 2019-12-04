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

        [Test]
        public void Verify_setting_properties()
        {
            var viewModel = new EllipsoidRegionViewModel(new EllipsoidTissueRegion(), "CurrentName");
            viewModel.Dx = 0.7;
            viewModel.Dy = 0.4;
            viewModel.Dz = 5.0;
            viewModel.X = 0.1;
            viewModel.Y = 0.2;
            viewModel.Z = 0.3;
            viewModel.OpticalPropertyVm = new OpticalPropertyViewModel(new OpticalProperties() { G = 0.8, Mua = 0.1, Musp = 0.01, N = 1.4 }, "cm", "NewTitle");
            viewModel.Name = "NewName";
            Assert.AreEqual("NewName" + StringLookup.GetLocalizedString("Label_Tissue"), viewModel.Name);
            Assert.AreEqual(0.1,viewModel.OpticalPropertyVm.Mua);
            Assert.AreEqual(0.8, viewModel.OpticalPropertyVm.G);
            Assert.AreEqual(0.01, viewModel.OpticalPropertyVm.Musp);
            Assert.AreEqual(1.4, viewModel.OpticalPropertyVm.N);
            Assert.AreEqual("cm", viewModel.OpticalPropertyVm.Units);
            Assert.AreEqual(0.1, viewModel.X);
            Assert.AreEqual(0.2, viewModel.Y);
            Assert.AreEqual(0.3, viewModel.Z);
            Assert.AreEqual(0.7, viewModel.Dx);
            Assert.AreEqual(0.4, viewModel.Dy);
            Assert.AreEqual(5.0, viewModel.Dz);
        }
    }
}
