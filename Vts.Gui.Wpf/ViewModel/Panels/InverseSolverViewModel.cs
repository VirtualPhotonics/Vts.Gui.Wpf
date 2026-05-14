using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Vts.Extensions;
using Vts.Factories;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.Gui.Wpf.Resources;
using Vts.Gui.Wpf.ViewModel.Controls;
using Vts.Gui.Wpf.ViewModel.Panels.SubPanels;
using Vts.IO;
#if WHITELIST
using Vts.Gui.Wpf.ViewModel.Application;
#endif

namespace Vts.Gui.Wpf.ViewModel.Panels;

/// <summary>
///     View model implementing Inverse Solver panel functionality
/// </summary>
public class InverseSolverViewModel : BaseSolverViewModel
{
    public InverseSolverViewModel()
    {
        ShowOpticalProperties = true;
        UseSpectralPanelData = false;

        AllRangeVMs = [new RangeViewModel {Title = Strings.IndependentVariableAxis_Rho}];

        SolutionDomainTypeOptionVm = new SolutionDomainOptionViewModel("Solution Domain", SolutionDomainType.ROfRho)
            {
                EnableSpectralPanelInputs = false,
                EnableMultiAxis = false,
                AllowMultiAxis = false
            };

        SolutionDomainTypeOptionVm.PropertyChanged += SolutionDomainTypeOptionVm_PropertyChanged;

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

        SimulateMeasuredDataCommand = new RelayCommand(SimulateMeasuredDataCommand_Executed);
        CalculateInitialGuessCommand = new RelayCommand(CalculateInitialGuessCommand_Executed);
        SolveInverseCommand = new RelayCommand(SolveInverseCommand_Executed);

        MeasuredOpticalPropertyVm.PropertyChanged += OpticalPropertyVm_PropertyChanged;
    }

    public RelayCommand SimulateMeasuredDataCommand { get; set; }
    public RelayCommand CalculateInitialGuessCommand { get; set; }
    public RelayCommand SolveInverseCommand { get; set; }

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

    public IForwardSolver InverseForwardSolver => SolverFactory.GetForwardSolver(InverseForwardSolverTypeOptionVm.SelectedValue);

    public IOptimizer Optimizer => SolverFactory.GetOptimizer(OptimizerTypeOptionVm.SelectedValue);

    public void UpdateOpticalProperties_Executed()
    {
        //need to get the value from the checkbox in case UseSpectralPanelData has not yet been updated
        if (SolutionDomainTypeOptionVm != null)
        {
            UseSpectralPanelData = SolutionDomainTypeOptionVm.UseSpectralInputs;
        }

        if (!UseSpectralPanelData || WindowViewModel.Current == null ||
            WindowViewModel.Current.SpectralMappingVm == null) return;
        InitialGuessOpticalPropertyVm?.SetOpticalProperties(WindowViewModel.Current.SpectralMappingVm.OpticalProperties);
        MeasuredOpticalPropertyVm?.SetOpticalProperties(WindowViewModel.Current.SpectralMappingVm.OpticalProperties);
    }

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
        WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(StringLookup.GetLocalizedString("Label_SimulatedMeasuredData") + MeasuredOpticalPropertyVm + " \r");
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
                forwardSolver = InverseForwardSolverTypeOptionVm.SelectedValue;
                break;
            case PlotDataType.Guess:
                solverString = "\n" + StringLookup.GetLocalizedString("Label_Guess");
                opticalProperties = InitialGuessOpticalPropertyVm.GetOpticalProperties();
                forwardSolver = InverseForwardSolverTypeOptionVm.SelectedValue;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(datatype));
        }
        var opString = "\r" + StringLookup.GetLocalizedString("Label_MuA") + " = " + 
                       Math.Round(opticalProperties.Mua, 4) + " " +
                       StringLookup.GetLocalizedString("Measurement_Inv_mm") + "\r" +
                       StringLookup.GetLocalizedString("Label_MuSPrime") + " = " +
                       Math.Round(opticalProperties.Musp, 4) + " " +
                       StringLookup.GetLocalizedString("Measurement_Inv_mm");

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

        var isWavelengthPlot = AllRangeVMs.Any(vm => vm.AxisType == IndependentVariableAxis.Wavelength);
        if (AllRangeVMs.Length <= 1) return [solverString + modelString + (isWavelengthPlot ? "\r" + StringLookup.GetLocalizedString("Label_SpectralMuAMuSPrime") : opString)];
        var secondaryRangeVm = isWavelengthPlot
            ? AllRangeVMs.First(vm => vm.AxisType != IndependentVariableAxis.Wavelength)
            : AllRangeVMs.First(
                vm => vm.AxisType != IndependentVariableAxis.Time && 
                      vm.AxisType != IndependentVariableAxis.Ft);

        var secondaryAxesStrings =
            secondaryRangeVm.Values.Select(
                    value =>
                        "\r" + secondaryRangeVm.AxisType.GetInternationalizedString() + " = " + value)
                .ToArray();
        return
            [.. secondaryAxesStrings.Select(
                    sas => solverString + modelString + sas + 
                           (isWavelengthPlot ? "\r" + StringLookup.GetLocalizedString("Label_SpectralMuAMuSPrime") : opString))];
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
        WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
            StringLookup.GetLocalizedString("Label_InitialGuess") +
            InitialGuessOpticalPropertyVm + " \r");
    }

    private void SolveInverseCommand_Executed()
    {
        // Report inverse solver setup and results
        WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
            StringLookup.GetLocalizedString("Label_InverseSolutionResults") + "\r");
        WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
            "   " + StringLookup.GetLocalizedString("Label_OptimizationParameter") +
            InverseFitTypeOptionVm.SelectedValue + " \r");
        WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
            "   " + StringLookup.GetLocalizedString("Label_InitialGuess") +
            InitialGuessOpticalPropertyVm + " \r");

        var inverseResult = SolveInverse();
        ResultOpticalPropertyVm.SetOpticalProperties(inverseResult.FitOpticalProperties[0]);
            // todo: this only works for one set of properties

        //Report the results
        if (
            SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.SelectedValues.Contains(
                IndependentVariableAxis.Wavelength) &&
            inverseResult.FitOpticalProperties.Length > 1)
            // If multivalued OPs, the results aren't in the "scalar" VMs, need to parse OPs directly
        {
            var fitOPs = inverseResult.FitOpticalProperties;
            var measuredOPs = inverseResult.MeasuredOpticalProperties;
            var wavelengths = GetParameterValues(IndependentVariableAxis.Wavelength);
            var wvUnitString = IndependentVariableAxisUnits.NM.GetInternationalizedString();
            var opUnitString = StringLookup.GetLocalizedString("Measurement_Inv_mm");
            var sb = new StringBuilder("\t[" + StringLookup.GetLocalizedString("Label_Wavelength") + 
                                       " (" + wvUnitString + ")]\t\t\t\t\t\t[" + 
                                       StringLookup.GetLocalizedString("Label_Exact") + "]\t\t\t\t\t\t[" + 
                                       StringLookup.GetLocalizedString("Label_ConvergedValues") + "]\t\t\t\t\t\t[" + 
                                       StringLookup.GetLocalizedString("Label_Units") + "]\r");
            for (var i = 0; i < fitOPs.Length; i++)
            {
                sb.Append("\t" + wavelengths[i] + "\t\t\t\t\t\t" + measuredOPs[i] + "\t\t\t" + fitOPs[i] + "\t\t\t" +
                          opUnitString + " \r");
            }
            WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(sb.ToString());
        }
        else
        {
            WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
                "   " + StringLookup.GetLocalizedString("Label_Exact") + 
                ": " + MeasuredOpticalPropertyVm + " \r");
            WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
                "   " + StringLookup.GetLocalizedString("Label_ConvergedValues") + 
                ": " + ResultOpticalPropertyVm + " \r");
            //Display Percent Error
            var muaError = 0.0;
            var muspError = 0.0;
            if (MeasuredOpticalPropertyVm.Mua > 0)
            {
                var tempMuaError = (int)(10000.0 * Math.Abs(ResultOpticalPropertyVm.Mua - MeasuredOpticalPropertyVm.Mua) / MeasuredOpticalPropertyVm.Mua);
                muaError = tempMuaError / 100.0;
            }
            if (MeasuredOpticalPropertyVm.Musp > 0)
            {
                var tempMuspError = (int)(10000.0 * Math.Abs(ResultOpticalPropertyVm.Musp - MeasuredOpticalPropertyVm.Musp) / MeasuredOpticalPropertyVm.Musp);
                muspError = tempMuspError / 100.0;
            }
            WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
                "   " + StringLookup.GetLocalizedString("Label_PercentError") + 
                StringLookup.GetLocalizedString("Label_MuA") + " = " + muaError +
                "%  " + StringLookup.GetLocalizedString("Label_MuSPrime") + 
                " = " + muspError + "% \r");
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
            [.. parameters.Values]);

        return measuredData.AddNoise(PercentNoise);
    }

    private OpticalProperties[] GetMeasuredOpticalProperties()
    {
        return GetOpticalPropertiesFromSpectralPanel() ?? [MeasuredOpticalPropertyVm.GetOpticalProperties()];
    }

    private OpticalProperties[] GetInitialGuessOpticalProperties()
    {
        return GetDuplicatedListOfOpticalProperties() ??
               [InitialGuessOpticalPropertyVm.GetOpticalProperties()];
    }

    private OpticalProperties[] GetDuplicatedListOfOpticalProperties()
    {
        var initialGuessOPs = InitialGuessOpticalPropertyVm.GetOpticalProperties();
        if (!SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.SelectedValues.Contains(
                IndependentVariableAxis.Wavelength)) return null;
        var wavelengths = GetParameterValues(IndependentVariableAxis.Wavelength);
        return [.. wavelengths.Select(_ => initialGuessOPs.Clone())];
    }

    private OpticalProperties[] GetOpticalPropertiesFromSpectralPanel()
    {
        if (!SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.SelectedValues.Contains(
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
            [.. parameters.Values]);
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
        //    InverseForwardSolverTypeOptionVm.SelectedValue,
        //    OptimizerTypeOptionVm.SelectedValue,
        //    SolutionDomainTypeOptionVm.SelectedValue,
        //    dependentValues,
        //    dependentValues, // set standard deviation, sd, to measured (works w/ or w/o noise)
        //    InverseFitTypeOptionVm.SelectedValue,
        //    initGuessParameters.Values.ToArray())

        var fit = ComputationFactory.SolveInverse(
            InverseForwardSolverTypeOptionVm.SelectedValue,
            OptimizerTypeOptionVm.SelectedValue,
            SolutionDomainTypeOptionVm.SelectedValue,
            dependentValues,
            dependentValues, // set standard deviation, sd, to measured (works w/ or w/o noise)
            InverseFitTypeOptionVm.SelectedValue,
            [.. initGuessParameters.Values],
            lowerBounds, upperBounds);

        var fitOpticalProperties = ComputationFactory.UnFlattenOpticalProperties(fit);

        var fitParameters = GetParametersInOrder(fitOpticalProperties);

        var resultDataValues = ComputationFactory.ComputeReflectance(
            InverseForwardSolverTypeOptionVm.SelectedValue,
            SolutionDomainTypeOptionVm.SelectedValue,
            ForwardAnalysisType.R,
            [.. fitParameters.Values]);

        var resultDataPoints = GetDataPoints(resultDataValues);

        return new InverseSolutionResult
        {
            FitDataPoints = resultDataPoints,
            MeasuredOpticalProperties = measuredOpticalProperties,
            // todo: currently only supports homogeneous OPs
            GuessOpticalProperties = initGuessOpticalProperties,
            // todo: currently only supports homogeneous OPss
            FitOpticalProperties = fitOpticalProperties
        };
    }

    private IDataPoint[][] GetDataPoints(double[] reflectance)
    {
        var plotIsVsWavelength = AllRangeVMs.Any(vm => vm.AxisType == IndependentVariableAxis.Wavelength);
        var isComplexPlot = ComputationFactory.IsComplexSolver(SolutionDomainTypeOptionVm.SelectedValue);
        var primaryIndependentValues = AllRangeVMs[0].Values.ToArray();
        var numPointsPerCurve = primaryIndependentValues.Length;
        var numForwardValues = isComplexPlot ? reflectance.Length/2 : reflectance.Length;
        // complex reported as all real numbers, then all imaginary numbers
        var numCurves = numForwardValues/numPointsPerCurve;

        var points = new IDataPoint[numCurves][];

        for (var j = 0; j < numCurves; j++)
        {
            points[j] = new IDataPoint[numPointsPerCurve];
            for (var i = 0; i < numPointsPerCurve; i++)
            {
                points[j][i] = GetReflectanceAtIndex(i, j);
            }
        }
        return points;

        IDataPoint GetReflectanceAtIndex(int i, int j)
        {
            // man, this is getting hacky...
            var index = plotIsVsWavelength
                ? i * numCurves + j
                : j * numPointsPerCurve + i;
            return isComplexPlot
                ? new ComplexDataPoint(primaryIndependentValues[i], new Complex(reflectance[index], reflectance[index + numForwardValues]))
                : new DoubleDataPoint(primaryIndependentValues[i], reflectance[index]);
        }
    }

    private IDictionary<IndependentVariableAxis, object> GetParametersInOrder(object opticalProperties)
    {
        // get all parameters to get arrays of
        // then, for each one, decide if it's an IV or a constant
        // then, call the appropriate parameter generator, defined above
        var allParameters =
            from iv in
                SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.SelectedValues.Concat(
                    SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.UnSelectedValues)
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


    private double[] GetParameterValues(IndependentVariableAxis axis)
    {
        var isConstant = SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.UnSelectedValues.Contains(axis);
        if (isConstant)
        {
            //var positionIndex = SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.UnSelectedValues.IndexOf(axis)
            var positionIndex = 0; //hard-coded for now
            return positionIndex switch
            {
                // add this back if hard-coding is removed
                // 1 => [SolutionDomainTypeOptionVm.ConstantAxesVMs[1].AxisValue],
                _ => [SolutionDomainTypeOptionVm.ConstantAxesVMs[0].AxisValue]
            };
        }
        else
        {
            var numAxes = SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.SelectedValues.Length;
            var positionIndex = 0; //hard-coded for now - commented the options that are currently unreachable
            //var positionIndex = SolutionDomainTypeOptionVm.IndependentVariableAxisOptionVm.SelectedValues.IndexOf(axis)
            return numAxes switch
            {
                2 => positionIndex switch
                {
                    // add this back if hard-coding is removed
                    //1 => AllRangeVMs[0].Values.ToArray(),
                    _ => [.. AllRangeVMs[1].Values]
                },
                3 => positionIndex switch
                {
                    // add these back if hard-coding is removed
                    //1 => AllRangeVMs[1].Values.ToArray(),
                    //2 => AllRangeVMs[0].Values.ToArray(),
                    _ => [.. AllRangeVMs[2].Values]
                },
                _ => [.. AllRangeVMs[0].Values]
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