using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Vts.Common;
using Vts.Factories;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.IO;
using Vts.Modeling.ForwardSolvers;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.ViewModel;

/// <summary>
///     View model implementing Fluence Solver panel functionality
/// </summary>
public class FluenceSolverViewModel : BindableObject
{
    // the following function determines a flattened mua array for layered tissue
    private static readonly Func<ILayerOpticalPropertyRegion[], double[], double[], double[]>
        GetRhoZMuaArrayFromLayerRegions = (regions, rhos, zs) =>
        {
            var numBins = rhos.Length*zs.Length;
            var muaArray = new double[numBins];
            for (var i = 0; i < zs.Length; i++)
            {
                var layerIndex = DetectorBinning.WhichBin(zs[i], [.. regions.Select(r => r.ZRange.Stop)]);
                for (var j = 0; j < rhos.Length; j++)
                {
                    muaArray[i*rhos.Length + j] = regions[layerIndex].RegionOP.Mua;
                }
            }
            return muaArray;
        };

    // private fields to cache created instances of tissue inputs, created on-demand in GetTissueInputVM (vs up-front in constructor)
    private OpticalProperties _currentHomogeneousOpticalProperties;
    private MultiLayerTissueInput _currentMultiLayerTissueInput;
    private SemiInfiniteTissueInput _currentSemiInfiniteTissueInput;
    private SingleEllipsoidTissueInput _currentSingleEllipsoidTissueInput;

    private object _tissueInputVm;
        // either an OpticalPropertyViewModel or a MultiRegionTissueViewModel is stored here, and dynamically displayed

    private CancellationTokenSource _currentCancellationTokenSource;
    private bool _canRunSolver;
    private bool _canCancelSolver;

    public FluenceSolverViewModel()
    {
        RhoRangeVm = new RangeViewModel(new DoubleRange(0.1, 19.9, 100), StringLookup.GetLocalizedString("Measurement_mm"), IndependentVariableAxis.Rho, "");
        ZRangeVm = new RangeViewModel(new DoubleRange(0.1, 19.9, 100), StringLookup.GetLocalizedString("Measurement_mm"), IndependentVariableAxis.Z, "");
        SourceDetectorSeparation = 10.0;
        TimeModulationFrequency = 0.1;
        _tissueInputVm = new OpticalPropertyViewModel(new OpticalProperties(),
            IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(),
            StringLookup.GetLocalizedString("Heading_OpticalProperties"));

        // right now, we're doing manual data binding to the selected item. need to enable data binding 
        // confused, though - do we need to use strings? or, how to make generics work with dependency properties?
        ForwardSolverTypeOptionVm = new OptionViewModel<ForwardSolverType>(
            "Forward Model",
            false,
            [
                ForwardSolverType.DistributedPointSourceSDA,
                ForwardSolverType.PointSourceSDA,
                ForwardSolverType.DistributedGaussianSourceSDA,
                ForwardSolverType.TwoLayerSDA
            ]); // explicitly enabling these for the workshop

        FluenceSolutionDomainTypeOptionVm = new FluenceSolutionDomainOptionViewModel(StringLookup.GetLocalizedString("Heading_FluenceSolutionDomain"))
        {
            IsFluenceOfRhoAndZAndTimeEnabled = false,
            IsFluenceOfRhoAndZAndFtEnabled = true
        };
        AbsorbedEnergySolutionDomainTypeOptionVm =
            new FluenceSolutionDomainOptionViewModel(StringLookup.GetLocalizedString("Heading_AbsorbedEnergySolutionDomain"))
            {
                IsFluenceOfRhoAndZAndTimeEnabled = false,
                IsFluenceOfRhoAndZAndFtEnabled = true
            };
        PhotonHittingDensitySolutionDomainTypeOptionVm =
            new FluenceSolutionDomainOptionViewModel(StringLookup.GetLocalizedString("Heading_PHDSolutionDomain"))
            {
                IsFluenceOfRhoAndZAndTimeEnabled = false,
                IsFluenceOfRhoAndZAndFtEnabled = true
            };
        PropertyChangedEventHandler updateSolutionDomain = (sender, args) =>
        {
            if (args.PropertyName == "IndependentAxisType")
            {
                RhoRangeVm = ((FluenceSolutionDomainOptionViewModel) sender)?.IndependentAxesVMs[0].AxisRangeVM;
            }
            // todo: must this fire on ANY property, or is there a specific one we can listen to, as above?
            OnPropertyChanged(nameof(IsTimeFrequencyDomain));
        };
        FluenceSolutionDomainTypeOptionVm.PropertyChanged += updateSolutionDomain;
        AbsorbedEnergySolutionDomainTypeOptionVm.PropertyChanged += updateSolutionDomain;
        PhotonHittingDensitySolutionDomainTypeOptionVm.PropertyChanged += updateSolutionDomain;

        MapTypeOptionVm = new OptionViewModel<MapType>(
            "Map Type",
            [
                MapType.Fluence,
                MapType.AbsorbedEnergy,
                MapType.PhotonHittingDensity
            ]);

        MapTypeOptionVm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != "SelectedValues") return;
            OnPropertyChanged(nameof(IsFluence));
            OnPropertyChanged(nameof(IsAbsorbedEnergy));
            OnPropertyChanged(nameof(IsPhotonHittingDensity));
            OnPropertyChanged(nameof(IsTimeFrequencyDomain));
            UpdateAvailableOptions();
        };

        ForwardSolverTypeOptionVm.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ForwardSolver));
            OnPropertyChanged(nameof(IsGaussianForwardModel));
            OnPropertyChanged(nameof(IsMultiRegion));
            OnPropertyChanged(nameof(IsSemiInfinite));
            TissueInputVm = GetTissueInputVm(IsMultiRegion ? "MultiLayer" : "SemiInfinite");
            UpdateAvailableOptions();
            OnPropertyChanged(nameof(IsTimeFrequencyDomain));
        };

        ExecuteFluenceSolverCommand = new RelayCommand(() => _ = ExecuteFluenceSolver_Executed());
        CancelFluenceSolverCommand = new RelayCommand(CancelFluenceSolver_Executed);
        _canRunSolver = true;
        _canCancelSolver = false;
    }

    public RelayCommand ExecuteFluenceSolverCommand { get; set; }
    public RelayCommand CancelFluenceSolverCommand { get; set; }

    public bool CanRunSolver
    {
        get => _canRunSolver;
        set
        {
            _canRunSolver = value;
            OnPropertyChanged(nameof(CanRunSolver));
        }
    }

    public bool CanCancelSolver
    {
        get => _canCancelSolver;
        set
        {
            _canCancelSolver = value;
            OnPropertyChanged(nameof(CanCancelSolver));
        }
    }
    public IForwardSolver ForwardSolver => SolverFactory.GetForwardSolver(
                ForwardSolverTypeOptionVm.SelectedValue);

    public bool IsGaussianForwardModel => ForwardSolverTypeOptionVm.SelectedValue.IsGaussianForwardModel();

    public bool IsMultiRegion => ForwardSolverTypeOptionVm.SelectedValue.IsMultiRegionForwardModel();

    public bool IsSemiInfinite => !ForwardSolverTypeOptionVm.SelectedValue.IsMultiRegionForwardModel();

    public bool IsFluence => MapTypeOptionVm.SelectedValue == MapType.Fluence;

    public bool IsAbsorbedEnergy => MapTypeOptionVm.SelectedValue == MapType.AbsorbedEnergy;

    public bool IsPhotonHittingDensity => MapTypeOptionVm.SelectedValue == MapType.PhotonHittingDensity;

    public bool IsTimeFrequencyDomain => (MapTypeOptionVm.SelectedValue == MapType.Fluence &&
                 FluenceSolutionDomainTypeOptionVm.SelectedValue == FluenceSolutionDomainType.FluenceOfRhoAndZAndFt) ||
                (MapTypeOptionVm.SelectedValue == MapType.AbsorbedEnergy &&
                 AbsorbedEnergySolutionDomainTypeOptionVm.SelectedValue ==
                 FluenceSolutionDomainType.FluenceOfRhoAndZAndFt) ||
                (MapTypeOptionVm.SelectedValue == MapType.PhotonHittingDensity &&
                 PhotonHittingDensitySolutionDomainTypeOptionVm.SelectedValue ==
                 FluenceSolutionDomainType.FluenceOfRhoAndZAndFt);

    public OptionViewModel<MapType> MapTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(MapTypeOptionVm));
        }
    }

    public FluenceSolutionDomainOptionViewModel FluenceSolutionDomainTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(FluenceSolutionDomainTypeOptionVm));
        }
    }

    public FluenceSolutionDomainOptionViewModel AbsorbedEnergySolutionDomainTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(AbsorbedEnergySolutionDomainTypeOptionVm));
        }
    }

    public FluenceSolutionDomainOptionViewModel PhotonHittingDensitySolutionDomainTypeOptionVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(PhotonHittingDensitySolutionDomainTypeOptionVm));
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

    public RangeViewModel RhoRangeVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(RhoRangeVm));
        }
    }

    public double SourceDetectorSeparation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(SourceDetectorSeparation));
        }
    }

    public double TimeModulationFrequency
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(TimeModulationFrequency));
        }
    }

    public RangeViewModel ZRangeVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ZRangeVm));
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

    private async Task ExecuteFluenceSolver_Executed()
    {
        //clear the map in case there is no new mapview
        WindowViewModel.Current.MapVm.ClearMap.Execute(null);

        CanRunSolver = false;
        CanCancelSolver = true;
        try
        {
            MainWindow.Current.Wait.Visibility = Visibility.Visible;
            ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Begin();
            try
            {
                await GetMapData();
            }
            catch (ArgumentException ex)
            {
                WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
                    StringLookup.GetLocalizedString("Label_FluenceSolver\r"));
                WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute("ERROR IN INPUT:" + ex.Message + "\r");
            }
        }

        catch (OperationCanceledException)
        {
            ((Storyboard) MainWindow.Current.FindResource("WaitStoryboard")).Stop();
            MainWindow.Current.Wait.Visibility = Visibility.Hidden;
            WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute("Operation Cancelled\r");
        }
        finally
        {
            ((Storyboard) MainWindow.Current.FindResource("WaitStoryboard")).Stop();
            MainWindow.Current.Wait.Visibility = Visibility.Hidden;
            CanRunSolver = true;
        }

        CanRunSolver = true;
        CanCancelSolver = false;
    }

    internal async Task<bool> GetMapData()
    {
        _currentCancellationTokenSource = new CancellationTokenSource();

        var mapData = await Task.Run(() => ExecuteForwardSolver(_currentCancellationTokenSource.Token));
        if (mapData == null) return true;
        WindowViewModel.Current.MapVm.PlotMap.Execute(mapData);
        var opString = OpticalPropertyVm + "\r";
        if (IsMultiRegion && ForwardSolver is TwoLayerSDAForwardSolver)
        {
            var regions = ((MultiRegionTissueViewModel)TissueInputVm).GetTissueInput().Regions;
            opString = "\rLayer 0: μa=" + regions[0].RegionOP.Mua + " μs'=" + regions[0].RegionOP.Musp + " n=" + regions[0].RegionOP.N + "\r" +
                       "Layer 1: μa=" + regions[1].RegionOP.Mua + " μs'=" + regions[1].RegionOP.Musp + " n=" + regions[0].RegionOP.N + "\r";
        }
            
        WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute(
            StringLookup.GetLocalizedString("Label_FluenceSolver") + opString);
        return true;
    }

    private void CancelFluenceSolver_Executed()
    {
        CanCancelSolver = false;
        _currentCancellationTokenSource?.Cancel();
        ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Stop();
        MainWindow.Current.Wait.Visibility = Visibility.Hidden;
        WindowViewModel.Current.TextOutputVm.TextOutputPostMessage.Execute("Canceling... \r");

    }

    private void UpdateAvailableOptions()
    {
        if (IsFluence)
        {
            switch (ForwardSolverTypeOptionVm.SelectedValue)
            {
                case ForwardSolverType.DistributedGaussianSourceSDA:
                    FluenceSolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    FluenceSolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndFtEnabled = false;
                    if (FluenceSolutionDomainTypeOptionVm.SelectedValue is FluenceSolutionDomainType.FluenceOfRhoAndZAndTime or FluenceSolutionDomainType.FluenceOfRhoAndZAndFt)
                    {
                        FluenceSolutionDomainTypeOptionVm.SelectedValue = FluenceSolutionDomainType.FluenceOfRhoAndZ;
                        OnPropertyChanged(nameof(FluenceSolutionDomainTypeOptionVm));
                    }
                    break;
                default: // default handles all other ForwardSolverTypes
                    FluenceSolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    FluenceSolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndFtEnabled = true;
                    break;
            }
        }
        if (IsAbsorbedEnergy)
        {
            switch (ForwardSolverTypeOptionVm.SelectedValue)
            {
                case ForwardSolverType.DistributedGaussianSourceSDA:
                case ForwardSolverType.TwoLayerSDA:
                    AbsorbedEnergySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    AbsorbedEnergySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndFtEnabled = false;
                    if (AbsorbedEnergySolutionDomainTypeOptionVm.SelectedValue ==
                        FluenceSolutionDomainType.FluenceOfRhoAndZAndFt)
                    {
                        AbsorbedEnergySolutionDomainTypeOptionVm.SelectedValue =
                            FluenceSolutionDomainType.FluenceOfRhoAndZ;
                        OnPropertyChanged(nameof(AbsorbedEnergySolutionDomainTypeOptionVm));
                    }
                    break;
                default: // default handles all other ForwardSolverTypes
                    AbsorbedEnergySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    AbsorbedEnergySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndFtEnabled = true;
                    break;
            }
        }
        if (IsPhotonHittingDensity)
        {
            switch (ForwardSolverTypeOptionVm.SelectedValue)
            {
                case ForwardSolverType.DistributedGaussianSourceSDA:
                case ForwardSolverType.TwoLayerSDA:
                    PhotonHittingDensitySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    PhotonHittingDensitySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndFtEnabled = false;
                    if (PhotonHittingDensitySolutionDomainTypeOptionVm.SelectedValue ==
                        FluenceSolutionDomainType.FluenceOfRhoAndZAndFt)
                    {
                        PhotonHittingDensitySolutionDomainTypeOptionVm.SelectedValue =
                            FluenceSolutionDomainType.FluenceOfRhoAndZ;
                        OnPropertyChanged(nameof(PhotonHittingDensitySolutionDomainTypeOptionVm));
                    }
                    break;
                default: // default handles all other ForwardSolverTypes
                    PhotonHittingDensitySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    PhotonHittingDensitySolutionDomainTypeOptionVm.IsFluenceOfRhoAndZAndFtEnabled = true;
                    break;
            }
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
                    StringLookup.GetLocalizedString("Heading_OpticalProperties"));
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
                throw new ArgumentOutOfRangeException(nameof(tissueType) );
        }
    }

    private MapData ExecuteForwardSolver(CancellationToken cancellationToken)
        // todo: simplify method calls to ComputationFactory, as with Forward/InverseSolver(s)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var opticalProperties = GetOpticalProperties();
            // could be OpticalProperties[] or IOpticalPropertyRegion[][]

        //double[] rhos = RhoRangeVM.Values.Reverse().Concat(RhoRangeVM.Values).ToArray();
        var rhos = RhoRangeVm.Values.Reverse().Select(rho => -rho).Concat(RhoRangeVm.Values).ToArray();
        var zs = ZRangeVm.Values.ToArray();

        double[][] independentValues = [rhos, zs];

        var sd = GetSelectedSolutionDomain();
        // todo: too much thinking at the VM layer?
        var constantValues = Array.Empty<double>();

        if (ComputationFactory.IsSolverWithConstantValues(sd.SelectedValue))
        {
            switch (sd.SelectedValue)
            {
                case FluenceSolutionDomainType.FluenceOfRhoAndZAndFt:
                    constantValues = [TimeModulationFrequency];
                    break;
                default:
                    constantValues = [sd.ConstantAxesVMs[0].AxisValue];
                    break;
            }
        }

        var independentAxes =
            GetIndependentVariableAxesInOrder(
                sd.IndependentVariableAxisOptionVm.SelectedValue,
                IndependentVariableAxis.Z);

        double[] results;
        if (ComputationFactory.IsComplexSolver(sd.SelectedValue))
        {
            Complex[] fluence;
            if (IsMultiRegion)
            {
                fluence =
                    ComputationFactory.ComputeFluenceComplex(
                        ForwardSolverTypeOptionVm.SelectedValue,
                        sd.SelectedValue,
                        independentAxes,
                        independentValues,
                        ((IOpticalPropertyRegion[][]) opticalProperties)[0],
                        constantValues);
            }
            else
            {
                fluence =
                    ComputationFactory.ComputeFluenceComplex(
                        ForwardSolverTypeOptionVm.SelectedValue,
                        sd.SelectedValue,
                        independentAxes,
                        independentValues,
                        ((OpticalProperties[]) opticalProperties)[0],
                        constantValues);
            }

            switch (MapTypeOptionVm.SelectedValue)
            {
                case MapType.Fluence:
                    results = [.. fluence.Select(f => f.Magnitude)];
                    break;
                case MapType.AbsorbedEnergy:
                    results =
                        [.. ComputationFactory.GetAbsorbedEnergy(fluence,
                            ((OpticalProperties[]) opticalProperties)[0].Mua).Select(a => a.Magnitude)];
                     break;
                case MapType.PhotonHittingDensity:
                    switch (PhotonHittingDensitySolutionDomainTypeOptionVm.SelectedValue)
                    {
                        case FluenceSolutionDomainType.FluenceOfRhoAndZAndFt:
                            results = [.. ComputationFactory.GetPHD(
                                ForwardSolverTypeOptionVm.SelectedValue,
                                [.. fluence],
                                SourceDetectorSeparation,
                                TimeModulationFrequency,
                                (OpticalProperties[]) opticalProperties,
                                independentValues[0],
                                independentValues[1])];
                            break;
                        case FluenceSolutionDomainType.FluenceOfRhoAndZ:
                        case FluenceSolutionDomainType.FluenceOfFxAndZ:
                        case FluenceSolutionDomainType.FluenceOfRhoAndZAndTime:
                        case FluenceSolutionDomainType.FluenceOfFxAndZAndTime:
                        case FluenceSolutionDomainType.FluenceOfFxAndZAndFt:
                            throw new InvalidEnumArgumentException(StringLookup.GetLocalizedString("Error_SolutionDomainNotSupported"));
                        default:
                            throw new InvalidEnumArgumentException(StringLookup.GetLocalizedString("Error_NoSolutionDomainExists"));
                    }
                break;
                default:
                    throw new InvalidEnumArgumentException(StringLookup.GetLocalizedString("Error_NoMapTypeExists"));
            }
        }
        else
        {
            double[] fluence;
            if (IsMultiRegion)
            {
                fluence = [.. ComputationFactory.ComputeFluence(
                    ForwardSolverTypeOptionVm.SelectedValue,
                    sd.SelectedValue,
                    independentAxes,
                    independentValues,
                    ((IOpticalPropertyRegion[][]) opticalProperties)[0],
                    constantValues)];
            }
            else
            {
                fluence = [.. ComputationFactory.ComputeFluence(
                    ForwardSolverTypeOptionVm.SelectedValue,
                    sd.SelectedValue,
                    independentAxes,
                    independentValues,
                    (OpticalProperties[]) opticalProperties,
                    constantValues)];
            }

            switch (MapTypeOptionVm.SelectedValue)
            {
                case MapType.Fluence:
                    results = fluence;
                    break;
                case MapType.AbsorbedEnergy:
                    if (IsMultiRegion)
                    {
                        if (ForwardSolver is TwoLayerSDAForwardSolver)
                        {
                            var regions = ((MultiRegionTissueViewModel) TissueInputVm).GetTissueInput().Regions
                                .Select(region => (ILayerOpticalPropertyRegion) region).ToArray();
                            var muas = GetRhoZMuaArrayFromLayerRegions(regions, rhos, zs);
                            results = [.. ComputationFactory.GetAbsorbedEnergy(fluence, muas)];
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        // Note: the line below was originally overwriting the multi-region results.
                        // I think this was a bug (DJC 7/11/14)
                        results =
                            [.. ComputationFactory.GetAbsorbedEnergy(fluence,
                                ((OpticalProperties[]) opticalProperties)[0].Mua)];
                    }
                    break;
                case MapType.PhotonHittingDensity:
                    switch (PhotonHittingDensitySolutionDomainTypeOptionVm.SelectedValue)
                    {
                        case FluenceSolutionDomainType.FluenceOfRhoAndZ:
                            if (IsMultiRegion)
                            {
                                var nop = (IOpticalPropertyRegion[][]) opticalProperties;
                                results = [.. ComputationFactory.GetPHD(
                                    ForwardSolverTypeOptionVm.SelectedValue,
                                    fluence,
                                    SourceDetectorSeparation,
                                    [.. (from LayerTissueRegion tissue in nop[0] select tissue.RegionOP)],
                                    independentValues[0],
                                    independentValues[1])];
                            }
                            else
                            {
                                results = [.. ComputationFactory.GetPHD(
                                    ForwardSolverTypeOptionVm.SelectedValue,
                                    fluence,
                                    SourceDetectorSeparation,
                                    (OpticalProperties[]) opticalProperties,
                                    independentValues[0],
                                    independentValues[1])];
                            }
                            break;
                        case FluenceSolutionDomainType.FluenceOfFxAndZ:
                        case FluenceSolutionDomainType.FluenceOfRhoAndZAndTime:
                        case FluenceSolutionDomainType.FluenceOfFxAndZAndTime:
                        case FluenceSolutionDomainType.FluenceOfRhoAndZAndFt:
                        case FluenceSolutionDomainType.FluenceOfFxAndZAndFt:
                            throw new InvalidEnumArgumentException(StringLookup.GetLocalizedString("Error_SolutionDomainNotSupported"));
                    default:
                        throw new InvalidEnumArgumentException(StringLookup.GetLocalizedString("Error_NoSolutionDomainExists"));
                }
                break;
                default:
                    throw new InvalidEnumArgumentException(StringLookup.GetLocalizedString("Error_NoMapTypeExists"));
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        // flip the array (since it goes over zs and then rhos, while map wants rhos and then zs
        var destinationArray = new double[results.Length];
        long index = 0;
        for (var rhoi = 0; rhoi < rhos.Length; rhoi++)
        {
            for (var zi = 0; zi < zs.Length; zi++)
            {
                destinationArray[rhoi + rhos.Length*zi] = results[index++];
            }
        }

        const double dRho = 1D;
        const double dZ = 1D;
        var dRhos = rhos.Select(rho => 2*Math.PI*Math.Abs(rho)*dRho).ToArray();
        var dZs = zs.Select(_ => dZ).ToArray();
        //var twoRhos = Enumerable.Concat(rhos.Reverse(), rhos).ToArray();
        //var twoDRhos = Enumerable.Concat(dRhos.Reverse(), dRhos).ToArray();

        return new MapData(destinationArray, rhos, zs, dRhos, dZs);
    }

    private static IndependentVariableAxis[] GetIndependentVariableAxesInOrder(params IndependentVariableAxis[] axes)
    {
        if (axes.Length <= 0)
            throw new ArgumentNullException(nameof(axes));

        var sortedAxes = axes.OrderBy(ax => ax.GetMaxArgumentLocation()).ToArray();

        return sortedAxes;
    }

    private FluenceSolutionDomainOptionViewModel GetSelectedSolutionDomain()
    {
        return MapTypeOptionVm.SelectedValue switch
        {
            MapType.Fluence => FluenceSolutionDomainTypeOptionVm,
            MapType.AbsorbedEnergy => AbsorbedEnergySolutionDomainTypeOptionVm,
            MapType.PhotonHittingDensity => PhotonHittingDensitySolutionDomainTypeOptionVm,
            _ => throw new InvalidEnumArgumentException(StringLookup.GetLocalizedString("Error_NoMapTypeExists"))
        };
    }

    private object GetOpticalProperties()
    {
        // todo: add-in spectral panel data
        //if (SolutionDomainTypeOptionVM.IndependentVariableAxisOptionVM.SelectedValues.Contains(IndependentVariableAxis.Wavelength) &&
        //    UseSpectralPanelData &&
        //    SolverDemoViewModel.Current != null &&
        //    SolverDemoViewModel.Current.SpectralMappingVM != null)
        //{
        //    var tissue = SolverDemoViewModel.Current.SpectralMappingVM.SelectedTissue;
        //    var wavelengths = GetParameterValues(IndependentVariableAxis.Wavelength);
        //    var ops = tissue.GetOpticalProperties(wavelengths);

        //    if (IsMultiRegion && MultiRegionTissueVM != null)
        //    {
        //        return ops.Select(op =>
        //        {
        //            var regions = MultiRegionTissueVM.GetTissueInput().Regions.Select(region => (IOpticalPropertyRegion)region).ToArray();
        //            regions.ForEach(region =>
        //            {
        //                region.RegionOP.Mua = op.Mua;
        //                region.RegionOP.Musp = op.Musp;
        //                region.RegionOP.G = op.G;
        //                region.RegionOP.N = op.N;
        //            });
        //            return regions.ToArray();
        //        });
        //    }

        //    return ops;
        //}

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