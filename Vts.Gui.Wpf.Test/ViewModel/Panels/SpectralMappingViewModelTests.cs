using NUnit.Framework;
using System;
using System.Collections.Generic;
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
        public void Verify_default_constructor_sets_properties_correctly()
        {
            Assert.That(viewModel.ScatteringTypeVM != null, Is.True);
            Assert.That(viewModel.BloodConcentrationVM != null, Is.True);
            Assert.That(viewModel.WavelengthRangeVM != null, Is.True);
            var listOfTissueTypes = viewModel.Tissues.Select(t => t.TissueType).ToList();
            Assert.That(listOfTissueTypes, Is.EquivalentTo([
                TissueType.Skin,
                TissueType.BrainWhiteMatter,
                TissueType.BrainGrayMatter,
                TissueType.BreastPreMenopause,
                TissueType.BreastPostMenopause,
                TissueType.Liver,
                TissueType.IntralipidPhantom,
                TissueType.Custom
            ]));
            Assert.That(viewModel.SelectedTissue.TissueType == TissueType.Skin, Is.True);
            Assert.That(viewModel.SelectedTissue.ScattererType, Is.EqualTo(viewModel.ScatteringTypeVM.SelectedValue));
            Assert.That(viewModel.ScatteringTypeName, Is.EqualTo("Vts.SpectralMapping.PowerLawScatterer"));
            Assert.That(Math.Abs(viewModel.OpticalProperties.Mua - 0.1677) < 0.0001, Is.True);
            Assert.That(Math.Abs(viewModel.OpticalProperties.Musp - 2.212) < 0.001, Is.True);
            Assert.That(viewModel.OpticalProperties.G, Is.EqualTo(0.8));
            Assert.That(viewModel.OpticalProperties.N, Is.EqualTo(1.4));
            Assert.That(viewModel.Wavelength, Is.EqualTo(650));
            Assert.That(viewModel.Mua, Is.EqualTo(viewModel.OpticalProperties.Mua));
            Assert.That(viewModel.Musp, Is.EqualTo(viewModel.OpticalProperties.Musp));
            Assert.That(viewModel.N, Is.EqualTo(viewModel.OpticalProperties.N));
            Assert.That(viewModel.G, Is.EqualTo(viewModel.OpticalProperties.G));
        }

        [Test]
        public void Verify_optical_property_values()
        {
            viewModel.Mua = 0.01;
            viewModel.Musp = 1.0;
            viewModel.N = 1.5;
            viewModel.G = 0.5;
            Assert.That(viewModel.OpticalProperties.Mua, Is.EqualTo(viewModel.Mua));
            Assert.That(viewModel.OpticalProperties.Musp, Is.EqualTo(viewModel.Musp));
            Assert.That(viewModel.OpticalProperties.N, Is.EqualTo(viewModel.N));
            Assert.That(viewModel.OpticalProperties.G, Is.EqualTo(viewModel.G));
        }

        // The following tests verify the Relay Commands

        /// <summary>
        /// Verifies that the PlotMuaSpectrumCommand returns correct values
        /// </summary>
        [Test]
        public void Verify_PlotMuaSpectrumCommand_returns_correct_values()
        {
            viewModel.PlotMuaSpectrumCommand.Execute(null);
            Assert.That(windowViewModel.PlotVM.Labels[0], Is.EqualTo("μa spectra"));
            Assert.That(windowViewModel.PlotVM.Title, Is.EqualTo("μa [mm-1] versus λ [nm]"));
            // can't verify plotted data because inside private object
            var textOutputViewModel = windowViewModel.TextOutputVM;
            Assert.That(textOutputViewModel.Text, Is.EqualTo("Plot View: plot cleared due to independent axis variable change\rPlotted μa spectrum; wavelength range [nm]: [650, 1000]\r"));
        }

        /// <summary>
        /// Verifies that the PlotMusSpectrumCommand returns correct values
        /// </summary>
        [Test]
        public void Verify_PlotMusSpectrumCommand_returns_correct_values()
        {
            viewModel.PlotMuspSpectrumCommand.Execute(null);
            Assert.That(windowViewModel.PlotVM.Labels[0], Is.EqualTo("μs' spectra"));
            Assert.That(windowViewModel.PlotVM.Title, Is.EqualTo("μs' [mm-1] versus λ [nm]"));
            var textOutputViewModel = windowViewModel.TextOutputVM;
            Assert.That(textOutputViewModel.Text, Is.EqualTo("Plot View: plot cleared due to independent axis variable change\rPlotted μs' spectrum; wavelength range [nm]: [650, 1000]\r"));
        }

        [Test]
        public void Verify_ResetConcentrations_resets_values()
        {
            var tissue = new Tissue(TissueType.Skin);
            viewModel.ResetConcentrations.Execute(tissue);
            Assert.That(viewModel.SelectedTissue.Absorbers, Is.InstanceOf<IList<IChromophoreAbsorber>>());
        }

        [Test]
        public void Verify_UpdateWavelength_resets_values()
        {
            const double wavelength = 600;
            viewModel.UpdateWavelength.Execute(wavelength);
            Assert.That(viewModel.Wavelength, Is.EqualTo(600));
        }

        // the following tests verify that the Tissue selection and the ScattererType selection work together and independently
        /// <summary>
        /// SelectedTissue property is bound to the xaml user selection of TissueType
        /// </summary>
        [Test]
        public void Verify_SelectedTissue_property_sets_correct_scatterer_type()
        {
            foreach (var tissue in viewModel.Tissues)
            {
                // set SelectedTissue which will set ScattererType
                viewModel.SelectedTissue = tissue;
                // make sure all real tissues except Intralipid has PowerLaw scatterer type
                switch (tissue.TissueType)
                {
                    case TissueType.IntralipidPhantom:
                        Assert.That(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.Intralipid, Is.True);
                        Assert.That(Math.Abs(((IntralipidScatterer)viewModel.Scatterer).VolumeFraction - 0.01) < 1e-3, Is.True);
                        break;
                    case TissueType.Skin:
                        Assert.That(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).A - 1.2) < 1e-1, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).B - 1.42) < 1e-2, Is.True);
                        break;
                    case TissueType.BrainGrayMatter:
                        Assert.That(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).A - 0.56) < 1e-2, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).B - 1.36) < 1e-2, Is.True); 
                        break;
                    case TissueType.BrainWhiteMatter:
                        Assert.That(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).A - 3.56) < 1e-2, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).B - 0.84) < 1e-2, Is.True);
                        break;
                    case TissueType.BreastPostMenopause:
                        Assert.That(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).A - 0.72) < 1e-2, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).B - 0.58) < 1e-2, Is.True);
                        break;
                    case TissueType.BreastPreMenopause:
                        Assert.That(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).A - 0.67) < 1e-2, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).B - 0.95) < 1e-2, Is.True);
                        break;
                    case TissueType.Liver:
                        Assert.That(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.PowerLaw, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).A - 0.84) < 1e-2, Is.True);
                        Assert.That(Math.Abs(((PowerLawScatterer)viewModel.Scatterer).B - 0.55) < 1e-2, Is.True);
                        break;
                    case TissueType.PolystyreneSpherePhantom:
                        Assert.That(viewModel.ScatteringTypeVM.SelectedValue == ScatteringType.Mie, Is.True);
                        Assert.That(Math.Abs(((MieScatterer)viewModel.Scatterer).ParticleRadius - 0.5) < 1e-1, Is.True);
                        Assert.That(Math.Abs(((MieScatterer)viewModel.Scatterer).ParticleRefractiveIndexMismatch - 1.4) < 1e-1, Is.True);
                        Assert.That(Math.Abs(((MieScatterer)viewModel.Scatterer).MediumRefractiveIndexMismatch - 1.0) < 1e-1, Is.True);
                        Assert.That(Math.Abs(((MieScatterer)viewModel.Scatterer).VolumeFraction - 0.01) < 1e-2, Is.True);
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
        public void Verify_scatterer_type_selection_is_correct()
        {
            foreach (var scattererType in viewModel.ScatteringTypeVM.Options.Keys)
            {
                // set scatterer type
                viewModel.ScatteringTypeVM.SelectedValue = scattererType;
                switch (scattererType)
                {
                    case ScatteringType.Intralipid:
                        Assert.That(viewModel.ScatteringTypeName == "Vts.SpectralMapping.IntralipidScatterer", Is.True);
                        break;
                    case ScatteringType.PowerLaw:
                        Assert.That(viewModel.ScatteringTypeName == "Vts.SpectralMapping.PowerLawScatterer", Is.True);
                        break;
                    case ScatteringType.Mie:
                        Assert.That(viewModel.ScatteringTypeName == "Vts.SpectralMapping.MieScatterer", Is.True);
                        break;

                }
            }
        }
    }
}