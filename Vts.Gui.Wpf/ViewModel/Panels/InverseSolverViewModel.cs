using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Input;
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

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary>
    ///     View model implementing Inverse Solver panel functionality
    /// </summary>
    public class InverseSolverViewModel : BindableObject
    {
        private RangeViewModel[] _allRangeVMs;
        private OpticalPropertyViewModel _InitialGuessOpticalPropertyVM;
        private OptionViewModel<InverseFitType> _InverseFitTypeVM;
        private OptionViewModel<ForwardSolverType> _InverseForwardSolverTypeVM;
        private OptionViewModel<ForwardSolverType> _MeasuredForwardSolverTypeVM;

        private OpticalPropertyViewModel _MeasuredOpticalPropertyVM;
        private OptionViewModel<OptimizerType> _OptimizerTypeVM;

        private double _PercentNoise;
        private OpticalPropertyViewModel _ResultOpticalPropertyVM;

        private bool _showOpticalProperties;
        private SolutionDomainOptionViewModel _SolutionDomainTypeOptionVM;
        private bool _useSpectralPanelData;

        public InverseSolverViewModel()
        {
            _showOpticalProperties = true;
            _useSpectralPanelData = false;

            _allRangeVMs = new[] {new RangeViewModel {Title = Strings.IndependentVariableAxis_Rho}};

            SolutionDomainTypeOptionVM = new SolutionDomainOptionViewModel("Solution Domain", SolutionDomainType.ROfRho);
            SolutionDomainTypeOptionVM.EnableMultiAxis = false;
            SolutionDomainTypeOptionVM.AllowMultiAxis = false;

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
                    var useSpectralPanelDataAndNotNull = UseSpectralPanelData && WindowViewModel.Current != null &&
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
                        !_allRangeVMs.Any(value => value.AxisType == IndependentVariableAxis.Wavelength);

                    // update solution domain wavelength constant if applicable
                    if (useSpectralPanelDataAndNotNull &&
                        SolutionDomainTypeOptionVM.ConstantAxesVMs.Any(
                            axis => axis.AxisType == IndependentVariableAxis.Wavelength))
                    {
                        updateSolutionDomainWithWavelength(WindowViewModel.Current.SpectralMappingVM.Wavelength);
                    }
                }
            };

#if WHITELIST
            MeasuredForwardSolverTypeOptionVM = new OptionViewModel<ForwardSolverType>(
                "Forward Model Engine",false, WhiteList.InverseForwardSolverTypes);
#else
            MeasuredForwardSolverTypeOptionVM = new OptionViewModel<ForwardSolverType>(
                "Forward Model Engine", false); //These titles are not diplayed so we can hard-code strings
#endif

#if WHITELIST 
            InverseForwardSolverTypeOptionVM = new OptionViewModel<ForwardSolverType>("Inverse Model Engine",false, WhiteList.InverseForwardSolverTypes);
#else
            InverseForwardSolverTypeOptionVM = new OptionViewModel<ForwardSolverType>("Inverse Model Engine", false); //These titles are not diplayed so we can hard-code strings
#endif
            InverseForwardSolverTypeOptionVM.PropertyChanged += (sender, args) =>
                OnPropertyChanged("InverseForwardSolver");

            OptimizerTypeOptionVM = new OptionViewModel<OptimizerType>(StringLookup.GetLocalizedString("Heading_OptimizerType"), true);
            OptimizerTypeOptionVM.PropertyChanged += (sender, args) =>
                OnPropertyChanged("Optimizer");

            InverseFitTypeOptionVM = new OptionViewModel<InverseFitType>(StringLookup.GetLocalizedString("Heading_OptimizationParameters"), true);

            MeasuredOpticalPropertyVM = new OpticalPropertyViewModel {Title = ""};
            InitialGuessOpticalPropertyVM = new OpticalPropertyViewModel {Title = ""};
            ResultOpticalPropertyVM = new OpticalPropertyViewModel {Title = ""};

            SimulateMeasuredDataCommand = new RelayCommand(() => SimulateMeasuredDataCommand_Executed(null, null));
            CalculateInitialGuessCommand = new RelayCommand(() => CalculateInitialGuessCommand_Executed(null, null));
            SolveInverseCommand = new RelayCommand(() => SolveInverseCommand_Executed(null, null));

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
                            WindowViewModel.Current.SpectralMappingVM != null &&
                            MeasuredOpticalPropertyVM != null)
                            {
                                MeasuredOpticalPropertyVM.SetOpticalProperties(
                                    WindowViewModel.Current.SpectralMappingVM.OpticalProperties);
                            }
                        
                    }
                };
            }
        }

        public RelayCommand SimulateMeasuredDataCommand { get; set; }
        public RelayCommand CalculateInitialGuessCommand { get; set; }
        public RelayCommand SolveInverseCommand { get; set; }

        public SolutionDomainOptionViewModel SolutionDomainTypeOptionVM
        {
            get { return _SolutionDomainTypeOptionVM; }
            set
            {
                _SolutionDomainTypeOptionVM = value;
                OnPropertyChanged("SolutionDomainTypeOptionVM");
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

        public OptionViewModel<ForwardSolverType> MeasuredForwardSolverTypeOptionVM
        {
            get { return _MeasuredForwardSolverTypeVM; }
            set
            {
                _MeasuredForwardSolverTypeVM = value;
                OnPropertyChanged("MeasuredForwardSolverTypeOptionVM");
            }
        }

        public OptionViewModel<ForwardSolverType> InverseForwardSolverTypeOptionVM
        {
            get { return _InverseForwardSolverTypeVM; }
            set
            {
                _InverseForwardSolverTypeVM = value;
                OnPropertyChanged("InverseForwardSolverTypeOptionVM");
            }
        }

        public OptionViewModel<OptimizerType> OptimizerTypeOptionVM
        {
            get { return _OptimizerTypeVM; }
            set
            {
                _OptimizerTypeVM = value;
                OnPropertyChanged("OptimizerTypeOptionVM");
            }
        }

        public OptionViewModel<InverseFitType> InverseFitTypeOptionVM
        {
            get { return _InverseFitTypeVM; }
            set
            {
                _InverseFitTypeVM = value;
                OnPropertyChanged("InverseFitTypeOptionVM");
            }
        }

        public OpticalPropertyViewModel MeasuredOpticalPropertyVM
        {
            get { return _MeasuredOpticalPropertyVM; }
            set
            {
                _MeasuredOpticalPropertyVM = value;
                OnPropertyChanged("MeasuredOpticalPropertyVM");
            }
        }

        public bool UseSpectralPanelData // for measured data
        {
            get { return _useSpectralPanelData; }
            set
            {
                _useSpectralPanelData = value;
                OnPropertyChanged("UseSpectralPanelData");
            }
        }

        public bool ShowOpticalProperties // for measured data
        {
            get { return _showOpticalProperties; }
            set
            {
                _showOpticalProperties = value;
                OnPropertyChanged("ShowOpticalProperties");
            }
        }

        public OpticalPropertyViewModel InitialGuessOpticalPropertyVM
        {
            get { return _InitialGuessOpticalPropertyVM; }
            set
            {
                _InitialGuessOpticalPropertyVM = value;
                OnPropertyChanged("InitialGuessOpticalPropertyVM");
            }
        }

        public OpticalPropertyViewModel ResultOpticalPropertyVM
        {
            get { return _ResultOpticalPropertyVM; }
            set
            {
                _ResultOpticalPropertyVM = value;
                OnPropertyChanged("ResultOpticalPropertyVM");
            }
        }

        public double PercentNoise
        {
            get { return _PercentNoise; }
            set
            {
                _PercentNoise = value;
                OnPropertyChanged("PercentNoise");
            }
        }

        public IForwardSolver MeasuredForwardSolver
        {
            get { return SolverFactory.GetForwardSolver(MeasuredForwardSolverTypeOptionVM.SelectedValue); }
        }

        public IForwardSolver InverseForwardSolver
        {
            get { return SolverFactory.GetForwardSolver(InverseForwardSolverTypeOptionVM.SelectedValue); }
        }

        public IOptimizer Optimizer
        {
            get { return SolverFactory.GetOptimizer(OptimizerTypeOptionVM.SelectedValue); }
        }

        private void SimulateMeasuredDataCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var measuredDataValues = GetSimulatedMeasuredData();
            var measuredDataPoints = GetDataPoints(measuredDataValues);

            //Report the results
            var axesLabels = GetPlotLabels();
            WindowViewModel.Current.PlotVM.SetAxesLabels.Execute(axesLabels);
            var plotLabels = GetLegendLabels(PlotDataType.Simulated);
            var plotData = measuredDataPoints.Zip(plotLabels, (p, el) => new PlotData(p, el)).ToArray();
            WindowViewModel.Current.PlotVM.PlotValues.Execute(plotData);
            WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(StringLookup.GetLocalizedString("Label_SimulatedMeasuredData") + MeasuredOpticalPropertyVM + " \r");
        }

        private string[] GetLegendLabels(PlotDataType datatype)
        {
            string solverString = null;
            OpticalProperties opticalProperties = null;
            string modelString = null;
            ForwardSolverType forwardSolver;

            switch (datatype)
            {
                case PlotDataType.Simulated:
                    solverString = "\n" + StringLookup.GetLocalizedString("Label_Simulated");
                    opticalProperties = MeasuredOpticalPropertyVM.GetOpticalProperties();
                    forwardSolver = MeasuredForwardSolverTypeOptionVM.SelectedValue;
                    break;
                case PlotDataType.Calculated:
                    solverString = "\n" + StringLookup.GetLocalizedString("Label_Calculated");
                    opticalProperties = ResultOpticalPropertyVM.GetOpticalProperties();
                    forwardSolver = MeasuredForwardSolverTypeOptionVM.SelectedValue;
                    break;
                case PlotDataType.Guess:
                    solverString = "\n" + StringLookup.GetLocalizedString("Label_Guess");
                    opticalProperties = InitialGuessOpticalPropertyVM.GetOpticalProperties();
                    forwardSolver = InverseForwardSolverTypeOptionVM.SelectedValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("datatype");
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

            if (_allRangeVMs.Length > 1)
            {
                var isWavelengthPlot = _allRangeVMs.Any(vm => vm.AxisType == IndependentVariableAxis.Wavelength);
                var secondaryRangeVM = isWavelengthPlot
                    ? _allRangeVMs.First(vm => vm.AxisType != IndependentVariableAxis.Wavelength)
                    : _allRangeVMs.First(
                        vm => vm.AxisType != IndependentVariableAxis.Time && 
                              vm.AxisType != IndependentVariableAxis.Ft);

                var secondaryAxesStrings =
                    secondaryRangeVM.Values.Select(
                        value =>
                            "\r" + secondaryRangeVM.AxisType.GetInternationalizedString() + " = " + value.ToString())
                        .ToArray();
                return
                    secondaryAxesStrings.Select(
                        sas => solverString + modelString + sas + (isWavelengthPlot ? "\r" + StringLookup.GetLocalizedString("Label_SpectralMuAMuSPrime") : opString))
                        .ToArray();
            }

            return new[] {solverString + modelString + opString};
        }

        private PlotAxesLabels GetPlotLabels()
        {
            var sd = SolutionDomainTypeOptionVM;
            var axesLabels = new PlotAxesLabels(
                sd.SelectedDisplayName, sd.SelectedValue.GetUnits(),
                sd.IndependentAxesVMs.First(),
                sd.ConstantAxesVMs);
            return axesLabels;
        }

        private void CalculateInitialGuessCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var initialGuessDataValues = CalculateInitialGuess();
            var initialGuessDataPoints = GetDataPoints(initialGuessDataValues);

            //Report the results
            var axesLabels = GetPlotLabels();
            WindowViewModel.Current.PlotVM.SetAxesLabels.Execute(axesLabels);
            var plotLabels = GetLegendLabels(PlotDataType.Guess);
            var plotData = initialGuessDataPoints.Zip(plotLabels, (p, el) => new PlotData(p, el)).ToArray();
            WindowViewModel.Current.PlotVM.PlotValues.Execute(plotData);
            WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(StringLookup.GetLocalizedString("Label_InitialGuess") +
                                                                                InitialGuessOpticalPropertyVM + " \r");
        }

        private void SolveInverseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Report inverse solver setup and results
            WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(StringLookup.GetLocalizedString("Label_InverseSolutionResults") + "\r");
            WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute("   " + StringLookup.GetLocalizedString("Label_OptimizationParameter") +
                                                                                InverseFitTypeOptionVM.SelectedValue +
                                                                                " \r");
            WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute("   " + StringLookup.GetLocalizedString("Label_InitialGuess") +
                                                                                InitialGuessOpticalPropertyVM + " \r");

            var inverseResult = SolveInverse();
            ResultOpticalPropertyVM.SetOpticalProperties(inverseResult.FitOpticalProperties.First());
                // todo: this only works for one set of properties

            //Report the results
            if (
                SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.Contains(
                    IndependentVariableAxis.Wavelength) &&
                inverseResult.FitOpticalProperties.Length > 1)
                // If multi-valued OPs, the results aren't in the "scalar" VMs, need to parse OPs directly
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
                WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(sb.ToString());
            }
            else
            {
                WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute("   " + StringLookup.GetLocalizedString("Label_Exact") + ": " +
                                                                                    MeasuredOpticalPropertyVM + " \r");
                WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute("   " + StringLookup.GetLocalizedString("Label_ConvergedValues") + ": " +
                                                                                    ResultOpticalPropertyVM + " \r");
                                //Display Percent Error
                double muaError = 0.0;
                double muspError = 0.0;
                if (MeasuredOpticalPropertyVM.Mua > 0)
                {
                    int tempMuaError = (int)(10000.0 * Math.Abs(ResultOpticalPropertyVM.Mua - MeasuredOpticalPropertyVM.Mua) / MeasuredOpticalPropertyVM.Mua);
                    muaError = tempMuaError / 100.0;
                }
                if (MeasuredOpticalPropertyVM.Musp > 0)
                {
                    int tempMuspError = (int)(10000.0 * Math.Abs(ResultOpticalPropertyVM.Musp - MeasuredOpticalPropertyVM.Musp) / MeasuredOpticalPropertyVM.Musp);
                    muspError = tempMuspError / 100.0;
                }
                WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute("   " + StringLookup.GetLocalizedString("Label_PercentError") + 
                                                                                    StringLookup.GetLocalizedString("Label_MuA") + " = " + muaError +
                                                                                    "%  " + StringLookup.GetLocalizedString("Label_MuSPrime") + " = " + muspError + "% \r");
            }

            var axesLabels = GetPlotLabels();
            WindowViewModel.Current.PlotVM.SetAxesLabels.Execute(axesLabels);
            var plotLabels = GetLegendLabels(PlotDataType.Calculated);
            var plotData = inverseResult.FitDataPoints.Zip(plotLabels, (p, el) => new PlotData(p, el)).ToArray();
            WindowViewModel.Current.PlotVM.PlotValues.Execute(plotData);
        }

        private double[] GetSimulatedMeasuredData()
        {
            var opticalProperties = GetMeasuredOpticalProperties();

            var parameters = GetParametersInOrder(opticalProperties);

            var measuredData = ComputationFactory.ComputeReflectance(
                MeasuredForwardSolverTypeOptionVM.SelectedValue,
                SolutionDomainTypeOptionVM.SelectedValue,
                ForwardAnalysisType.R,
                parameters.Values.ToArray());

            return measuredData.AddNoise(PercentNoise);
        }

        private object GetMeasuredOpticalProperties()
        {
            return GetOpticalPropertiesFromSpectralPanel() ?? new[] {MeasuredOpticalPropertyVM.GetOpticalProperties()};
        }

        private object GetInitialGuessOpticalProperties()
        {
            return GetDuplicatedListofOpticalProperties() ??
                   new[] {InitialGuessOpticalPropertyVM.GetOpticalProperties()};
        }

        private OpticalProperties[] GetDuplicatedListofOpticalProperties()
        {
            var initialGuessOPs = InitialGuessOpticalPropertyVM.GetOpticalProperties();
            if (
                SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.Contains(
                    IndependentVariableAxis.Wavelength))
            {
                var wavelengths = GetParameterValues(IndependentVariableAxis.Wavelength);
                return wavelengths.Select(_ => initialGuessOPs.Clone()).ToArray();
            }

            return null;
        }

        private OpticalProperties[] GetOpticalPropertiesFromSpectralPanel()
        {
            if (
                SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.Contains(
                    IndependentVariableAxis.Wavelength) &&
                UseSpectralPanelData &&
                WindowViewModel.Current != null &&
                WindowViewModel.Current.SpectralMappingVM != null)
            {
                var tissue = WindowViewModel.Current.SpectralMappingVM.SelectedTissue;
                var wavelengths = GetParameterValues(IndependentVariableAxis.Wavelength);
                var ops = tissue.GetOpticalProperties(wavelengths);

                return ops;
            }

            return null;
        }

        public double[] CalculateInitialGuess()
        {
            var opticalProperties = GetInitialGuessOpticalProperties();

            var parameters = GetParametersInOrder(opticalProperties);

            return ComputationFactory.ComputeReflectance(
                InverseForwardSolverTypeOptionVM.SelectedValue,
                SolutionDomainTypeOptionVM.SelectedValue,
                ForwardAnalysisType.R,
                parameters.Values.ToArray());
        }

        public InverseSolutionResult SolveInverse()
        {
            var lowerBounds = new double[] { 0, 0, 0, 0 };
            var upperBounds = new double[] { double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity };

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
                InverseForwardSolverTypeOptionVM.SelectedValue,
                OptimizerTypeOptionVM.SelectedValue,
                SolutionDomainTypeOptionVM.SelectedValue,
                dependentValues,
                dependentValues, // set standard deviation, sd, to measured (works w/ or w/o noise)
                InverseFitTypeOptionVM.SelectedValue,
                initGuessParameters.Values.ToArray(),
                lowerBounds, upperBounds);

            var fitOpticalProperties = ComputationFactory.UnFlattenOpticalProperties(fit);

            var fitParameters = GetParametersInOrder(fitOpticalProperties);

            var resultDataValues = ComputationFactory.ComputeReflectance(
                InverseForwardSolverTypeOptionVM.SelectedValue,
                SolutionDomainTypeOptionVM.SelectedValue,
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
            var isComplexPlot = ComputationFactory.IsComplexSolver(SolutionDomainTypeOptionVM.SelectedValue);
            var primaryIdependentValues = _allRangeVMs.First().Values.ToArray();
            var numPointsPerCurve = primaryIdependentValues.Length;
            var numForwardValues = isComplexPlot ? reflectance.Length/2 : reflectance.Length;
            // complex reported as all reals, then all imaginaries
            var numCurves = numForwardValues/numPointsPerCurve;

            var points = new IDataPoint[numCurves][];
            Func<int, int, IDataPoint> getReflectanceAtIndex = (i, j) =>
            {
                // man, this is getting hacky...
                var index = plotIsVsWavelength
                    ? i*numCurves + j
                    : j*numPointsPerCurve + i;
                return isComplexPlot
                    ? (IDataPoint)
                        new ComplexDataPoint(primaryIdependentValues[i],
                            new Complex(reflectance[index], reflectance[index + numForwardValues]))
                    : (IDataPoint) new DoubleDataPoint(primaryIdependentValues[i], reflectance[index]);
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
                //var positionIndex = SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.UnSelectedValues.IndexOf(axis);
                var positionIndex = 0; //hard-coded for now
                switch (positionIndex)
                {
                    case 0:
                    default:
                        return new[] {SolutionDomainTypeOptionVM.ConstantAxesVMs[0].AxisValue};
                    case 1:
                        return new[] {SolutionDomainTypeOptionVM.ConstantAxesVMs[1].AxisValue};
                    //case 2:
                    //    return new[] { SolutionDomainTypeOptionVM.ConstantAxisThreeValue };
                }
            }
            else
            {
                var numAxes = SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.Length;
                var positionIndex = 0; //hard-coded for now
                //var positionIndex = SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.IndexOf(axis);
                switch (numAxes)
                {
                    case 1:
                    default:
                        return AllRangeVMs[0].Values.ToArray();
                    case 2:
                        switch (positionIndex)
                        {
                            case 0:
                            default:
                                return AllRangeVMs[1].Values.ToArray();
                            case 1:
                                return AllRangeVMs[0].Values.ToArray();
                        }
                    case 3:
                        switch (positionIndex)
                        {
                            case 0:
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
}