using NUnit.Framework;
using System;
using System.Linq;
using Vts.Gui.Wpf.ViewModel;
using Vts.SpectralMapping;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels
{
    /// <summary>
    /// Summary description for SpectralMappingViewModelTests
    /// </summary>
    [TestFixture]
    public class SpectralMappingViewModelTests
    {
        private WindowViewModel windowViewModel;
        private SpectralMappingViewModel viewModel;
        [SetUp]
        public void Setup()
        {
            windowViewModel = new WindowViewModel();
            viewModel = windowViewModel.SpectralMappingVM;
        }
        /// <summary>
        /// Verifies the SpectralMappingViewModel default constructor 
        /// </summary>
        [Test]
        public void verify_default_constructor_sets_properties_correctly()
        {
            Assert.IsTrue(viewModel.ScatteringTypeVM != null);
            Assert.IsTrue(viewModel.BloodConcentrationVM != null);
            Assert.IsTrue(viewModel.WavelengthRangeVM != null);
            var listOfTissueTypes = viewModel.Tissues.Select(t => t.TissueType).ToList();
            CollectionAssert.AreEquivalent( new[]
                {
                    TissueType.Skin,
                    TissueType.BrainWhiteMatter,
                    TissueType.BrainGrayMatter,
                    TissueType.BreastPreMenopause,
                    TissueType.BreastPostMenopause,
                    TissueType.Liver,
                    TissueType.IntralipidPhantom,
                    TissueType.Custom
                }, 
                listOfTissueTypes);
            Assert.IsTrue(viewModel.SelectedTissue.TissueType == TissueType.Skin);
            Assert.AreEqual(viewModel.ScatteringTypeVM.SelectedValue, viewModel.SelectedTissue.ScattererType);
            Assert.AreEqual("Vts.SpectralMapping.PowerLawScatterer", viewModel.ScatteringTypeName);
            Assert.IsTrue(Math.Abs(viewModel.OpticalProperties.Mua - 0.1677) < 0.0001);
            Assert.IsTrue(Math.Abs(viewModel.OpticalProperties.Musp - 2.212) < 0.001);
            Assert.AreEqual(0.8, viewModel.OpticalProperties.G);
            Assert.AreEqual(1.4, viewModel.OpticalProperties.N);
            Assert.AreEqual(650, viewModel.Wavelength);
        }

        // The following tests verify the Relay Commands

        /// <summary>
        /// Verifies that the PlotMuaSpectrumCommand returns correct values
        /// </summary>
        [Test]
        public void verify_PlotMuaSpectrumCommand_returns_correct_values()
        {
            viewModel.PlotMuaSpectrumCommand.Execute(null);
            Assert.AreEqual(windowViewModel.PlotVM.Labels[0], "μa spectra");
            Assert.AreEqual(windowViewModel.PlotVM.Title, "μa [mm-1] versus λ [nm]");
            // can't verify plotted data because inside private object
            TextOutputViewModel textOutputViewModel = windowViewModel.TextOutputVM;
            Assert.AreEqual(textOutputViewModel.Text,
                "Plot View: plot cleared due to independent axis variable change\rPlotted μa spectrum; wavelength range [nm]: [650, 1000]\r");
        }

        /// <summary>
        /// Verifies that the PlotMusSpectrumCommand returns correct values
        /// </summary>
        [Test]
        public void verify_PlotMusSpectrumCommand_returns_correct_values()
        {
            viewModel.PlotMuspSpectrumCommand.Execute(null);
            Assert.AreEqual(windowViewModel.PlotVM.Labels[0], "μs' spectra");
            Assert.AreEqual(windowViewModel.PlotVM.Title, "μs' [mm-1] versus λ [nm]");
            TextOutputViewModel textOutputViewModel = windowViewModel.TextOutputVM;
            Assert.AreEqual(textOutputViewModel.Text,
                "Plot View: plot cleared due to independent axis variable change\rPlotted μs' spectrum; wavelength range [nm]: [650, 1000]\r");

        }

        // the following tests verify that the Tissue selection and the ScattererType selection work together and independently
        /// <summary>
        /// SelectedTissue property is bound to the xaml user selection of TissueType
        /// </summary>
        [Test]
        public void verify_SelectedTissue_property_sets_correct_scatterer_type()
        {
            foreach (var tissue in viewModel.Tissues)
            {
                // set SelectedTissue which will set ScattererType
                viewModel.SelectedTissue = tissue;
                // make sure all real tissues except Intralipid has PowerLaw scatterer type
                switch (tissue.TissueType)
                {
                    case TissueType.IntralipidPhantom:
                        Assert.IsTrue(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.Intralipid);
                        Assert.IsTrue(Math.Abs(
                            ((IntralipidScatterer)viewModel.Scatterer).VolumeFraction - 0.01) < 1e-3);
                        break;
                    case TissueType.Skin:
                        Assert.IsTrue(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).A - 1.2) < 1e-1);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).B - 1.42) < 1e-2);
                        break;
                    case TissueType.BrainGrayMatter:
                        Assert.IsTrue(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).A - 0.56) < 1e-2);
                        Assert.IsTrue(Math.Abs(                                                                    
                                          ((PowerLawScatterer)viewModel.Scatterer).B - 1.36) < 1e-2); 
                        break;
                    case TissueType.BrainWhiteMatter:
                        Assert.IsTrue(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).A - 3.56) < 1e-2);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).B - 0.84) < 1e-2);
                        break;
                    case TissueType.BreastPostMenopause:
                        Assert.IsTrue(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).A - 0.72) < 1e-2);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).B - 0.58) < 1e-2);
                        break;
                    case TissueType.BreastPreMenopause:
                        Assert.IsTrue(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).A - 0.67) < 1e-2);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).B - 0.95) < 1e-2);
                        break;
                    case TissueType.Liver:
                        Assert.IsTrue(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).A - 0.84) < 1e-2);
                        Assert.IsTrue(Math.Abs(
                                          ((PowerLawScatterer)viewModel.Scatterer).B - 0.55) < 1e-2);
                        break;
                    case TissueType.PolystyreneSpherePhantom:
                        Assert.IsTrue(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.Mie);
                        Assert.IsTrue(Math.Abs(
                                          ((MieScatterer)viewModel.Scatterer).ParticleRadius - 0.5) < 1e-1);
                        Assert.IsTrue(Math.Abs(
                                          ((MieScatterer)viewModel.Scatterer).ParticleRefractiveIndexMismatch - 1.4) < 1e-1);
                        Assert.IsTrue(Math.Abs(
                                          ((MieScatterer)viewModel.Scatterer).MediumRefractiveIndexMismatch - 1.0) < 1e-1);
                        Assert.IsTrue(Math.Abs(
                                          ((MieScatterer)viewModel.Scatterer).VolumeFraction - 0.01) < 1e-2);
                        break;
                    case TissueType.Custom: // Custom does not change Scatterer
                        break; 

                }
            }
        }
        /// <summary>
        /// ScatteringTypeVM.SelectedValue is bound to xaml user selection of Scatterer Type.
        /// ScatteringTypeName is bound to xaml to display correct scatterer data entry boxes
        /// </summary>
        [Test]
        public void verify_scatterer_type_selection_is_correct()
        {
            foreach (var scattererType in viewModel.ScatteringTypeVM.Options.Keys)
            {
                // set scatterer type
                viewModel.ScatteringTypeVM.SelectedValue = scattererType;
                switch (scattererType)
                {
                    case ScatteringType.Intralipid:
                        Assert.IsTrue(viewModel.ScatteringTypeName == "Vts.SpectralMapping.IntralipidScatterer");
                        break;
                    case ScatteringType.PowerLaw:
                        Assert.IsTrue(viewModel.ScatteringTypeName == "Vts.SpectralMapping.PowerLawScatterer");
                        break;
                    case ScatteringType.Mie:
                        Assert.IsTrue(viewModel.ScatteringTypeName == "Vts.SpectralMapping.MieScatterer");
                        break;

                }
            }
        }
    }
}