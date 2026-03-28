using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using CommunityToolkit.Mvvm.Input;
using Vts.Extensions;
using Vts.Factories;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.Gui.Wpf.Resources;
using Vts.IO;

#if WHITELIST
using Vts.Gui.Wpf.ViewModel.Application;
#endif

namespace Vts.Gui.Wpf.ViewModel;

/// <summary>
///     View model implementing Inverse Solver panel functionality
/// </summary>
public class InverseSolverViewModel : BindableObject
{
    private RangeViewModel[] _allRangeVMs;

    private bool _showOpticalProperties;
    private bool _useSpectralPanelData;

    public InverseSolverViewModel()
    {
        _showOpticalProperties = true;
        _useSpectralPanelData = false;

        _allRangeVMs = [new RangeViewModel {Title = Strings.IndependentVariableAxis_Rho}];

        SolutionDomainTypeOptionVm = new SolutionDomainOptionViewModel("Solution Domain", SolutionDomainType.ROfRho)
            {
                EnableMultiAxis = false,
                AllowMultiAxis = false
            };

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
                    var useSpectralPanelDataAndNotNull = UseSpectralPanelData && WindowViewModel.Current != null &&
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
                                    ? WindowViewModel.Current.SpectralMappingVm.WavelengthRangeVM
                                    // bind to same instance, not a copy
                                    : SolutionDomainTypeOptionVm.IndependentAxesVMs[i].AxisRangeVM).ToArray();

                    // if the independent axis is wavelength, then hide optical properties (because they come from spectral panel)
                    ShowOpticalProperties =
                        !_allRangeVMs.Any(value => value.AxisType == IndependentVariableAxis.Wavelength);

                    // update solution domain wavelength constant if applicable
                    if (useSpectralPanelDataAndNotNull &&
                        SolutionDomainTypeOptionVm.ConstantAxesVMs.Any(
                            axis => axis.AxisType == IndependentVariableAxis.Wavelength))
                    {
                        updateSolutionDomainWithWavelength(WindowViewModel.Current.SpectralMappingVm.Wavelength);
                    }
                    break;
                }
            }
        };

#if WHITELIST
        MeasuredForwardSolverTypeOptionVm = new OptionViewModel<ForwardSolverType>(
            "Forward Model Engine",false, WhiteList.InverseForwardSolverTypes);
#else
        MeasuredForwardSolverTypeOptionVm = new OptionViewModel<ForwardSolverType>(
            "Forward Model Engine", false); //These titles are not displayed so we can hard-code strings
#endif

#if WHITELIST 
        InverseForwardSolverTypeOptionVm = new OptionViewModel<ForwardSolverType>("Inverse Model Engine",false, WhiteList.InverseForwardSolverTypes);
#else
        InverseForwardSolverTypeOptionVm = new OptionViewModel<ForwardSolverType>("Inverse Model Engine", false); //These titles are not displayed so we can hard-code strings
#endif
        InverseForwardSolverTypeOptionVm.PropertyChanged += (_, _) =>
            OnPropertyChanged(nameof(InverseForwardSolver));

        OptimizerTypeOptionVm = new OptionViewModel<OptimizerType>(StringLookup.GetLocalizedString("Heading_OptimizerType"), true);
        OptimizerTypeOptionVm.PropertyChanged += (_, _) =>
            OnPropertyChanged(nameof(Optimizer));

        InverseFitTypeOptionVm = new OptionViewModel<InverseFitType>(StringLookup.GetLocalizedString("Heading_OptimizationParameters"), true);

        MeasuredOpticalPropertyVm = new OpticalPropertyViewModel {Title = ""};
        InitialGuessOpticalPropertyVm = new OpticalPropertyViewModel {Title = ""};
        ResultOpticalPropertyVm = new OpticalPropertyViewModel {Title = ""};

        SimulateMeasuredDataCommand = new RelayCommand(() => SimulateMeasuredDataCommand_Executed());
        CalculateInitialGuessCommand = new RelayCommand(() => CalculateInitialGuessCommand_Executed());
        SolveInverseCommand = new RelayCommand(() => SolveInverseCommand_Executed());

        if (WindowViewModel.Current.SpectralMappingVm != null)
        {
            WindowViewModel.Current.SpectralMappingVm.PropertyChanged += (_, args) =>
            {
                switch (args.PropertyName)
                {
                    case "Wavelength":
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

                        break;
                    }
                    case "OpticalProperties":
                    {
                        //need to get the value from the checkbox in case UseSpectralPanelData has not yet been updated
                        if (SolutionDomainTypeOptionVm != null)
                        {
                            UseSpectralPanelData = SolutionDomainTypeOptionVm.UseSpectralInputs;
                        }
                        if (UseSpectralPanelData && WindowViewModel.Current != null &&
                            WindowViewModel.Current.SpectralMappingVm != null &&
                            MeasuredOpticalPropertyVm != null)
                        {
                            MeasuredOpticalPropertyVm.SetOpticalProperties(
                                WindowViewModel.Current.SpectralMappingVm.OpticalProperties);
                        }
                        break;
                    }
                }
            };
        }
    }

    public RelayCommand SimulateMeasuredDataCommand { get; set; }
    public RelayCommand CalculateInitialGuessCommand { get; set; }
    public RelayCommand SolveInverseCommand { get; set; }

    public SolutionDomainOptionViewModel SolutionDomainTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(SolutionDomainTypeOptionVm));
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

    public OptionViewModel<ForwardSolverType> MeasuredForwardSolverTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(MeasuredForwardSolverTypeOptionVm));
        }
    }

    public OptionViewModel<ForwardSolverType> InverseForwardSolverTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(InverseForwardSolverTypeOptionVm));
        }
    }

    public OptionViewModel<OptimizerType> OptimizerTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(OptimizerTypeOptionVm));
        }
    }

    public OptionViewModel<InverseFitType> InverseFitTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(InverseFitTypeOptionVm));
        }
    }

    public OpticalPropertyViewModel MeasuredOpticalPropertyVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(MeasuredOpticalPropertyVm));
        }
    }

    public bool UseSpectralPanelData // for measured data
    {
        get => _useSpectralPanelData;
        set
        {
            _useSpectralPanelData = value;
            OnPropertyChanged(nameof(UseSpectralPanelData));
        }
    }

    public bool ShowOpticalProperties // for measured data
    {
        get => _showOpticalProperties;
        set
        {
            _showOpticalProperties = value;
            OnPropertyChanged(nameof(ShowOpticalProperties));
        }
    }

    public OpticalPropertyViewModel InitialGuessOpticalPropertyVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(InitialGuessOpticalPropertyVm));
        }
    }

    public OpticalPropertyViewModel ResultOpticalPropertyVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ResultOpticalPropertyVm));
        }
    }

    public double PercentNoise
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(PercentNoise));
        }
    }

    public IForwardSolver MeasuredForwardSolver => SolverFactory.GetForwardSolver(MeasuredForwardSolverTypeOptionVm.SelectedValue);

    public IForwardSolver InverseForwardSolver => SolverFactory.GetForwardSolver(InverseForwardSolverTypeOptionVm.SelectedValue);

    public IOptimizer Optimizer => SolverFactory.GetOptimizer(OptimizerTypeOptionVm.SelectedValue);

    private void SimulateMeasuredDataCommand_Executed()
    {
        var measuredDataValues = GetSimulatedMeasuredData();
        var measuredDataPoints = GetDataPoints(measuredDataValues);

        //Report the results
        var axesLabels = GetPlotLabels();
        WindowViewModel.Current.PlotVm.SetAxesLabels.Execute(axesLabels);
        var plotLabels = GetLegendLabels(PlotDataType.Simulated);
        var plotData = measuredDataPoints.Zip(plotLabels, (p, el) => new PlotData(p, el)).ToArray();
        WindowViewModel.Current.PlotVm.PlotValues.Execute(plotData);
        WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute(StringLookup.GetLocalizedString("Label_SimulatedMeasuredData") + MeasuredOpticalPropertyVm + " \r");
    }

    private string[] GetLegendLabels(PlotDataType datatype)
    {
        string solverString;
        OpticalProperties opticalProperties;
        string modelString = null;
        ForwardSolverType forwardSolver;

        switch (datatype)
        {
            case PlotDataType.Simulated:
                solverString = "\n" + StringLookup.GetLocalizedString("Label_Simulated");
                opticalProperties = MeasuredOpticalPropertyVm.GetOpticalProperties();
                forwardSolver = MeasuredForwardSolverTypeOptionVm.SelectedValue;
                break;
            case PlotDataType.Calculated:
                solverString = "\n" + StringLookup.GetLocalizedString("Label_Calculated");
                opticalProperties = ResultOpticalPropertyVm.GetOpticalProperties();
                forwardSolver = MeasuredForwardSolverTypeOptionVm.SelectedValue;
                break;
            case PlotDataType.Guess:
                solverString = "\n" + StringLookup.GetLocalizedString("Label_Guess");
                opticalProperties = InitialGuessOpticalPropertyVm.GetOpticalProperties();
                forwardSolver = InverseForwardSolverTypeOptionVm.SelectedValue;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(datatype));
        }
        var opString = "\r" + StringLookup.GetLocalizedString("Label_MuA") + "=" + opticalProperties.Mua.ToString("F4") + " \r" + StringLookup.GetLocalizedString("Label_MuSPrime") + "=" +
                       opticalProperties.Musp.ToString("F4");

        switch (forwardSolver)
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

        if (_allRangeVMs.Length <= 1) return [solverString + modelString + opString];
        var isWavelengthPlot = _allRangeVMs.Any(vm => vm.AxisType == IndependentVariableAxis.Wavelength);
        var secondaryRangeVm = isWavelengthPlot
            ? _allRangeVMs.First(vm => vm.AxisType != IndependentVariableAxis.Wavelength)
            : _allRangeVMs.First(
                vm => vm.AxisType != IndependentVariableAxis.Time && 
                      vm.AxisType != IndependentVariableAxis.Ft);

        var secondaryAxesStrings =
            secondaryRangeVm.Values.Select(
                    value =>
                        "\r" + secondaryRangeVm.AxisType.GetInternationalizedString() + " = " + value.ToString(CultureInfo.InvariantCulture))
                .ToArray();
        return
            secondaryAxesStrings.Select(
                    sas => solverString + modelString + sas + (isWavelengthPlot ? "\r" + StringLookup.GetLocalizedString("Label_SpectralMuAMuSPrime") : opString))
                .ToArray();
    }

    private PlotAxesLabels GetPlotLabels()
    {
        var sd = SolutionDomainTypeOptionVm;
        var axesLabels = new PlotAxesLabels(
            sd.SelectedDisplayName, sd.SelectedValue.GetUnits(),
            sd.IndependentAxesVMs.First(),
            sd.ConstantAxesVMs);
        return axesLabels;
    }

    private void CalculateInitialGuessCommand_Executed()
    {
        var initialGuessDataValues = CalculateInitialGuess();
        var initialGuessDataPoints = GetDataPoints(initialGuessDataValues);

        //Report the results
        var axesLabels = GetPlotLabels();
        WindowViewModel.Current.PlotVm.SetAxesLabels.Execute(axesLabels);
        var plotLabels = GetLegendLabels(PlotDataType.Guess);
        var plotData = initialGuessDataPoints.Zip(plotLabels, (p, el) => new PlotData(p, el)).ToArray();
        WindowViewModel.Current.PlotVm.PlotValues.Execute(plotData);
        WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute(StringLookup.GetLocalizedString("Label_InitialGuess") +
                                                                            InitialGuessOpticalPropertyVm + " \r");
    }

    private void SolveInverseCommand_Executed()
    {
        // Report inverse solver setup and results
        WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute(StringLookup.GetLocalizedString("Label_InverseSolutionResults") + "\r");
        WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute("   " + StringLookup.GetLocalizedString("Label_OptimizationParameter") +
                                                                            InverseFitTypeOptionVm.SelectedValue +
                                                                            " \r");
        WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute("   " + StringLookup.GetLocalizedString("Label_InitialGuess") +
                                                                            InitialGuessOpticalPropertyVm + " \r");

        var inverseResult = SolveInverse();
        ResultOpticalPropertyVm.SetOpticalProperties(inverseResult.FitOpticalProperties.First());
            // todo: this only works for one set of properties

        //Report the results
        if (
            SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.SelectedValues.Contains(
                IndependentVariableAxis.Wavelength) &&
            inverseResult.FitOpticalProperties.Length > 1)
            // If multivalued OPs, the results aren't in the "scalar" VMs, need to parse OPs directly
        {
            var fitOPs = inverseResult.FitOpticalProperties;
            var measuredOPs = inverseResult.MeasuredOpticalProperties;
            var wavelengths = GetParameterValues(IndependentVariableAxis.Wavelength);
            var wvUnitString = IndependentVariableAxisUnits.NM.GetInternationalizedString();
            var opUnitString = IndependentVariableAxisUnits.InverseMM.GetInternationalizedString();
            var sb =
                new StringBuilder("\t[" + StringLookup.GetLocalizedString("Label_Wavelength") + " (" + wvUnitString +
                                  ")]\t\t\t\t\t\t[" + StringLookup.GetLocalizedString("Label_Exact") + "]\t\t\t\t\t\t[" + 
                                  StringLookup.GetLocalizedString("Label_ConvergedValues") + "]\t\t\t\t\t\t[" + StringLookup.GetLocalizedString("Label_Units") + "]\r");
            for (var i = 0; i < fitOPs.Length; i++)
            {
                sb.Append("\t" + wavelengths[i] + "\t\t\t\t\t\t" + measuredOPs[i] + "\t\t\t" + fitOPs[i] + "\t\t\t" +
                          opUnitString + " \r");
            }
            WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute(sb.ToString());
        }
        else
        {
            WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute("   " + StringLookup.GetLocalizedString("Label_Exact") + ": " +
                                                                                MeasuredOpticalPropertyVm + " \r");
            WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute("   " + StringLookup.GetLocalizedString("Label_ConvergedValues") + ": " +
                                                                                ResultOpticalPropertyVm + " \r");
                            //Display Percent Error
            double muaError = 0.0;
            double muspError = 0.0;
            if (MeasuredOpticalPropertyVm.Mua > 0)
            {
                int tempMuaError = (int)(10000.0 * Math.Abs(ResultOpticalPropertyVm.Mua - MeasuredOpticalPropertyVm.Mua) / MeasuredOpticalPropertyVm.Mua);
                muaError = tempMuaError / 100.0;
            }
            if (MeasuredOpticalPropertyVm.Musp > 0)
            {
                int tempMuspError = (int)(10000.0 * Math.Abs(ResultOpticalPropertyVm.Musp - MeasuredOpticalPropertyVm.Musp) / MeasuredOpticalPropertyVm.Musp);
                muspError = tempMuspError / 100.0;
            }
            WindowViewModel.Current.TextOutputVm.TextOutput_PostMessage.Execute("   " + StringLookup.GetLocalizedString("Label_PercentError") + 
                                                                                StringLookup.GetLocalizedString("Label_MuA") + " = " + muaError +
                                                                                "%  " + StringLookup.GetLocalizedString("Label_MuSPrime") + " = " + muspError + "% \r");
        }

        var axesLabels = GetPlotLabels();
        WindowViewModel.Current.PlotVm.SetAxesLabels.Execute(axesLabels);
        var plotLabels = GetLegendLabels(PlotDataType.Calculated);
        var plotData = inverseResult.FitDataPoints.Zip(plotLabels, (p, el) => new PlotData(p, el)).ToArray();
        WindowViewModel.Current.PlotVm.PlotValues.Execute(plotData);
    }

    private double[] GetSimulatedMeasuredData()
    {
        var opticalProperties = GetMeasuredOpticalProperties();

        var parameters = GetParametersInOrder(opticalProperties);

        var measuredData = ComputationFactory.ComputeReflectance(
            MeasuredForwardSolverTypeOptionVm.SelectedValue,
            SolutionDomainTypeOptionVm.SelectedValue,
            ForwardAnalysisType.R,
            parameters.Values.ToArray());

        return measuredData.AddNoise(PercentNoise);
    }

    private object GetMeasuredOpticalProperties()
    {
        return GetOpticalPropertiesFromSpectralPanel() ?? [MeasuredOpticalPropertyVm.GetOpticalProperties()];
    }

    private object GetInitialGuessOpticalProperties()
    {
        return GetDuplicatedListOfOpticalProperties() ??
               [InitialGuessOpticalPropertyVm.GetOpticalProperties()];
    }

    private OpticalProperties[] GetDuplicatedListOfOpticalProperties()
    {
        var initialGuessOPs = InitialGuessOpticalPropertyVm.GetOpticalProperties();
        if (!SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.SelectedValues.Contains(
                IndependentVariableAxis.Wavelength)) return null;
        var wavelengths = GetParameterValues(IndependentVariableAxis.Wavelength);
        return wavelengths.Select(_ => initialGuessOPs.Clone()).ToArray();
    }

    private OpticalProperties[] GetOpticalPropertiesFromSpectralPanel()
    {
        if (!SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.SelectedValues.Contains(
                IndependentVariableAxis.Wavelength) ||
            !UseSpectralPanelData ||
            WindowViewModel.Current == null ||
            WindowViewModel.Current.SpectralMappingVm == null) return null;
        var tissue = WindowViewModel.Current.SpectralMappingVm.SelectedTissue;
        var wavelengths = GetParameterValues(IndependentVariableAxis.Wavelength);
        var ops = tissue.GetOpticalProperties(wavelengths);

        return ops;
    }

    public double[] CalculateInitialGuess()
    {
        var opticalProperties = GetInitialGuessOpticalProperties();

        var parameters = GetParametersInOrder(opticalProperties);

        return ComputationFactory.ComputeReflectance(
            InverseForwardSolverTypeOptionVm.SelectedValue,
            SolutionDomainTypeOptionVm.SelectedValue,
            ForwardAnalysisType.R,
            parameters.Values.ToArray());
    }

    public InverseSolutionResult SolveInverse()
    {
        var lowerBounds = new double[] { 0, 0, 0, 0 };
        var upperBounds = new[] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity };

        var measuredOpticalProperties = GetMeasuredOpticalProperties();
        var measuredDataValues = GetSimulatedMeasuredData();

        var dependentValues = measuredDataValues.ToArray();
        var initGuessOpticalProperties = GetInitialGuessOpticalProperties();
        var initGuessParameters = GetParametersInOrder(initGuessOpticalProperties);

        // replace unconstrained L-M optimization with constrained version
        // this solves problem of when distributed source solution produces neg OPs during inversion
        //var fit = ComputationFactory.SolveInverse(
        //    InverseForwardSolverTypeOptionVM.SelectedValue,
        //    OptimizerTypeOptionVM.SelectedValue,
        //    SolutionDomainTypeOptionVM.SelectedValue,
        //    dependentValues,
        //    dependentValues, // set standard deviation, sd, to measured (works w/ or w/o noise)
        //    InverseFitTypeOptionVM.SelectedValue,
        //    initGuessParameters.Values.ToArray());

        var fit = ComputationFactory.SolveInverse(
            InverseForwardSolverTypeOptionVm.SelectedValue,
            OptimizerTypeOptionVm.SelectedValue,
            SolutionDomainTypeOptionVm.SelectedValue,
            dependentValues,
            dependentValues, // set standard deviation, sd, to measured (works w/ or w/o noise)
            InverseFitTypeOptionVm.SelectedValue,
            initGuessParameters.Values.ToArray(),
            lowerBounds, upperBounds);

        var fitOpticalProperties = ComputationFactory.UnFlattenOpticalProperties(fit);

        var fitParameters = GetParametersInOrder(fitOpticalProperties);

        var resultDataValues = ComputationFactory.ComputeReflectance(
            InverseForwardSolverTypeOptionVm.SelectedValue,
            SolutionDomainTypeOptionVm.SelectedValue,
            ForwardAnalysisType.R,
            fitParameters.Values.ToArray());

        var resultDataPoints = GetDataPoints(resultDataValues);

        return new InverseSolutionResult
        {
            FitDataPoints = resultDataPoints,
            MeasuredOpticalProperties = (OpticalProperties[]) measuredOpticalProperties,
            // todo: currently only supports homog OPs
            GuessOpticalProperties = (OpticalProperties[]) initGuessOpticalProperties,
            // todo: currently only supports homog OPss
            FitOpticalProperties = fitOpticalProperties
        };
    }

    private IDataPoint[][] GetDataPoints(double[] reflectance)
    {
        var plotIsVsWavelength = _allRangeVMs.Any(vm => vm.AxisType == IndependentVariableAxis.Wavelength);
        var isComplexPlot = ComputationFactory.IsComplexSolver(SolutionDomainTypeOptionVm.SelectedValue);
        var primaryIndependentValues = _allRangeVMs.First().Values.ToArray();
        var numPointsPerCurve = primaryIndependentValues.Length;
        var numForwardValues = isComplexPlot ? reflectance.Length/2 : reflectance.Length;
        // complex reported as all real numbers, then all imaginary numbers
        var numCurves = numForwardValues/numPointsPerCurve;

        var points = new IDataPoint[numCurves][];
        Func<int, int, IDataPoint> getReflectanceAtIndex = (i, j) =>
        {
            // man, this is getting hacky...
            var index = plotIsVsWavelength
                ? i*numCurves + j
                : j*numPointsPerCurve + i;
            return isComplexPlot
                ?
                    new ComplexDataPoint(primaryIndependentValues[i],
                        new Complex(reflectance[index], reflectance[index + numForwardValues]))
                : new DoubleDataPoint(primaryIndependentValues[i], reflectance[index]);
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
        var returnValue = new KeyValuePair<IndependentVariableAxis, object>(IndependentVariableAxis.Wavelength, opticalProperties)
                .AsEnumerable()
                .Concat(allParameters);
        return
            EnumerableExtensions.ToDictionary(returnValue);
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
                throw new ArgumentOutOfRangeException(nameof(axis));
        }
    }

    private double[] GetParameterValues(IndependentVariableAxis axis)
    {
        var isConstant = SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.UnSelectedValues.Contains(axis);
        if (isConstant)
        {
            //var positionIndex = SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.UnSelectedValues.IndexOf(axis);
            const int positionIndex = 0; //hard-coded for now
            return positionIndex switch
            {
                // add this back if hard-coding is removed
                // 1 => [SolutionDomainTypeOptionVm.ConstantAxesVMs[1].AxisValue],
                _ => [SolutionDomainTypeOptionVm.ConstantAxesVMs[0].AxisValue]
            };
        }
        else
        {
            var numAxes = SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVM.SelectedValues.Length;
            //var positionIndex = SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.IndexOf(axis);
            const int positionIndex = 0; //hard-coded for now
            return numAxes switch
            {
                2 => positionIndex switch
                {
                    // add this back if hard-coding is removed
                    //1 => AllRangeVMs[0].Values.ToArray(),
                    _ => AllRangeVMs[1].Values.ToArray()
                },
                3 => positionIndex switch
                {
                    // add these back if hard-coding is removed
                    //1 => AllRangeVMs[1].Values.ToArray(),
                    //2 => AllRangeVMs[0].Values.ToArray(),
                    _ => AllRangeVMs[2].Values.ToArray()
                },
                _ => AllRangeVMs[0].Values.ToArray()
            };
        }
    }

    private enum PlotDataType
    {
        Simulated,
        Guess,
        Calculated
    }

    public class InverseSolutionResult
    {
        public IDataPoint[][] FitDataPoints { get; set; }
        public OpticalProperties[] MeasuredOpticalProperties { get; set; }
        public OpticalProperties[] GuessOpticalProperties { get; set; }
        public OpticalProperties[] FitOpticalProperties { get; set; }
    }
}