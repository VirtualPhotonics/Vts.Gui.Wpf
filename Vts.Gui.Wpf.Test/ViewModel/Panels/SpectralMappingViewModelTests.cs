using NUnit.Framework;
using Vts.Gui.Wpf.ViewModel;
using Vts.Gui.Wpf.ViewModel.Panels;
using Vts.SpectralMapping;

namespace Vts.Gui.Wpf.Test.ViewModel.Panels;

/// <summary>
/// Summary description for SpectralMappingViewModelTests
/// </summary>
[TestFixture]
public class SpectralMappingViewModelTests
{
    private WindowViewModel? _windowViewModel;
    private SpectralMappingViewModel? _viewModel;
    
    [SetUp]
    public void Setup()
    {
        _windowViewModel = new WindowViewModel();
        _viewModel = _windowViewModel.SpectralMappingVm;
    }
    /// <summary>
    /// Verifies the SpectralMappingViewModel default constructor 
    /// </summary>
    [Test]
    public void Verify_default_constructor_sets_properties_correctly()
    {
        Assert.That(_viewModel.ScatteringTypeVm != null, Is.True);
        Assert.That(_viewModel.BloodConcentrationVm != null, Is.True);
        Assert.That(_viewModel.WavelengthRangeVm != null, Is.True);
        var listOfTissueTypes = _viewModel.Tissues.Select(t => t.TissueType).ToList();
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
        Assert.That(_viewModel.SelectedTissue.TissueType == TissueType.Skin, Is.True);
        Assert.That(_viewModel.ScatteringTypeVm, Is.Not.Null);
        Assert.That(_viewModel.SelectedTissue.ScattererType, Is.EqualTo(_viewModel.ScatteringTypeVm!.SelectedValue));
        Assert.That(_viewModel.ScatteringTypeName, Is.EqualTo("Vts.SpectralMapping.PowerLawScatterer"));
        Assert.That(Math.Abs(_viewModel.OpticalProperties.Mua - 0.1677) < 0.0001, Is.True);
        Assert.That(Math.Abs(_viewModel.OpticalProperties.Musp - 2.212) < 0.001, Is.True);
        Assert.That(_viewModel.OpticalProperties.G, Is.EqualTo(0.8));
        Assert.That(_viewModel.OpticalProperties.N, Is.EqualTo(1.4));
        Assert.That(_viewModel.Wavelength, Is.EqualTo(650));
        Assert.That(_viewModel.Mua, Is.EqualTo(_viewModel.OpticalProperties.Mua));
        Assert.That(_viewModel.Musp, Is.EqualTo(_viewModel.OpticalProperties.Musp));
        Assert.That(_viewModel.N, Is.EqualTo(_viewModel.OpticalProperties.N));
        Assert.That(_viewModel.G, Is.EqualTo(_viewModel.OpticalProperties.G));
    }

    [Test]
    public void Verify_optical_property_values()
    {
        _viewModel.Mua = 0.01;
        _viewModel.Musp = 1.0;
        _viewModel.N = 1.5;
        _viewModel.G = 0.5;
        Assert.That(_viewModel.OpticalProperties.Mua, Is.EqualTo(_viewModel.Mua));
        Assert.That(_viewModel.OpticalProperties.Musp, Is.EqualTo(_viewModel.Musp));
        Assert.That(_viewModel.OpticalProperties.N, Is.EqualTo(_viewModel.N));
        Assert.That(_viewModel.OpticalProperties.G, Is.EqualTo(_viewModel.G));
    }

    // The following tests verify the Relay Commands

    /// <summary>
    /// Verifies that the PlotMuaSpectrumCommand returns correct values
    /// </summary>
    [Test]
    public void Verify_PlotMuaSpectrumCommand_returns_correct_values()
    {
        _viewModel.PlotMuaSpectrumCommand.Execute(null);
        Assert.That(_windowViewModel.PlotVm.Labels[0], Is.EqualTo("μa spectra"));
        Assert.That(_windowViewModel.PlotVm.Title, Is.EqualTo("μa [mm-1] versus λ [nm]"));
        // can't verify plotted data because inside private object
        var textOutputViewModel = _windowViewModel.TextOutputVm;
        Assert.That(textOutputViewModel.Text, Is.EqualTo("Plot View: plot cleared due to independent axis variable change\rPlotted μa spectrum; wavelength range [nm]: [650, 1000]\r"));
    }

    /// <summary>
    /// Verifies that the PlotMusSpectrumCommand returns correct values
    /// </summary>
    [Test]
    public void Verify_PlotMusSpectrumCommand_returns_correct_values()
    {
        _viewModel.PlotMuspSpectrumCommand.Execute(null);
        Assert.That(_windowViewModel.PlotVm.Labels[0], Is.EqualTo("μs' spectra"));
        Assert.That(_windowViewModel.PlotVm.Title, Is.EqualTo("μs' [mm-1] versus λ [nm]"));
        var textOutputViewModel = _windowViewModel.TextOutputVm;
        Assert.That(textOutputViewModel.Text, Is.EqualTo("Plot View: plot cleared due to independent axis variable change\rPlotted μs' spectrum; wavelength range [nm]: [650, 1000]\r"));
    }

    [Test]
    public void Verify_ResetConcentrations_resets_values()
    {
        var tissue = new Tissue(TissueType.Skin);
        _viewModel.ResetConcentrations.Execute(tissue);
        Assert.That(_viewModel.SelectedTissue.Absorbers, Is.InstanceOf<IList<IChromophoreAbsorber>>());
    }

    [Test]
    public void Verify_UpdateWavelength_resets_values()
    {
        const double wavelength = 600;
        _viewModel.UpdateWavelength.Execute(wavelength);
        Assert.That(_viewModel.Wavelength, Is.EqualTo(600));
    }

    // the following tests verify that the Tissue selection and the ScattererType selection work together and independently
    /// <summary>
    /// SelectedTissue property is bound to the xaml user selection of TissueType
    /// </summary>
    [Test]
    public void Verify_SelectedTissue_property_sets_correct_scatterer_type()
    {
        foreach (var tissue in _viewModel.Tissues)
        {
            // set SelectedTissue which will set ScattererType
            _viewModel.SelectedTissue = tissue;
            // make sure all real tissues except Intralipid has PowerLaw scatterer type
            switch (tissue.TissueType)
            {
                case TissueType.IntralipidPhantom:
                    Assert.That(_viewModel.ScatteringTypeVm.SelectedValue == ScatteringType.Intralipid, Is.True);
                    Assert.That(Math.Abs(((IntralipidScatterer)_viewModel.Scatterer).VolumeFraction - 0.01) < 1e-3, Is.True);
                    break;
                case TissueType.Skin:
                    Assert.That(_viewModel.ScatteringTypeVm.SelectedValue == ScatteringType.PowerLaw, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).A - 1.2) < 1e-1, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).B - 1.42) < 1e-2, Is.True);
                    break;
                case TissueType.BrainGrayMatter:
                    Assert.That(_viewModel.ScatteringTypeVm.SelectedValue == ScatteringType.PowerLaw, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).A - 0.56) < 1e-2, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).B - 1.36) < 1e-2, Is.True); 
                    break;
                case TissueType.BrainWhiteMatter:
                    Assert.That(_viewModel.ScatteringTypeVm.SelectedValue == ScatteringType.PowerLaw, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).A - 3.56) < 1e-2, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).B - 0.84) < 1e-2, Is.True);
                    break;
                case TissueType.BreastPostMenopause:
                    Assert.That(_viewModel.ScatteringTypeVm.SelectedValue == ScatteringType.PowerLaw, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).A - 0.72) < 1e-2, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).B - 0.58) < 1e-2, Is.True);
                    break;
                case TissueType.BreastPreMenopause:
                    Assert.That(_viewModel.ScatteringTypeVm.SelectedValue == ScatteringType.PowerLaw, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).A - 0.67) < 1e-2, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).B - 0.95) < 1e-2, Is.True);
                    break;
                case TissueType.Liver:
                    Assert.That(_viewModel.ScatteringTypeVm.SelectedValue == ScatteringType.PowerLaw, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).A - 0.84) < 1e-2, Is.True);
                    Assert.That(Math.Abs(((PowerLawScatterer)_viewModel.Scatterer).B - 0.55) < 1e-2, Is.True);
                    break;
                case TissueType.PolystyreneSpherePhantom:
                    Assert.That(_viewModel.ScatteringTypeVm.SelectedValue == ScatteringType.Mie, Is.True);
                    Assert.That(Math.Abs(((MieScatterer)_viewModel.Scatterer).ParticleRadius - 0.5) < 1e-1, Is.True);
                    Assert.That(Math.Abs(((MieScatterer)_viewModel.Scatterer).ParticleRefractiveIndexMismatch - 1.4) < 1e-1, Is.True);
                    Assert.That(Math.Abs(((MieScatterer)_viewModel.Scatterer).MediumRefractiveIndexMismatch - 1.0) < 1e-1, Is.True);
                    Assert.That(Math.Abs(((MieScatterer)_viewModel.Scatterer).VolumeFraction - 0.01) < 1e-2, Is.True);
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
        foreach (var scattererType in _viewModel.ScatteringTypeVm.Options.Keys)
        {
            // set scatterer type
            _viewModel.ScatteringTypeVm.SelectedValue = scattererType;
            switch (scattererType)
            {
                case ScatteringType.Intralipid:
                    Assert.That(_viewModel.ScatteringTypeName == "Vts.SpectralMapping.IntralipidScatterer", Is.True);
                    break;
                case ScatteringType.PowerLaw:
                    Assert.That(_viewModel.ScatteringTypeName == "Vts.SpectralMapping.PowerLawScatterer", Is.True);
                    break;
                case ScatteringType.Mie:
                    Assert.That(_viewModel.ScatteringTypeName == "Vts.SpectralMapping.MieScatterer", Is.True);
                    break;

            }
        }
    }
}