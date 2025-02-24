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
            Assert.That(viewModel.Name, Is.EqualTo(StringLookup.GetLocalizedString("Label_Tissue")));
            Assert.That(viewModel.IsEllipsoid, Is.True);
            Assert.That(viewModel.IsLayer, Is.False);
            Assert.That(viewModel.Units, Is.EqualTo(StringLookup.GetLocalizedString("Measurement_mm")));
            Assert.That(viewModel.OpticalPropertyVM != null, Is.True);
        }

        [Test]
        public void Verify_constructor_sets_properties_correctly()
        {
            var viewModel = new EllipsoidRegionViewModel(new EllipsoidTissueRegion(), "TestEllipsiodTissue");
            Assert.That(viewModel.Name, Is.EqualTo("TestEllipsiodTissue" + StringLookup.GetLocalizedString("Label_Tissue")));
            Assert.That(viewModel.IsEllipsoid, Is.True);
            Assert.That(viewModel.IsLayer, Is.False);
            Assert.That(viewModel.Units, Is.EqualTo(StringLookup.GetLocalizedString("Measurement_mm")));
            Assert.That(viewModel.OpticalPropertyVM != null, Is.True);
        }

        [Test]
        public void Verify_region_is_air()
        {
            var ellisoidTissueRegion = new EllipsoidTissueRegion {RegionOP = {G = 0.8, Mua = 0D, Mus = 1E-10}};
            ellisoidTissueRegion.RegionOP.G = 1.4;
            var viewModel = new EllipsoidRegionViewModel(ellisoidTissueRegion, "TestEllipsiodTissue");
            Assert.That(ellisoidTissueRegion.IsAir(), Is.True);
            Assert.That(viewModel.Name, Is.EqualTo("TestEllipsiodTissue" + StringLookup.GetLocalizedString("Label_Air")));
        }

        [Test]
        public void Verify_setting_properties()
        {
            var viewModel = new EllipsoidRegionViewModel(new EllipsoidTissueRegion(), "CurrentName")
            {
                Dx = 0.7,
                Dy = 0.4,
                Dz = 5.0,
                X = 0.1,
                Y = 0.2,
                Z = 0.3,
                OpticalPropertyVM =
                    new OpticalPropertyViewModel(new OpticalProperties()
                        {
                            G = 0.8, Mua = 0.1, Musp = 0.01, N = 1.4
                        },
                        "cm", "NewTitle"),
                Name = "NewName"
            };
            Assert.That(viewModel.Name, Is.EqualTo("NewName" + StringLookup.GetLocalizedString("Label_Tissue")));
            Assert.That(viewModel.OpticalPropertyVM.Mua, Is.EqualTo(0.1));
            Assert.That(viewModel.OpticalPropertyVM.G, Is.EqualTo(0.8));
            Assert.That(viewModel.OpticalPropertyVM.Musp, Is.EqualTo(0.01));
            Assert.That(viewModel.OpticalPropertyVM.N, Is.EqualTo(1.4));
            Assert.That(viewModel.OpticalPropertyVM.Units, Is.EqualTo("cm"));
            Assert.That(viewModel.X, Is.EqualTo(0.1));
            Assert.That(viewModel.Y, Is.EqualTo(0.2));
            Assert.That(viewModel.Z, Is.EqualTo(0.3));
            Assert.That(viewModel.Dx, Is.EqualTo(0.7));
            Assert.That(viewModel.Dy, Is.EqualTo(0.4));
            Assert.That(viewModel.Dz, Is.EqualTo(5.0));
        }
    }
}
