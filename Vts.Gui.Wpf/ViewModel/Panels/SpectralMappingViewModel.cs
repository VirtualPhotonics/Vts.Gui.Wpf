using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Vts.Common;
using Vts.Factories;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.SpectralMapping;

#if WHITELIST
using Vts.Gui.Wpf.ViewModel.Application;
#endif

namespace Vts.Gui.Wpf.ViewModel;

/// <summary>
///     View model implementing Spectral panel functionality
/// </summary>
public class SpectralMappingViewModel : BindableObject
{
    private Tissue _selectedTissue;
    private double _wavelength;

    public SpectralMappingViewModel()
    {
#if WHITELIST 
        ScatteringTypeVm = new OptionViewModel<ScatteringType>(StringLookup.GetLocalizedString("Heading_ScattererType"), true, WhiteList.ScatteringTypes);
#else
        ScatteringTypeVm = new OptionViewModel<ScatteringType>(StringLookup.GetLocalizedString("Heading_ScattererType"), true);
#endif
        ScatteringTypeVm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "SelectedValue" && SelectedTissue != null)
            {
                SelectedTissue.Scatterer = SolverFactory.GetScattererType(ScatteringTypeVm.SelectedValue);
                if (SelectedTissue.Scatterer is INotifyPropertyChanged bindableScatterer)
                {
                    bindableScatterer.PropertyChanged += (_, _) => UpdateOpticalProperties();
                }
                ScatteringTypeName = SelectedTissue.Scatterer.GetType().FullName;
                // Set the tissue type again for skin so it will set the PowerLaw A and B values correctly
                if (SelectedTissue.ScattererType == ScatteringType.PowerLaw && SelectedTissue.TissueType != TissueType.IntralipidPhantom)
                {
                    var myScatterer = (PowerLawScatterer)SelectedTissue.Scatterer;
                    myScatterer.SetTissueType(SelectedTissue.TissueType);
                }
            }
            OnPropertyChanged(nameof(Scatterer));
            UpdateOpticalProperties();
        };

        WavelengthRangeVm = new RangeViewModel(new DoubleRange(650.0, 1000.0, 36), StringLookup.GetLocalizedString("Measurement_nm"),
            IndependentVariableAxis.Wavelength, StringLookup.GetLocalizedString("Heading_WavelengthRange"));

        Tissues =
        [
            new Tissue(TissueType.Skin),
            new Tissue(TissueType.BrainWhiteMatter),
            new Tissue(TissueType.BrainGrayMatter),
            new Tissue(TissueType.BreastPreMenopause),
            new Tissue(TissueType.BreastPostMenopause),
            new Tissue(TissueType.Liver),
            new Tissue(TissueType.IntralipidPhantom),
            //new Tissue(TissueType.PolystyreneSpherePhantom),
            new Tissue(TissueType.Custom)
        ];

        BloodConcentrationVm = new BloodConcentrationViewModel();

        #region DC notes 1

        // DC NOTES on how to propagate the correct hemoglobin instances into BloodConcentrationVM:
        // Upon setting SelectedTissue (below), we internally update the BloodConcentrationVM hemoglobin references 
        // This is the simplest solution, but maybe violates SOC...(see SelectedTissue property for details)
        // A second alternative way would be to override AfterPropertyChanged (see AfterPropertyChanged method below)

        #endregion

        BloodConcentrationVm.PropertyChanged += (_, _) => UpdateOpticalProperties();

        SelectedTissue = Tissues[0];
        ScatteringTypeVm.SelectedValue = SelectedTissue.ScattererType;
        // forces update to all bindings established in handler for ScatteringTypeVM.PropertyChanged above
        ScatteringTypeName = SelectedTissue.Scatterer.GetType().FullName;

        OpticalProperties = new OpticalProperties(0.01, 1, 0.8, 1.4);
        Wavelength = 650;

        ResetConcentrations = new RelayCommand<object>(ResetConcentrations_Executed);
        UpdateWavelength = new RelayCommand<object>(UpdateWavelength_Executed);
        PlotMuaSpectrumCommand = new RelayCommand(PlotMuaSpectrum_Executed);
        PlotMuspSpectrumCommand = new RelayCommand(PlotMuspSpectrum_Executed);
    }

    public RelayCommand<object> ResetConcentrations { get; set; }
    public RelayCommand<object> UpdateWavelength { get; set; }
    public RelayCommand PlotMuaSpectrumCommand { get; set; }
    public RelayCommand PlotMuspSpectrumCommand { get; set; }

    /// <summary>
    ///     Simple pass-through for SelectedTissue.Scatterer
    ///     to allow simpler data binding in Views
    /// </summary>
    public IScatterer Scatterer => _selectedTissue.Scatterer;

    public string ScatteringTypeName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ScatteringTypeName));
        }
    }

    public OptionViewModel<ScatteringType> ScatteringTypeVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ScatteringTypeVm));
        }
    }

    public Tissue SelectedTissue
    {
        get => _selectedTissue;
        set
        {
            _selectedTissue = value;
            ScatteringTypeName = _selectedTissue.Scatterer.GetType().FullName;
            OnPropertyChanged(nameof(SelectedTissue));
            OnPropertyChanged(nameof(Scatterer));

            ScatteringTypeVm.Options[_selectedTissue.Scatterer.ScattererType].IsSelected = true;

            UpdateOpticalProperties();

            // update the BloodConcentrationViewModel to point to the IChromophoreAbsorber instances 
            // specified in the updated SelectedTissue
            var hb = _selectedTissue.Absorbers.FirstOrDefault(abs => abs.Name == "Hb");
            var hbO2 = _selectedTissue.Absorbers.FirstOrDefault(abs => abs.Name == "HbO2");

            // only assign the values if both queries return valid (non-null) instances of IChromophoreAbsorber
            if (hb != null && hbO2 != null)
            {
                BloodConcentrationVm.Hb = hb;
                BloodConcentrationVm.HbO2 = hbO2;
                BloodConcentrationVm.DisplayBloodVM = true;
            }
            else
                BloodConcentrationVm.DisplayBloodVM = false;
        }
    }

    public List<Tissue> Tissues
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Tissues));
        }
    }

    public double Wavelength
    {
        get => _wavelength;
        set
        {
            _wavelength = value;
            UpdateOpticalProperties();
            OnPropertyChanged(nameof(Wavelength));
        }
    }

    public OpticalProperties OpticalProperties { get; private set; }

    public double Mua
    {
        get => OpticalProperties.Mua;
        set
        {
            OpticalProperties.Mua = value;
            OnPropertyChanged(nameof(Mua));
        }
    }

    public double Musp
    {
        get => OpticalProperties.Musp;
        set
        {
            OpticalProperties.Musp = value;
            OnPropertyChanged(nameof(Musp));
            OnPropertyChanged(nameof(ScatteringTypeVm));
        }
    }

    public double G
    {
        get => OpticalProperties.G;
        set
        {
            OpticalProperties.G = value;
            OnPropertyChanged(nameof(G));
        }
    }

    public double N
    {
        get => OpticalProperties.N;
        set
        {
            OpticalProperties.N = value;
            OnPropertyChanged(nameof(N));
        }
    }

    public RangeViewModel WavelengthRangeVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(WavelengthRangeVm));
        }
    }

    public BloodConcentrationViewModel BloodConcentrationVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(BloodConcentrationVm));
            OnPropertyChanged(nameof(SelectedTissue));
        }
    }

    private void UpdateOpticalProperties()
    {
        OpticalProperties = SelectedTissue.GetOpticalProperties(Wavelength);
        OnPropertyChanged(nameof(Mua));
        OnPropertyChanged(nameof(Musp));
        OnPropertyChanged(nameof(G));
        OnPropertyChanged(nameof(N));
        OnPropertyChanged(nameof(OpticalProperties));
        WindowViewModel.Current.ForwardSolverVm.UpdateOpticalProperties_Executed();
    }

    private void ResetConcentrations_Executed(object obj)
    {
        var tissue = (Tissue)obj;
        tissue.Absorbers = TissueProvider.CreateAbsorbers(tissue.TissueType);
        SelectedTissue = tissue;
    }

    private void PlotMuaSpectrum_Executed()
    {
        const IndependentVariableAxis axisType = IndependentVariableAxis.Wavelength;
        const IndependentVariableAxisUnits axisUnits = IndependentVariableAxisUnits.NM;
        var axesLabels = new PlotAxesLabels(
            StringLookup.GetLocalizedString("Label_MuA"),
            StringLookup.GetLocalizedString("Measurement_Inv_mm"),
            new IndependentAxisViewModel
            {
                AxisType = axisType,
                AxisLabel = axisType.GetInternationalizedString(),
                AxisUnits = axisUnits.GetInternationalizedString(),
                AxisRangeVM = WavelengthRangeVm
            });

        WindowViewModel.Current.PlotVm.SetAxesLabels.Execute(axesLabels);

        var tissue = SelectedTissue;
        var wavelengths = WavelengthRangeVm.Values.ToArray();
        var points = new Point[wavelengths.Length];
        for (var wvi = 0; wvi < wavelengths.Length; wvi++)
        {
            var wavelength = wavelengths[wvi];
            points[wvi] = new Point(wavelength, tissue.GetMua(wavelength));
        }
        WindowViewModel.Current.PlotVm.PlotValues.Execute(new[] {new PlotData(points, StringLookup.GetLocalizedString("Label_MuASpectra"))});

        var minWavelength = WavelengthRangeVm.Values.Min();
        var maxWavelength = WavelengthRangeVm.Values.Max();
        WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
            StringLookup.GetLocalizedString("Message_PlotMuASpectrum") + "[" + minWavelength + ", " + maxWavelength + "]\r");
    }

    private void PlotMuspSpectrum_Executed()
    {
        const IndependentVariableAxis axisType = IndependentVariableAxis.Wavelength;
        const IndependentVariableAxisUnits axisUnits = IndependentVariableAxisUnits.NM;
        var axesLabels = new PlotAxesLabels(
            StringLookup.GetLocalizedString("Label_MuSPrime"),
            StringLookup.GetLocalizedString("Measurement_Inv_mm"),
            new IndependentAxisViewModel
            {
                AxisType = axisType,
                AxisLabel = axisType.GetInternationalizedString(),
                AxisUnits = axisUnits.GetInternationalizedString(),
                AxisRangeVM = WavelengthRangeVm
            });
        WindowViewModel.Current.PlotVm.SetAxesLabels.Execute(axesLabels);

        var tissue = SelectedTissue;
        var wavelengths = WavelengthRangeVm.Values.ToArray();
        var points = new Point[wavelengths.Length];
        for (var wvi = 0; wvi < wavelengths.Length; wvi++)
        {
            var wavelength = wavelengths[wvi];
            points[wvi] = new Point(wavelength, tissue.GetMusp(wavelength));
        }

        WindowViewModel.Current.PlotVm.PlotValues.Execute(new[] {new PlotData(points, StringLookup.GetLocalizedString("Label_MuSPrimeSpectra"))});

        var minWavelength = WavelengthRangeVm.Values.Min();
        var maxWavelength = WavelengthRangeVm.Values.Max();
        WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
            StringLookup.GetLocalizedString("Message_PlotMuSPrimeSpectrum") + "[" + minWavelength + ", " + maxWavelength + "]\r");
    }

    private void UpdateWavelength_Executed(object sender)
        // updates when solution domain is involved in spectral feedback
    {
        _wavelength = (double) sender;
        UpdateOpticalProperties();
        OnPropertyChanged(nameof(Wavelength));
    }

    #region DC notes 2

    //protected override void AfterPropertyChanged(string propertyName)
    //{
    //    if (propertyName == "SelectedTissue")
    //    {
    //        // update the BloodConcentrationViewModel to point to the IChromophoreAbsorber instances 
    //        // specified in the updated SelectedTissue
    //        var hb = _SelectedTissue.Absorbers.Where(abs => abs.Name == "Hb").FirstOrDefault();
    //        var hbO2 = _SelectedTissue.Absorbers.Where(abs => abs.Name == "HbO2").FirstOrDefault();
    //
    //        // only assign the values if both queries return valid (non-null) instances of IChromophoreAbsorber
    //        if (hb != null && hbO2 != null)
    //        {
    //            BloodConcentrationVM.Hb = hb;
    //            BloodConcentrationVM.HbO2 = hbO2;
    //        }
    //    }
    //    base.AfterPropertyChanged(propertyName);
    //}

    #endregion
}