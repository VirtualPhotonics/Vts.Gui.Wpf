using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Input;
using Vts.Common;
using Vts.Extensions;
using Vts.Factories;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.IO;
using Vts.Modeling;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

#if WHITELIST
using Vts.Gui.Wpf.ViewModel.Application;
#endif

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary>
    ///     View model implementing Forward Solver panel functionality
    /// </summary>
    public class ForwardSolverViewModel : BindableObject
    {
        private RangeViewModel[] _allRangeVMs;

        // private fields to cache created instances of tissue inputs, created on-demand in GetTissueInputVM (vs up-front in constructor)
        private OpticalProperties _currentHomogeneousOpticalProperties;
        private MultiLayerTissueInput _currentMultiLayerTissueInput;
        private SemiInfiniteTissueInput _currentSemiInfiniteTissueInput;
        private SingleEllipsoidTissueInput _currentSingleEllipsoidTissueInput;
        private OptionViewModel<ForwardAnalysisType> _ForwardAnalysisTypeOptionVM;
        private OptionViewModel<ForwardSolverType> _ForwardSolverTypeOptionVM;

        private bool _showOpticalProperties;
        private SolutionDomainOptionViewModel _SolutionDomainTypeOptionVM;

        private object _tissueInputVM;
        // either an OpticalPropertyViewModel or a MultiRegionTissueViewModel is stored here, and dynamically displayed

        private bool _useSpectralPanelData;

        public ForwardSolverViewModel()
        {
            _showOpticalProperties = true;
            _useSpectralPanelData = false;


            _allRangeVMs = new[] {new RangeViewModel {Title = StringLookup.GetLocalizedString("IndependentVariableAxis_Rho")}};

#if WHITELIST 
            ForwardSolverTypeOptionVM = new OptionViewModel<ForwardSolverType>("Forward Model",false, WhiteList.ForwardSolverTypes);
#else
            ForwardSolverTypeOptionVM = new OptionViewModel<ForwardSolverType>("Forward Model", false);
#endif
            SolutionDomainTypeOptionVM = new SolutionDomainOptionViewModel("Solution Domain", SolutionDomainType.ROfRho);

            ForwardAnalysisTypeOptionVM = new OptionViewModel<ForwardAnalysisType>(StringLookup.GetLocalizedString("Heading_ModelAnalysisOutput"), true);

            ForwardSolverTypeOptionVM.PropertyChanged += (sender, args) =>
            {
                OnPropertyChanged("IsGaussianForwardModel");
                OnPropertyChanged("IsMultiRegion");
                OnPropertyChanged("IsSemiInfinite");
                TissueInputVM = GetTissueInputVM(IsMultiRegion ? "MultiLayer" : "SemiInfinite");
                UpdateAvailableOptions();
            };
            ForwardSolverTypeOptionVM.SelectedValue = ForwardSolverType.PointSourceSDA; // force the model choice here?

            Action<double> updateSolutionDomainWithWavelength = wv =>
            {
                var wvAxis =
                    SolutionDomainTypeOptionVM.ConstantAxesVMs.FirstOrDefault(
                        axis => axis.AxisType == IndependentVariableAxis.Wavelength);
                if (wvAxis != null)
                {
                    wvAxis.AxisValue = wv;
                }
            };

            SolutionDomainTypeOptionVM.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "UseSpectralInputs")
                {
                    UseSpectralPanelData = SolutionDomainTypeOptionVM.UseSpectralInputs;
                }
                if (args.PropertyName == "IndependentAxesVMs")
                {
                    var useSpectralPanelDataAndNotNull = SolutionDomainTypeOptionVM.UseSpectralInputs &&
                                                         WindowViewModel.Current != null &&
                                                         WindowViewModel.Current.SpectralMappingVM != null;

                    AllRangeVMs =
                        (from i in
                            Enumerable.Range(0,
                                SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.Length)
                            orderby i descending
                            // descending so that wavelength takes highest priority, then time/time frequency, then space/spatial frequency
                            select
                                useSpectralPanelDataAndNotNull &&
                                SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues[i] ==
                                IndependentVariableAxis.Wavelength
                                    ? WindowViewModel.Current.SpectralMappingVM.WavelengthRangeVM
                                    // bind to same instance, not a copy
                                    : SolutionDomainTypeOptionVM.IndependentAxesVMs[i].AxisRangeVM).ToArray();

                    // if the independent axis is wavelength, then hide optical properties (because they come from spectral panel)
                    ShowOpticalProperties =
                        _allRangeVMs.All(value => value.AxisType != IndependentVariableAxis.Wavelength);

                    // update solution domain wavelength constant if applicable
                    if (useSpectralPanelDataAndNotNull &&
                        WindowViewModel.Current != null &&
                        SolutionDomainTypeOptionVM.ConstantAxesVMs.Any(
                            axis => axis.AxisType == IndependentVariableAxis.Wavelength))
                    {
                        updateSolutionDomainWithWavelength(WindowViewModel.Current.SpectralMappingVM.Wavelength);
                    }
                }
            };

            ExecuteForwardSolverCommand = new RelayCommand(() => ExecuteForwardSolver_Executed(null, null));

            OpticalPropertyVM.PropertyChanged += (sender, args) =>
            {
                //need to get the value from the checkbox in case UseSpectralPanelData has not yet been updated
                if (SolutionDomainTypeOptionVM != null)
                {
                    UseSpectralPanelData = SolutionDomainTypeOptionVM.UseSpectralInputs;
                }
                if (UseSpectralPanelData && WindowViewModel.Current != null &&
                    WindowViewModel.Current.SpectralMappingVM != null)
                {
                    updateSolutionDomainWithWavelength(WindowViewModel.Current.SpectralMappingVM.Wavelength);
                }
            };

            if (WindowViewModel.Current.SpectralMappingVM != null)
            {
                WindowViewModel.Current.SpectralMappingVM.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "Wavelength")
                    {
                        //need to get the value from the checkbox in case UseSpectralPanelData has not yet been updated
                        if (SolutionDomainTypeOptionVM != null)
                        {
                            UseSpectralPanelData = SolutionDomainTypeOptionVM.UseSpectralInputs;
                        }
                        if (UseSpectralPanelData && WindowViewModel.Current != null &&
                            WindowViewModel.Current.SpectralMappingVM != null)
                        {
                            updateSolutionDomainWithWavelength(WindowViewModel.Current.SpectralMappingVM.Wavelength);
                        }
                    }

                    if (args.PropertyName == "OpticalProperties")
                    {
                        //need to get the value from the checkbox in case UseSpectralPanelData has not yet been updated
                        if (SolutionDomainTypeOptionVM != null)
                        {
                            UseSpectralPanelData = SolutionDomainTypeOptionVM.UseSpectralInputs;
                        }
                        if (UseSpectralPanelData && WindowViewModel.Current != null &&
                            WindowViewModel.Current.SpectralMappingVM != null)
                        {
                            if (IsMultiRegion && MultiRegionTissueVM != null)
                            {
                                MultiRegionTissueVM.RegionsVM.ForEach(region =>
                                    ((dynamic) region).OpticalPropertyVM.SetOpticalProperties(
                                        WindowViewModel.Current.SpectralMappingVM.OpticalProperties));
                            }
                            else if (OpticalPropertyVM != null)
                            {
                                OpticalPropertyVM.SetOpticalProperties(
                                    WindowViewModel.Current.SpectralMappingVM.OpticalProperties);
                            }
                        }
                    }
                };
            }
        }

        public RelayCommand ExecuteForwardSolverCommand { get; set; }

        public IForwardSolver ForwardSolver
        {
            get { return SolverFactory.GetForwardSolver(ForwardSolverTypeOptionVM.SelectedValue); }
        }

        public bool IsGaussianForwardModel
        {
            get { return ForwardSolverTypeOptionVM.SelectedValue.IsGaussianForwardModel(); }
        }

        public bool ShowOpticalProperties
        {
            get { return _showOpticalProperties; }
            set
            {
                _showOpticalProperties = value;
                OnPropertyChanged("ShowOpticalProperties");
            }
        }

        public bool IsMultiRegion
        {
            get { return ForwardSolverTypeOptionVM.SelectedValue.IsMultiRegionForwardModel(); }
        }

        public bool IsSemiInfinite
        {
            get { return !ForwardSolverTypeOptionVM.SelectedValue.IsMultiRegionForwardModel(); }
        }

        public bool UseSpectralPanelData
        {
            get { return _useSpectralPanelData; }
            set
            {
                _useSpectralPanelData = value;
                OnPropertyChanged("UseSpectralPanelData");
            }
        }

        public SolutionDomainOptionViewModel SolutionDomainTypeOptionVM
        {
            get { return _SolutionDomainTypeOptionVM; }
            set
            {
                _SolutionDomainTypeOptionVM = value;
                OnPropertyChanged("SolutionDomainTypeOptionVM");
            }
        }
        public OptionViewModel<ForwardSolverType> ForwardSolverTypeOptionVM
        {
            get { return _ForwardSolverTypeOptionVM; }
            set
            {
                _ForwardSolverTypeOptionVM = value;
                OnPropertyChanged("ForwardSolverTypeOptionVM");
            }
        }

        public RangeViewModel[] AllRangeVMs
        {
            get { return _allRangeVMs; }
            set
            {
                _allRangeVMs = value;
                OnPropertyChanged("AllRangeVMs");
            }
        }

        public object TissueInputVM
        {
            get { return _tissueInputVM; }
            private set
            {
                _tissueInputVM = value;
                OnPropertyChanged("TissueInputVM");
            }
        }

        private OpticalPropertyViewModel OpticalPropertyVM
        {
            get { return _tissueInputVM as OpticalPropertyViewModel; }
        }

        private MultiRegionTissueViewModel MultiRegionTissueVM
        {
            get { return _tissueInputVM as MultiRegionTissueViewModel; }
        }

        public OptionViewModel<ForwardAnalysisType> ForwardAnalysisTypeOptionVM
        {
            get { return _ForwardAnalysisTypeOptionVM; }
            set
            {
                _ForwardAnalysisTypeOptionVM = value;
                OnPropertyChanged("ForwardAnalysisTypeOptionVM");
            }
        }

        public void UpdateOpticalProperties_Executed()
        {
            //need to get the value from the checkbox in case UseSpectralPanelData has not yet been updated
            if (SolutionDomainTypeOptionVM != null)
            {
                UseSpectralPanelData = SolutionDomainTypeOptionVM.UseSpectralInputs;
            }
            if (UseSpectralPanelData && WindowViewModel.Current != null &&
                WindowViewModel.Current.SpectralMappingVM != null)
            {
                if (IsMultiRegion && MultiRegionTissueVM != null)
                {
                    MultiRegionTissueVM.RegionsVM.ForEach(region =>
                        ((dynamic) region).OpticalPropertyVM.SetOpticalProperties(
                            WindowViewModel.Current.SpectralMappingVM.OpticalProperties));
                }
                else if (OpticalPropertyVM != null)
                {
                    OpticalPropertyVM.SetOpticalProperties(WindowViewModel.Current.SpectralMappingVM.OpticalProperties);
                }
            }
        }

        private void ExecuteForwardSolver_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var points = ExecuteForwardSolver();
                var axesLabels = GetPlotLabels();
                WindowViewModel.Current.PlotVM.SetAxesLabels.Execute(axesLabels);

                var plotLabels = GetLegendLabels();

                var plotData = points.Zip(plotLabels, (p, el) => new PlotData(p, el)).ToArray();
                WindowViewModel.Current.PlotVM.PlotValues.Execute(plotData);
                WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(
                    StringLookup.GetLocalizedString("Label_ForwardSolver") + TissueInputVM + "\r");
            }
            catch (ArgumentException ex)
            {
                WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(
                    StringLookup.GetLocalizedString("Label_ForwardSolver\r"));
                WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute("ERROR IN INPUT:" + ex.Message + "\r");
            }
        }

        private PlotAxesLabels GetPlotLabels()
        {
            var sd = SolutionDomainTypeOptionVM;
            var axesLabels = new PlotAxesLabels(
                sd.SelectedDisplayName, sd.SelectedValue.GetUnits(),
                sd.IndependentAxesVMs.First(vm => vm.AxisType == AllRangeVMs.First().AxisType),
                sd.ConstantAxesVMs)
            {
                IsComplexPlot = ComputationFactory.IsComplexSolver(SolutionDomainTypeOptionVM.SelectedValue)
            };
            return axesLabels;
        }

        private void UpdateAvailableOptions()
        {
            if (SolutionDomainTypeOptionVM != null)
            {
                if (ForwardSolverTypeOptionVM.SelectedValue == ForwardSolverType.TwoLayerSDA)
                {
                    // properties of AbstractSolutionDomainOptionViewModel: describe if shown
                    SolutionDomainTypeOptionVM.AllowMultiAxis = false;
                    SolutionDomainTypeOptionVM.UseSpectralInputs = false;
                    // properties of this class: local state saved of properties above
                    SolutionDomainTypeOptionVM.EnableMultiAxis = false;
                    SolutionDomainTypeOptionVM.EnableSpectralPanelInputs = false;
                }
                else
                {
                    // if not TwoLayerSDA enable both
                    SolutionDomainTypeOptionVM.EnableMultiAxis = true;
                    SolutionDomainTypeOptionVM.EnableSpectralPanelInputs = true;
                }
                // grey out time dependent solution domain options for DistributedGaussianSource since code not implemented
                if (ForwardSolverTypeOptionVM.SelectedValue == ForwardSolverType.DistributedGaussianSourceSDA)
                {
                    SolutionDomainTypeOptionVM.IsROfRhoAndTimeEnabled = false;
                    SolutionDomainTypeOptionVM.IsROfRhoAndFtEnabled = false;
                    SolutionDomainTypeOptionVM.IsROfFxAndTimeEnabled = false;
                    SolutionDomainTypeOptionVM.IsROfFxAndFtEnabled = false;
                    if (SolutionDomainTypeOptionVM.SelectedValue == SolutionDomainType.ROfRhoAndTime ||
                        SolutionDomainTypeOptionVM.SelectedValue == SolutionDomainType.ROfRhoAndFt ||
                        SolutionDomainTypeOptionVM.SelectedValue == SolutionDomainType.ROfFxAndTime ||
                        SolutionDomainTypeOptionVM.SelectedValue == SolutionDomainType.ROfFxAndFt )
                    {
                        SolutionDomainTypeOptionVM.SelectedValue = SolutionDomainType.ROfRho;
                        OnPropertyChanged("SolutionDomainTypeOptionVM");
                    }
                }
                else
                {
                    SolutionDomainTypeOptionVM.IsROfRhoAndTimeEnabled = true;
                    SolutionDomainTypeOptionVM.IsROfRhoAndFtEnabled = true;
                    SolutionDomainTypeOptionVM.IsROfFxAndTimeEnabled = true;
                    SolutionDomainTypeOptionVM.IsROfFxAndFtEnabled = true;
                }
                //SolutionDomainTypeOptionVM.EnableMultiAxis =
                //    ForwardSolverTypeOptionVM.SelectedValue != ForwardSolverType.TwoLayerSDA;
                //SolutionDomainTypeOptionVM.EnableSpectralPanelInputs = 
                //    ForwardSolverTypeOptionVM.SelectedValue != ForwardSolverType.TwoLayerSDA;
            }

            if (ForwardAnalysisTypeOptionVM != null)
            {
                if (ForwardSolverTypeOptionVM.SelectedValue == ForwardSolverType.TwoLayerSDA)
                {
                    ForwardAnalysisTypeOptionVM.Options[ForwardAnalysisType.R].IsSelected = true;
                }
                ForwardAnalysisTypeOptionVM.Options[ForwardAnalysisType.dRdMua].IsEnabled =
                    ForwardSolverTypeOptionVM.SelectedValue != ForwardSolverType.TwoLayerSDA;
                ForwardAnalysisTypeOptionVM.Options[ForwardAnalysisType.dRdMusp].IsEnabled =
                    ForwardSolverTypeOptionVM.SelectedValue != ForwardSolverType.TwoLayerSDA;
                ForwardAnalysisTypeOptionVM.Options[ForwardAnalysisType.dRdG].IsEnabled =
                    ForwardSolverTypeOptionVM.SelectedValue != ForwardSolverType.TwoLayerSDA;
                ForwardAnalysisTypeOptionVM.Options[ForwardAnalysisType.dRdN].IsEnabled =
                    ForwardSolverTypeOptionVM.SelectedValue != ForwardSolverType.TwoLayerSDA;
            } 
        }
        private object GetTissueInputVM(string tissueType)
        {
            // ops to use as the basis for instantiating multi-region tissues based on homogeneous values (for differential comparison)
            if (_currentHomogeneousOpticalProperties == null)
            {
                _currentHomogeneousOpticalProperties = new OpticalProperties(0.01, 1, 0.8, 1.4);
            }

            switch (tissueType)
            {
                case "SemiInfinite":
                    if (_currentSemiInfiniteTissueInput == null)
                    {
                        _currentSemiInfiniteTissueInput =
                            new SemiInfiniteTissueInput(
                                new SemiInfiniteTissueRegion(_currentHomogeneousOpticalProperties));
                    }
                    return new OpticalPropertyViewModel(
                        _currentSemiInfiniteTissueInput.Regions.First().RegionOP,
                        IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(),
                        "Optical Properties");
                case "MultiLayer":
                    if (_currentMultiLayerTissueInput == null)
                    {
                        _currentMultiLayerTissueInput = new MultiLayerTissueInput(new ITissueRegion[]
                        {
                            new LayerTissueRegion(new DoubleRange(0, 2), _currentHomogeneousOpticalProperties.Clone()),
                            new LayerTissueRegion(new DoubleRange(2, double.PositiveInfinity),
                                _currentHomogeneousOpticalProperties.Clone())
                        });
                    }
                    return new MultiRegionTissueViewModel(_currentMultiLayerTissueInput);
                case "SingleEllipsoid":
                    if (_currentSingleEllipsoidTissueInput == null)
                    {
                        _currentSingleEllipsoidTissueInput = new SingleEllipsoidTissueInput(
                            new EllipsoidTissueRegion(new Position(0, 0, 10), 5, 5, 5,
                                new OpticalProperties(0.05, 1.0, 0.8, 1.4)),
                            new ITissueRegion[]
                            {
                                new LayerTissueRegion(new DoubleRange(0, double.PositiveInfinity),
                                    _currentHomogeneousOpticalProperties.Clone())
                            });
                    }
                    return new MultiRegionTissueViewModel(_currentSingleEllipsoidTissueInput);
                default:
                    throw new ArgumentOutOfRangeException(nameof(tissueType));
            }
        }

        // todo: rename? this was to get a concise name for the legend
        private string[] GetLegendLabels()
        {
            string modelString = null;
            switch (ForwardSolverTypeOptionVM.SelectedValue)
            {
                case ForwardSolverType.DistributedGaussianSourceSDA:
                case ForwardSolverType.DistributedPointSourceSDA:
                case ForwardSolverType.PointSourceSDA:
                    modelString = "\r" + StringLookup.GetLocalizedString("Label_ModelSDA");
                    break;
                case ForwardSolverType.MonteCarlo:
                    modelString = "\r" + StringLookup.GetLocalizedString("Label_ModelScaledMC");
                    break;
                case ForwardSolverType.Nurbs:
                    modelString = "\r" + StringLookup.GetLocalizedString("Label_ModelNurbs");
                    break;
                case ForwardSolverType.TwoLayerSDA:
                    modelString = "\r" + StringLookup.GetLocalizedString("Label_Model2LayerSDA");
                    break;
            }

            string opString = null;
            if (IsMultiRegion && MultiRegionTissueVM != null)
            {
                var regions = MultiRegionTissueVM.GetTissueInput().Regions;
                opString = "\r" + StringLookup.GetLocalizedString("Label_MuA1") + "=" + regions[0].RegionOP.Mua.ToString("F4") + "\r" + StringLookup.GetLocalizedString("Label_MuSPrime1") + "=" +
                           regions[0].RegionOP.Musp.ToString("F4") +
                           "\r" + StringLookup.GetLocalizedString("Label_MuA2") + "=" + regions[1].RegionOP.Mua.ToString("F4") + "\r" + StringLookup.GetLocalizedString("Label_MuSPrime2") + "=" +
                           regions[1].RegionOP.Musp.ToString("F4");
            }
            else
            {
                var opticalProperties = OpticalPropertyVM.GetOpticalProperties();
                opString = "\r" + StringLookup.GetLocalizedString("Label_MuA") + "=" + opticalProperties.Mua.ToString("F4") + " \r" + StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                           opticalProperties.Musp.ToString("F4");
            }

            if (_allRangeVMs.Length > 1)
            {
                var isWavelengthPlot = _allRangeVMs.Any(vm => vm.AxisType == IndependentVariableAxis.Wavelength);
                var secondaryRangeVM = isWavelengthPlot
                    ? _allRangeVMs.First(vm => vm.AxisType != IndependentVariableAxis.Wavelength)
                    : _allRangeVMs
                        .First(vm => vm.AxisType != IndependentVariableAxis.Time && vm.AxisType != IndependentVariableAxis.Ft);

                var secondaryAxesStrings =
                    secondaryRangeVM.Values.Select(
                        value =>
                            "\r" + secondaryRangeVM.AxisType.GetInternationalizedString() + " = " + value)
                        .ToArray();
                return
                    secondaryAxesStrings.Select(
                        sas => modelString + sas + (isWavelengthPlot ? "\r" + StringLookup.GetLocalizedString("Label_SpectralMuAMuSPrime") : opString)).ToArray();
            }

            return new[] {modelString + opString};
        }

        public IDataPoint[][] ExecuteForwardSolver()
        {
            var opticalProperties = GetOpticalProperties();

            var parameters = GetParametersInOrder(opticalProperties);

            var reflectance = ComputationFactory.ComputeReflectance(
                ForwardSolverTypeOptionVM.SelectedValue,
                SolutionDomainTypeOptionVM.SelectedValue,
                ForwardAnalysisTypeOptionVM.SelectedValue,
                parameters.Values.ToArray());

            return GetDataPoints(reflectance);
        }

        private IDataPoint[][] GetDataPoints(double[] reflectance)
        {
            var plotIsVsWavelength = _allRangeVMs.Any(vm => vm.AxisType == IndependentVariableAxis.Wavelength);
            var isComplexPlot = ComputationFactory.IsComplexSolver(SolutionDomainTypeOptionVM.SelectedValue);
            var forwardAnalysisType = ForwardAnalysisTypeOptionVM.SelectedValue;
            var isDerivativePlot = ForwardAnalysisTypeOptionVM.SelectedValue != ForwardAnalysisType.R;
            var primaryIndependentValues = _allRangeVMs[0].Values.ToArray();
            var numPointsPerCurve = primaryIndependentValues.Length;

            int numForwardValues;
            if (isComplexPlot)
            {
                if (isDerivativePlot) numForwardValues = reflectance.Length / 4;
                else numForwardValues = reflectance.Length / 2;
            }
            else numForwardValues = reflectance.Length;

            // complex reported as all reals, then all imaginaries
            var numCurves = numForwardValues/numPointsPerCurve;

            var points = new IDataPoint[numCurves][];
            Func<int, int, IDataPoint> getReflectanceAtIndex = (i, j) =>
            {
                // man, this is getting hacky...
                var index = plotIsVsWavelength
                    ? i * numCurves + j
                    : j * numPointsPerCurve + i;
                if (!isComplexPlot)
                    return (IDataPoint)new DoubleDataPoint(primaryIndependentValues[i], reflectance[index]);
                if (isDerivativePlot)
                {
                    // derivatives should be appended to real,imag, i.e. real,imag,dreal,dimag
                    return (IDataPoint)new ComplexDerivativeDataPoint(primaryIndependentValues[i],
                        new Complex(reflectance[index], reflectance[index + numForwardValues]),
                        new Complex(reflectance[index + 2 * numForwardValues],
                            reflectance[index + 3 * numForwardValues]), forwardAnalysisType);
                }

                return (IDataPoint)new ComplexDataPoint(
                    primaryIndependentValues[i],
                    new Complex(reflectance[index], reflectance[index + numForwardValues]));
            };

            for (var j = 0; j < numCurves; j++)
            {
                points[j] = new IDataPoint[numPointsPerCurve];
                for (var i = 0; i < numPointsPerCurve; i++)
                {
                    points[j][i] = getReflectanceAtIndex(i, j);
                }
            }
            return points;
        }

        /// <summary>
        ///     Function to provide ordering information for assembling forward calls
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        private int GetParameterOrder(IndependentVariableAxis axis)
        {
            switch (axis)
            {
                case IndependentVariableAxis.Wavelength:
                    return 0;
                case IndependentVariableAxis.Rho:
                    return 1;
                case IndependentVariableAxis.Fx:
                    return 1;
                case IndependentVariableAxis.Time:
                    return 2;
                case IndependentVariableAxis.Ft:
                    return 2;
                case IndependentVariableAxis.Z:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException("axis");
            }
        }

        private double[] GetParameterValues(IndependentVariableAxis axis)
        {
            var isConstant = SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.UnSelectedValues.Contains(axis);
            if (isConstant)
            {
                var independentValues =
                    SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.UnSelectedValues.Length;
                var positionIndex = 0;
                for (var i = 0; i < independentValues; i++)
                {
                    if (SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.UnSelectedValues[i] == axis)
                    {
                        positionIndex = i;
                    }
                }
                switch (positionIndex)
                {
                    default:
                        return new[] {SolutionDomainTypeOptionVM.ConstantAxesVMs[0].AxisValue};
                    case 1:
                        return new[] {SolutionDomainTypeOptionVM.ConstantAxesVMs[1].AxisValue};
                }
            }
            else
            {
                var numAxes = SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.Length;
                var positionIndex = 0;
                for (var i = 0; i < numAxes; i++)
                {
                    if (SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues[i] == axis)
                    {
                        positionIndex = i;
                    }
                }
                switch (numAxes)
                {
                    default:
                        return AllRangeVMs[0].Values.ToArray();
                    case 2:
                        switch (positionIndex)
                        {
                            default:
                                return AllRangeVMs[1].Values.ToArray();
                            case 1:
                                return AllRangeVMs[0].Values.ToArray();
                        }
                    case 3:
                        switch (positionIndex)
                        {
                            default:
                                return AllRangeVMs[2].Values.ToArray();
                            case 1:
                                return AllRangeVMs[1].Values.ToArray();
                            case 2:
                                return AllRangeVMs[0].Values.ToArray();
                        }
                }
            }
        }

        private IDictionary<IndependentVariableAxis, object> GetParametersInOrder(object opticalProperties)
        {
            // get all parameters to get arrays of
            // then, for each one, decide if it's an IV or a constant
            // then, call the appropriate parameter generator, defined above
            var allParameters =
                from iv in
                    SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.Concat(
                        SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.UnSelectedValues)
                where iv != IndependentVariableAxis.Wavelength
                orderby GetParameterOrder(iv)
                select new KeyValuePair<IndependentVariableAxis, object>(iv, GetParameterValues(iv));

            // OPs are always first in the list
            return
                new KeyValuePair<IndependentVariableAxis, object>(IndependentVariableAxis.Wavelength, opticalProperties)
                    .AsEnumerable()
                    .Concat(allParameters).ToDictionary();
        }

        private object GetOpticalProperties()
        {
            if (
                SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.Contains(
                    IndependentVariableAxis.Wavelength) &&
                SolutionDomainTypeOptionVM.UseSpectralInputs &&
                WindowViewModel.Current != null &&
                WindowViewModel.Current.SpectralMappingVM != null)
            {
                var tissue = WindowViewModel.Current.SpectralMappingVM.SelectedTissue;
                var wavelengths = GetParameterValues(IndependentVariableAxis.Wavelength);
                var ops = tissue.GetOpticalProperties(wavelengths);

                if (IsMultiRegion && MultiRegionTissueVM != null)
                {
                    return ops.Select(op =>
                    {
                        var regions =
                            MultiRegionTissueVM.GetTissueInput()
                                .Regions.Select(region => (IOpticalPropertyRegion) region)
                                .ToArray();
                        regions.ForEach(region =>
                        {
                            region.RegionOP.Mua = op.Mua;
                            region.RegionOP.Musp = op.Musp;
                            region.RegionOP.G = op.G;
                            region.RegionOP.N = op.N;
                        });
                        return regions.ToArray();
                    });
                }

                return ops;
            }

            if (IsMultiRegion && MultiRegionTissueVM != null)
            {
                return new[]
                {
                    MultiRegionTissueVM.GetTissueInput()
                        .Regions.Select(region => (IOpticalPropertyRegion) region)
                        .ToArray()
                };
            }

            return new[] {OpticalPropertyVM.GetOpticalProperties()};
        }
    }
}