using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using Vts.Common;
using Vts.Factories;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.SpectralMapping;

#if WHITELIST
using Vts.Gui.Wpf.ViewModel.Application;
#endif

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary>
    ///     View model implementing Spectral panel functionality
    /// </summary>
    public class SpectralMappingViewModel : BindableObject
    {
        private BloodConcentrationViewModel _bloodConcentrationVM;
        private double _g;
        private double _mua;
        private double _musp;
        private IScatterer _scatterer;
        private string _scatteringTypeName;
        private OptionViewModel<ScatteringType> _scatteringTypeVM;
        private Tissue _selectedTissue;
        private List<Tissue> _tissues;
        private double _wavelength;
        private RangeViewModel _wavelengthRangeVM;

        public SpectralMappingViewModel()
        {
#if WHITELIST 
            ScatteringTypeVM = new OptionViewModel<ScatteringType>(StringLookup.GetLocalizedString("Heading_ScattererType"), true, WhiteList.ScatteringTypes);
#else
            ScatteringTypeVM = new OptionViewModel<ScatteringType>(StringLookup.GetLocalizedString("Heading_ScattererType"), true);
#endif
            ScatteringTypeVM.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SelectedValue" && SelectedTissue != null)
                    //SelectedTissue.ScattererType != ScatteringTypeVM.SelectedValue)
                {
                    SelectedTissue.Scatterer = SolverFactory.GetScattererType(ScatteringTypeVM.SelectedValue);
                    var bindableScatterer = SelectedTissue.Scatterer as INotifyPropertyChanged;
                    if (bindableScatterer != null)
                    {
                        bindableScatterer.PropertyChanged += (s, a) => UpdateOpticalProperties();
                    }
                    //LM - Temporary Fix to reset the tissue type after a new scatterer is created
                    if (SelectedTissue.ScattererType == ScatteringType.PowerLaw)
                    {
                        var myScatterer = (PowerLawScatterer) SelectedTissue.Scatterer;
                        myScatterer.SetTissueType(SelectedTissue.TissueType);
                    }
                    ScatteringTypeName = SelectedTissue.Scatterer.GetType().FullName;
                }
                OnPropertyChanged("Scatterer");
                UpdateOpticalProperties();
            };

            WavelengthRangeVM = new RangeViewModel(new DoubleRange(650.0, 1000.0, 36), StringLookup.GetLocalizedString("Measurement_nm"),
                IndependentVariableAxis.Wavelength, StringLookup.GetLocalizedString("Heading_WavelengthRange"));

            Tissues = new List<Tissue>
            {
                new Tissue(TissueType.Skin),
                new Tissue(TissueType.BrainWhiteMatter),
                new Tissue(TissueType.BrainGrayMatter),
                new Tissue(TissueType.BreastPreMenopause),
                new Tissue(TissueType.BreastPostMenopause),
                new Tissue(TissueType.Liver),
                new Tissue(TissueType.IntralipidPhantom),
                //new Tissue(TissueType.PolystyreneSpherePhantom),
                new Tissue(TissueType.Custom)
            };

            BloodConcentrationVM = new BloodConcentrationViewModel();

            #region DC notes 1

            // DC NOTES on how to propagate the correct hemoglobin instances into BloodConcentrationVM:
            // Upon setting SelectedTissue (below), we internally update the BloodConcentrationVM hemoglobin references 
            // This is the simplest solution, but maybe violates SOC...(see SelectedTissue property for details)
            // A second alternative way would be to override AfterPropertyChanged (see AfterPropertyChanged method below)

            #endregion

            BloodConcentrationVM.PropertyChanged += (sender, args) => UpdateOpticalProperties();

            SelectedTissue = Tissues.First();
            ScatteringTypeVM.SelectedValue = SelectedTissue.ScattererType;
                // forces update to all bindings established in hanlder for ScatteringTypeVM.PropertyChanged above
            ScatteringTypeName = SelectedTissue.GetType().FullName;
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
        public IScatterer Scatterer
        {
            get { return _selectedTissue.Scatterer; }
        }

        public string ScatteringTypeName
        {
            get { return _scatteringTypeName; }
            set
            {
                _scatteringTypeName = value;
                OnPropertyChanged("ScatteringTypeName");
            }
        }

        public OptionViewModel<ScatteringType> ScatteringTypeVM
        {
            get { return _scatteringTypeVM; }
            set
            {
                _scatteringTypeVM = value;
                OnPropertyChanged("ScatteringTypeVM");
            }
        }

        public Tissue SelectedTissue
        {
            get { return _selectedTissue; }
            set
            {
                // var realScatterer = value.Scatterer;

                _selectedTissue = value;
                OnPropertyChanged("SelectedTissue");
                OnPropertyChanged("Scatterer");

                ScatteringTypeVM.Options[_selectedTissue.Scatterer.ScattererType].IsSelected = true;
                ScatteringTypeName = _selectedTissue.Scatterer.GetType().FullName;

                UpdateOpticalProperties();

                // update the BloodConcentrationViewModel to point to the IChromophoreAbsorber instances 
                // specified in the updated SelectedTissue
                var hb = _selectedTissue.Absorbers.Where(abs => abs.Name == "Hb").FirstOrDefault();
                var hbO2 = _selectedTissue.Absorbers.Where(abs => abs.Name == "HbO2").FirstOrDefault();

                // only assign the values if both queries return valid (non-null) instances of IChromophoreAbsorber
                if (hb != null && hbO2 != null)
                {
                    BloodConcentrationVM.Hb = hb;
                    BloodConcentrationVM.HbO2 = hbO2;
                    BloodConcentrationVM.DisplayBloodVM = true;
                }
                else
                    BloodConcentrationVM.DisplayBloodVM = false;
            }
        }

        public List<Tissue> Tissues
        {
            get { return _tissues; }
            set
            {
                _tissues = value;
                OnPropertyChanged("Tissues");
            }
        }

        public double Wavelength
        {
            get { return _wavelength; }
            set
            {
                _wavelength = value;
                UpdateOpticalProperties();
                OnPropertyChanged("Wavelength");
            }
        }

        public OpticalProperties OpticalProperties { get; private set; }

        public double Mua
        {
            get { return OpticalProperties.Mua; }
            set
            {
                OpticalProperties.Mua = value;
                OnPropertyChanged("Mua");
            }
        }

        public double Musp
        {
            get { return OpticalProperties.Musp; }
            set
            {
                OpticalProperties.Musp = value;
                OnPropertyChanged("Musp");
                OnPropertyChanged("ScatteringTypeVM");
            }
        }

        public double G
        {
            get { return OpticalProperties.G; }
            set
            {
                OpticalProperties.G = value;
                OnPropertyChanged("G");
            }
        }

        public double N
        {
            get { return OpticalProperties.N; }
            set
            {
                OpticalProperties.N = value;
                OnPropertyChanged("N");
            }
        }

        public RangeViewModel WavelengthRangeVM
        {
            get { return _wavelengthRangeVM; }
            set
            {
                _wavelengthRangeVM = value;
                OnPropertyChanged("WavelengthRangeVM");
            }
        }

        public BloodConcentrationViewModel BloodConcentrationVM
        {
            get { return _bloodConcentrationVM; }
            set
            {
                _bloodConcentrationVM = value;
                OnPropertyChanged("BloodConcentrationVM");
                OnPropertyChanged("SelectedTissue");
            }
        }

        private void UpdateOpticalProperties()
        {
            OpticalProperties = SelectedTissue.GetOpticalProperties(Wavelength);
            OnPropertyChanged("Mua");
            OnPropertyChanged("Musp");
            OnPropertyChanged("G");
            OnPropertyChanged("N");
            OnPropertyChanged("OpticalProperties");
            WindowViewModel.Current.ForwardSolverVM.UpdateOpticalProperties_Executed();
            //Commands.Spec_UpdateOpticalProperties.Execute(OpticalProperties, null);
        }

        private void ResetConcentrations_Executed(object obj)
        {
            var _tissue = (Tissue)obj;
            _tissue.Absorbers = TissueProvider.CreateAbsorbers(_tissue.TissueType);
            SelectedTissue = _tissue;
        }

        private void PlotMuaSpectrum_Executed()
        {
            var axisType = IndependentVariableAxis.Wavelength;
            var axisUnits = IndependentVariableAxisUnits.NM;
            var axesLabels = new PlotAxesLabels(
                StringLookup.GetLocalizedString("Label_MuA"),
                StringLookup.GetLocalizedString("Measurement_Inv_mm"),
                new IndependentAxisViewModel
                {
                    AxisType = axisType,
                    AxisLabel = axisType.GetInternationalizedString(),
                    AxisUnits = axisUnits.GetInternationalizedString(),
                    AxisRangeVM = WavelengthRangeVM
                });

            WindowViewModel.Current.PlotVM.SetAxesLabels.Execute(axesLabels);
            //Commands.Plot_SetAxesLabels.Execute(axesLabels, null);

            var tissue = SelectedTissue;
            var wavelengths = WavelengthRangeVM.Values.ToArray();
            var points = new Point[wavelengths.Length];
            for (var wvi = 0; wvi < wavelengths.Length; wvi++)
            {
                var wavelength = wavelengths[wvi];
                points[wvi] = new Point(wavelength, tissue.GetMua(wavelength));
            }
            WindowViewModel.Current.PlotVM.PlotValues.Execute(new[] {new PlotData(points, StringLookup.GetLocalizedString("Label_MuASpectra"))});

            var minWavelength = WavelengthRangeVM.Values.Min();
            var maxWavelength = WavelengthRangeVM.Values.Max();
            WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(
                StringLookup.GetLocalizedString("Message_PlotMuASpectrum") + "[" + minWavelength + ", " + maxWavelength + "]\r");
        }

        private void PlotMuspSpectrum_Executed()
        {
            var axisType = IndependentVariableAxis.Wavelength;
            var axisUnits = IndependentVariableAxisUnits.NM;
            var axesLabels = new PlotAxesLabels(
                StringLookup.GetLocalizedString("Label_MuSPrime"),
                StringLookup.GetLocalizedString("Measurement_Inv_mm"),
                new IndependentAxisViewModel
                {
                    AxisType = axisType,
                    AxisLabel = axisType.GetInternationalizedString(),
                    AxisUnits = axisUnits.GetInternationalizedString(),
                    AxisRangeVM = WavelengthRangeVM
                });
            WindowViewModel.Current.PlotVM.SetAxesLabels.Execute(axesLabels);

            var tissue = SelectedTissue;
            var wavelengths = WavelengthRangeVM.Values.ToArray();
            var points = new Point[wavelengths.Length];
            for (var wvi = 0; wvi < wavelengths.Length; wvi++)
            {
                var wavelength = wavelengths[wvi];
                points[wvi] = new Point(wavelength, tissue.GetMusp(wavelength));
            }

            WindowViewModel.Current.PlotVM.PlotValues.Execute(new[] {new PlotData(points, StringLookup.GetLocalizedString("Label_MuSPrimeSpectra"))});

            var minWavelength = WavelengthRangeVM.Values.Min();
            var maxWavelength = WavelengthRangeVM.Values.Max();
            WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(
                StringLookup.GetLocalizedString("Message_PlotMuSPrimeSpectrum") + "[" + minWavelength + ", " + maxWavelength + "]\r");
        }

        private void UpdateWavelength_Executed(object sender)
            // updates when solution domain is involved in spectral feedback
        {
            _wavelength = (double) sender;
            UpdateOpticalProperties();
            OnPropertyChanged("Wavelength");
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
}