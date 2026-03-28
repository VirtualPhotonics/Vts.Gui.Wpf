using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vts.Common;
using Vts.Extensions;
using Vts.Factories;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.IO;
using Vts.MonteCarlo.Tissues;

#if WHITELIST
using Vts.Gui.Wpf.ViewModel.Application;
#endif

namespace Vts.Gui.Wpf.ViewModel;

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

    private bool _showOpticalProperties;

    private object _tissueInputVm;
    // either an OpticalPropertyViewModel or a MultiRegionTissueViewModel is stored here, and dynamically displayed

    private bool _useSpectralPanelData;

    public ForwardSolverViewModel()
    {
        _showOpticalProperties = true;
        _useSpectralPanelData = false;


        _allRangeVMs = [new RangeViewModel {Title = StringLookup.GetLocalizedString("IndependentVariableAxis_Rho")}];

#if WHITELIST 
        ForwardSolverTypeOptionVm = new OptionViewModel<ForwardSolverType>("Forward Model",false, WhiteList.ForwardSolverTypes);
#else
        ForwardSolverTypeOptionVm = new OptionViewModel<ForwardSolverType>("Forward Model", false);
#endif
        SolutionDomainTypeOptionVm = new SolutionDomainOptionViewModel("Solution Domain", SolutionDomainType.ROfRho);

        ForwardAnalysisTypeOptionVm = new OptionViewModel<ForwardAnalysisType>(StringLookup.GetLocalizedString("Heading_ModelAnalysisOutput"), true);

        ForwardSolverTypeOptionVm.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsGaussianForwardModel));
            OnPropertyChanged(nameof(IsMultiRegion));
            OnPropertyChanged(nameof(IsSemiInfinite));
            TissueInputVm = GetTissueInputVm(IsMultiRegion ? "MultiLayer" : "SemiInfinite");
            UpdateAvailableOptions();
        };
        ForwardSolverTypeOptionVm.SelectedValue = ForwardSolverType.PointSourceSDA; // force the model choice here?

        Action<double> updateSolutionDomainWithWavelength = wv =>
        {
            var wvAxis =
                SolutionDomainTypeOptionVm.ConstantAxesVMs.FirstOrDefault(
                    axis => axis.AxisType == IndependentVariableAxis.Wavelength);
            wvAxis?.AxisValue = wv;
        };

        SolutionDomainTypeOptionVm.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case "UseSpectralInputs":
                    UseSpectralPanelData = SolutionDomainTypeOptionVm.UseSpectralInputs;
                    break;
                case "IndependentAxesVMs":
                {
                    var useSpectralPanelDataAndNotNull = SolutionDomainTypeOptionVm.UseSpectralInputs &&
                                                         WindowViewModel.Current != null &&
                                                         WindowViewModel.Current.SpectralMappingVm != null;

                    AllRangeVMs =
                        (from i in
                                Enumerable.Range(0,
                                    SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.SelectedValues.Length)
                            orderby i descending
                            // descending so that wavelength takes highest priority, then time/time frequency, then space/spatial frequency
                            select
                                useSpectralPanelDataAndNotNull &&
                                SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.SelectedValues[i] ==
                                IndependentVariableAxis.Wavelength
                                    ? WindowViewModel.Current.SpectralMappingVm.WavelengthRangeVm
                                    // bind to same instance, not a copy
                                    : SolutionDomainTypeOptionVm.IndependentAxesVMs[i].AxisRangeVM).ToArray();

                    // if the independent axis is wavelength, then hide optical properties (because they come from spectral panel)
                    ShowOpticalProperties =
                        _allRangeVMs.All(value => value.AxisType != IndependentVariableAxis.Wavelength);

                    // update solution domain wavelength constant if applicable
                    if (useSpectralPanelDataAndNotNull &&
                        WindowViewModel.Current != null &&
                        SolutionDomainTypeOptionVm.ConstantAxesVMs.Any(
                            axis => axis.AxisType == IndependentVariableAxis.Wavelength))
                    {
                        updateSolutionDomainWithWavelength(WindowViewModel.Current.SpectralMappingVm.Wavelength);
                    }

                    break;
                }
            }
        };

        ExecuteForwardSolverCommand = new RelayCommand(() => ExecuteForwardSolver_Executed());

        OpticalPropertyVm.PropertyChanged += (_, _) =>
        {
            //need to get the value from the checkbox in case UseSpectralPanelData has not yet been updated
            if (SolutionDomainTypeOptionVm != null)
            {
                UseSpectralPanelData = SolutionDomainTypeOptionVm.UseSpectralInputs;
            }
            if (UseSpectralPanelData && WindowViewModel.Current != null &&
                WindowViewModel.Current.SpectralMappingVm != null)
            {
                updateSolutionDomainWithWavelength(WindowViewModel.Current.SpectralMappingVm.Wavelength);
            }
        };

        if (WindowViewModel.Current.SpectralMappingVm != null)
        {
            WindowViewModel.Current.SpectralMappingVm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == "Wavelength")
                {
                    //need to get the value from the checkbox in case UseSpectralPanelData has not yet been updated
                    if (SolutionDomainTypeOptionVm != null)
                    {
                        UseSpectralPanelData = SolutionDomainTypeOptionVm.UseSpectralInputs;
                    }
                    if (UseSpectralPanelData && WindowViewModel.Current != null &&
                        WindowViewModel.Current.SpectralMappingVm != null)
                    {
                        updateSolutionDomainWithWavelength(WindowViewModel.Current.SpectralMappingVm.Wavelength);
                    }
                }

                if (args.PropertyName == "OpticalProperties")
                {
                    //need to get the value from the checkbox in case UseSpectralPanelData has not yet been updated
                    if (SolutionDomainTypeOptionVm != null)
                    {
                        UseSpectralPanelData = SolutionDomainTypeOptionVm.UseSpectralInputs;
                    }
                    if (UseSpectralPanelData && WindowViewModel.Current != null &&
                        WindowViewModel.Current.SpectralMappingVm != null)
                    {
                        if (IsMultiRegion && MultiRegionTissueVm != null)
                        {
                            MultiRegionTissueVm.RegionsVM.ForEach(region =>
                                ((dynamic) region).OpticalPropertyVM.SetOpticalProperties(
                                    WindowViewModel.Current.SpectralMappingVm.OpticalProperties));
                        }
                        else if (OpticalPropertyVm != null)
                        {
                            OpticalPropertyVm.SetOpticalProperties(
                                WindowViewModel.Current.SpectralMappingVm.OpticalProperties);
                        }
                    }
                }
            };
        }
    }

    public RelayCommand ExecuteForwardSolverCommand { get; set; }

    public IForwardSolver ForwardSolver => SolverFactory.GetForwardSolver(ForwardSolverTypeOptionVm.SelectedValue);

    public bool IsGaussianForwardModel => ForwardSolverTypeOptionVm.SelectedValue.IsGaussianForwardModel();

    public bool ShowOpticalProperties
    {
        get => _showOpticalProperties;
        set
        {
            _showOpticalProperties = value;
            OnPropertyChanged(nameof(ShowOpticalProperties));
        }
    }

    public bool IsMultiRegion => ForwardSolverTypeOptionVm.SelectedValue.IsMultiRegionForwardModel();

    public bool IsSemiInfinite => !ForwardSolverTypeOptionVm.SelectedValue.IsMultiRegionForwardModel();

    public bool UseSpectralPanelData
    {
        get => _useSpectralPanelData;
        set
        {
            _useSpectralPanelData = value;
            OnPropertyChanged(nameof(UseSpectralPanelData));
        }
    }

    public SolutionDomainOptionViewModel SolutionDomainTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(SolutionDomainTypeOptionVm));
        }
    }

    public OptionViewModel<ForwardSolverType> ForwardSolverTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ForwardSolverTypeOptionVm));
        }
    }

    public RangeViewModel[] AllRangeVMs
    {
        get => _allRangeVMs;
        set
        {
            _allRangeVMs = value;
            OnPropertyChanged(nameof(AllRangeVMs));
        }
    }

    public object TissueInputVm
    {
        get => _tissueInputVm;
        private set
        {
            _tissueInputVm = value;
            OnPropertyChanged(nameof(TissueInputVm));
        }
    }

    private OpticalPropertyViewModel OpticalPropertyVm => _tissueInputVm as OpticalPropertyViewModel;

    private MultiRegionTissueViewModel MultiRegionTissueVm => _tissueInputVm as MultiRegionTissueViewModel;

    public OptionViewModel<ForwardAnalysisType> ForwardAnalysisTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ForwardAnalysisTypeOptionVm));
        }
    }

    public void UpdateOpticalProperties_Executed()
    {
        //need to get the value from the checkbox in case UseSpectralPanelData has not yet been updated
        if (SolutionDomainTypeOptionVm != null)
        {
            UseSpectralPanelData = SolutionDomainTypeOptionVm.UseSpectralInputs;
        }

        if (!UseSpectralPanelData || WindowViewModel.Current == null ||
            WindowViewModel.Current.SpectralMappingVm == null) return;
        if (IsMultiRegion && MultiRegionTissueVm != null)
        {
            MultiRegionTissueVm.RegionsVM.ForEach(region =>
                ((dynamic) region).OpticalPropertyVM.SetOpticalProperties(
                    WindowViewModel.Current.SpectralMappingVm.OpticalProperties));
        }
        else
        {
            OpticalPropertyVm?.SetOpticalProperties(WindowViewModel.Current.SpectralMappingVm.OpticalProperties);
        }
    }

    private void ExecuteForwardSolver_Executed()
    {
        try
        {
            var points = ExecuteForwardSolver();
            var axesLabels = GetPlotLabels();
            WindowViewModel.Current.PlotVm.SetAxesLabels.Execute(axesLabels);

            var plotLabels = GetLegendLabels();

            var plotData = points.Zip(plotLabels, (p, el) => new PlotData(p, el)).ToArray();
            WindowViewModel.Current.PlotVm.PlotValues.Execute(plotData);
            WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute(
                StringLookup.GetLocalizedString("Label_ForwardSolver") + TissueInputVm + "\r");
        }
        catch (ArgumentException ex)
        {
            WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute(
                StringLookup.GetLocalizedString("Label_ForwardSolver\r"));
            WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute("ERROR IN INPUT:" + ex.Message + "\r");
        }
    }

    private PlotAxesLabels GetPlotLabels()
    {
        var sd = SolutionDomainTypeOptionVm;
        var axesLabels = new PlotAxesLabels(
            sd.SelectedDisplayName, sd.SelectedValue.GetUnits(),
            sd.IndependentAxesVMs.First(vm => vm.AxisType == AllRangeVMs.First().AxisType),
            sd.ConstantAxesVMs)
        {
            IsComplexPlot = ComputationFactory.IsComplexSolver(SolutionDomainTypeOptionVm.SelectedValue)
        };
        return axesLabels;
    }

    private void UpdateAvailableOptions()
    {
        if (SolutionDomainTypeOptionVm != null)
        {
            if (ForwardSolverTypeOptionVm.SelectedValue == ForwardSolverType.TwoLayerSDA)
            {
                // properties of AbstractSolutionDomainOptionViewModel: describe if shown
                SolutionDomainTypeOptionVm.AllowMultiAxis = false;
                SolutionDomainTypeOptionVm.UseSpectralInputs = false;
                // properties of this class: local state saved of properties above
                SolutionDomainTypeOptionVm.EnableMultiAxis = false;
                SolutionDomainTypeOptionVm.EnableSpectralPanelInputs = false;
            }
            else
            {
                // if not TwoLayerSDA enable both
                SolutionDomainTypeOptionVm.EnableMultiAxis = true;
                SolutionDomainTypeOptionVm.EnableSpectralPanelInputs = true;
            }
            // grey out time dependent solution domain options for DistributedGaussianSource since code not implemented
            if (ForwardSolverTypeOptionVm.SelectedValue == ForwardSolverType.DistributedGaussianSourceSDA)
            {
                SolutionDomainTypeOptionVm.IsROfRhoAndTimeEnabled = false;
                SolutionDomainTypeOptionVm.IsROfRhoAndFtEnabled = false;
                SolutionDomainTypeOptionVm.IsROfFxAndTimeEnabled = false;
                SolutionDomainTypeOptionVm.IsROfFxAndFtEnabled = false;
                if (SolutionDomainTypeOptionVm.SelectedValue == SolutionDomainType.ROfRhoAndTime ||
                    SolutionDomainTypeOptionVm.SelectedValue == SolutionDomainType.ROfRhoAndFt ||
                    SolutionDomainTypeOptionVm.SelectedValue == SolutionDomainType.ROfFxAndTime ||
                    SolutionDomainTypeOptionVm.SelectedValue == SolutionDomainType.ROfFxAndFt )
                {
                    SolutionDomainTypeOptionVm.SelectedValue = SolutionDomainType.ROfRho;
                    OnPropertyChanged(nameof(SolutionDomainTypeOptionVm));
                }
            }
            else
            {
                SolutionDomainTypeOptionVm.IsROfRhoAndTimeEnabled = true;
                SolutionDomainTypeOptionVm.IsROfRhoAndFtEnabled = true;
                SolutionDomainTypeOptionVm.IsROfFxAndTimeEnabled = true;
                SolutionDomainTypeOptionVm.IsROfFxAndFtEnabled = true;
            }
            //SolutionDomainTypeOptionVM.EnableMultiAxis =
            //    ForwardSolverTypeOptionVM.SelectedValue != ForwardSolverType.TwoLayerSDA;
            //SolutionDomainTypeOptionVM.EnableSpectralPanelInputs = 
            //    ForwardSolverTypeOptionVM.SelectedValue != ForwardSolverType.TwoLayerSDA;
        }

        if (ForwardAnalysisTypeOptionVm != null)
        {
            if (ForwardSolverTypeOptionVm.SelectedValue == ForwardSolverType.TwoLayerSDA)
            {
                ForwardAnalysisTypeOptionVm.Options[ForwardAnalysisType.R].IsSelected = true;
            }
            ForwardAnalysisTypeOptionVm.Options[ForwardAnalysisType.dRdMua].IsEnabled =
                ForwardSolverTypeOptionVm.SelectedValue != ForwardSolverType.TwoLayerSDA;
            ForwardAnalysisTypeOptionVm.Options[ForwardAnalysisType.dRdMusp].IsEnabled =
                ForwardSolverTypeOptionVm.SelectedValue != ForwardSolverType.TwoLayerSDA;
            ForwardAnalysisTypeOptionVm.Options[ForwardAnalysisType.dRdG].IsEnabled =
                ForwardSolverTypeOptionVm.SelectedValue != ForwardSolverType.TwoLayerSDA;
            ForwardAnalysisTypeOptionVm.Options[ForwardAnalysisType.dRdN].IsEnabled =
                ForwardSolverTypeOptionVm.SelectedValue != ForwardSolverType.TwoLayerSDA;
        } 
    }
    private object GetTissueInputVm(string tissueType)
    {
        // ops to use as the basis for instantiating multi-region tissues based on homogeneous values (for differential comparison)
        _currentHomogeneousOpticalProperties ??= new OpticalProperties(0.01, 1, 0.8, 1.4);

        switch (tissueType)
        {
            case "SemiInfinite":
                _currentSemiInfiniteTissueInput ??=
                        new SemiInfiniteTissueInput(
                            new SemiInfiniteTissueRegion(_currentHomogeneousOpticalProperties));
                return new OpticalPropertyViewModel(
                    _currentSemiInfiniteTissueInput.Regions.First().RegionOP,
                    IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(),
                    "Optical Properties");
            case "MultiLayer":
                _currentMultiLayerTissueInput ??= new MultiLayerTissueInput([
                    new LayerTissueRegion(new DoubleRange(0, 2), _currentHomogeneousOpticalProperties.Clone()),
                        new LayerTissueRegion(new DoubleRange(2, double.PositiveInfinity),
                            _currentHomogeneousOpticalProperties.Clone())
                ]);
                return new MultiRegionTissueViewModel(_currentMultiLayerTissueInput);
            case "SingleEllipsoid":
                _currentSingleEllipsoidTissueInput ??= new SingleEllipsoidTissueInput(
                        new EllipsoidTissueRegion(new Position(0, 0, 10), 5, 5, 5,
                            new OpticalProperties(0.05, 1.0, 0.8, 1.4)),
                        [
                            new LayerTissueRegion(new DoubleRange(0, double.PositiveInfinity),
                                _currentHomogeneousOpticalProperties.Clone())
                        ]);
                return new MultiRegionTissueViewModel(_currentSingleEllipsoidTissueInput);
            default:
                throw new ArgumentOutOfRangeException(nameof(tissueType));
        }
    }

    // todo: rename? this was to get a concise name for the legend
    private string[] GetLegendLabels()
    {
        string modelString = null;
        switch (ForwardSolverTypeOptionVm.SelectedValue)
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

        string opString;
        if (IsMultiRegion && MultiRegionTissueVm != null)
        {
            var regions = MultiRegionTissueVm.GetTissueInput().Regions;
            opString = "\r" + StringLookup.GetLocalizedString("Label_MuA1") + "=" + regions[0].RegionOP.Mua.ToString("F4") + "\r" + StringLookup.GetLocalizedString("Label_MuSPrime1") + "=" +
                       regions[0].RegionOP.Musp.ToString("F4") +
                       "\r" + StringLookup.GetLocalizedString("Label_MuA2") + "=" + regions[1].RegionOP.Mua.ToString("F4") + "\r" + StringLookup.GetLocalizedString("Label_MuSPrime2") + "=" +
                       regions[1].RegionOP.Musp.ToString("F4");
        }
        else
        {
            var opticalProperties = OpticalPropertyVm.GetOpticalProperties();
            opString = "\r" + StringLookup.GetLocalizedString("Label_MuA") + "=" + opticalProperties.Mua.ToString("F4") + " \r" + StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                       opticalProperties.Musp.ToString("F4");
        }

        if (_allRangeVMs.Length <= 1) return [modelString + opString];
        var isWavelengthPlot = _allRangeVMs.Any(vm => vm.AxisType == IndependentVariableAxis.Wavelength);
        var secondaryRangeVm = isWavelengthPlot
            ? _allRangeVMs.First(vm => vm.AxisType != IndependentVariableAxis.Wavelength)
            : _allRangeVMs
                .First(vm => vm.AxisType != IndependentVariableAxis.Time && vm.AxisType != IndependentVariableAxis.Ft);

        var secondaryAxesStrings =
            secondaryRangeVm.Values.Select(
                    value =>
                        "\r" + secondaryRangeVm.AxisType.GetInternationalizedString() + " = " + value)
                .ToArray();
        return
            secondaryAxesStrings.Select(
                sas => modelString + sas + (isWavelengthPlot ? "\r" + StringLookup.GetLocalizedString("Label_SpectralMuAMuSPrime") : opString)).ToArray();

    }

    public IDataPoint[][] ExecuteForwardSolver()
    {
        var opticalProperties = GetOpticalProperties();

        var parameters = GetParametersInOrder(opticalProperties);

        var reflectance = ComputationFactory.ComputeReflectance(
            ForwardSolverTypeOptionVm.SelectedValue,
            SolutionDomainTypeOptionVm.SelectedValue,
            ForwardAnalysisTypeOptionVm.SelectedValue,
            parameters.Values.ToArray());

        return GetDataPoints(reflectance);
    }

    private IDataPoint[][] GetDataPoints(double[] reflectance)
    {
        var plotIsVsWavelength = _allRangeVMs.Any(vm => vm.AxisType == IndependentVariableAxis.Wavelength);
        var isComplexPlot = ComputationFactory.IsComplexSolver(SolutionDomainTypeOptionVm.SelectedValue);
        var forwardAnalysisType = ForwardAnalysisTypeOptionVm.SelectedValue;
        var isDerivativePlot = ForwardAnalysisTypeOptionVm.SelectedValue != ForwardAnalysisType.R;
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
                return new DoubleDataPoint(primaryIndependentValues[i], reflectance[index]);
            if (isDerivativePlot)
            {
                // derivatives should be appended to real,imag, i.e. real,imag,dreal,dimag
                return new ComplexDerivativeDataPoint(primaryIndependentValues[i],
                    new Complex(reflectance[index], reflectance[index + numForwardValues]),
                    new Complex(reflectance[index + 2 * numForwardValues],
                        reflectance[index + 3 * numForwardValues]), forwardAnalysisType);
            }

            return new ComplexDataPoint(
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
    private static int GetParameterOrder(IndependentVariableAxis axis)
    {
        return axis switch
        {
            IndependentVariableAxis.Wavelength => 0,
            IndependentVariableAxis.Rho => 1,
            IndependentVariableAxis.Fx => 1,
            IndependentVariableAxis.Time => 2,
            IndependentVariableAxis.Ft => 2,
            IndependentVariableAxis.Z => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(axis))
        };
    }

    private double[] GetParameterValues(IndependentVariableAxis axis)
    {
        var isConstant = SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.UnSelectedValues.Contains(axis);
        if (isConstant)
        {
            var independentValues =
                SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.UnSelectedValues.Length;
            var positionIndex = 0;
            for (var i = 0; i < independentValues; i++)
            {
                if (SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.UnSelectedValues[i] == axis)
                {
                    positionIndex = i;
                }
            }

            return positionIndex switch
            {
                1 => [SolutionDomainTypeOptionVm.ConstantAxesVMs[1].AxisValue],
                _ => [SolutionDomainTypeOptionVm.ConstantAxesVMs[0].AxisValue]
            };
        }
        else
        {
            var numAxes = SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.SelectedValues.Length;
            var positionIndex = 0;
            for (var i = 0; i < numAxes; i++)
            {
                if (SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.SelectedValues[i] == axis)
                {
                    positionIndex = i;
                }
            }

            return numAxes switch
            {
                2 => positionIndex switch
                {
                    1 => AllRangeVMs[0].Values.ToArray(),
                    _ => AllRangeVMs[1].Values.ToArray()
                },
                3 => positionIndex switch
                {
                    1 => AllRangeVMs[1].Values.ToArray(),
                    2 => AllRangeVMs[0].Values.ToArray(),
                    _ => AllRangeVMs[2].Values.ToArray()
                },
                _ => AllRangeVMs[0].Values.ToArray()
            };
        }
    }

    private IDictionary<IndependentVariableAxis, object> GetParametersInOrder(object opticalProperties)
    {
        // get all parameters to get arrays of
        // then, for each one, decide if it's an IV or a constant
        // then, call the appropriate parameter generator, defined above
        var allParameters =
            from iv in
                SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.SelectedValues.Concat(
                    SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.UnSelectedValues)
            where iv != IndependentVariableAxis.Wavelength
            orderby GetParameterOrder(iv)
            select new KeyValuePair<IndependentVariableAxis, object>(iv, GetParameterValues(iv));

        // OPs are always first in the list
        var returnValue = new KeyValuePair<IndependentVariableAxis, object>(IndependentVariableAxis.Wavelength, opticalProperties).AsEnumerable().Concat(allParameters);

        // convert the parameters to a dictionary for return
        return EnumerableExtensions.ToDictionary(returnValue);
    }

    private object GetOpticalProperties()
    {
        if (
            SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.SelectedValues.Contains(
                IndependentVariableAxis.Wavelength) &&
            SolutionDomainTypeOptionVm.UseSpectralInputs &&
            WindowViewModel.Current != null &&
            WindowViewModel.Current.SpectralMappingVm != null)
        {
            var tissue = WindowViewModel.Current.SpectralMappingVm.SelectedTissue;
            var wavelengths = GetParameterValues(IndependentVariableAxis.Wavelength);
            var ops = tissue.GetOpticalProperties(wavelengths);

            if (IsMultiRegion && MultiRegionTissueVm != null)
            {
                return ops.Select(op =>
                {
                    var regions =
                        MultiRegionTissueVm.GetTissueInput()
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

        if (IsMultiRegion && MultiRegionTissueVm != null)
        {
            return new[]
            {
                MultiRegionTissueVm.GetTissueInput()
                    .Regions.Select(region => (IOpticalPropertyRegion) region)
                    .ToArray()
            };
        }

        return new[] {OpticalPropertyVm.GetOpticalProperties()};
    }
}