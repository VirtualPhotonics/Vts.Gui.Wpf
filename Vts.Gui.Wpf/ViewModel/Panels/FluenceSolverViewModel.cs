﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.Command;
using Vts.Common;
using Vts.Factories;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.Model;
using Vts.IO;
using Vts.Modeling.ForwardSolvers;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary>
    ///     View model implementing Fluence Solver panel functionality
    /// </summary>
    public class FluenceSolverViewModel : BindableObject
    {
        // the following function determines a flattened mua array for layered tissue
        private static readonly Func<ILayerOpticalPropertyRegion[], double[], double[], double[]>
            getRhoZMuaArrayFromLayerRegions = (regions, rhos, zs) =>
            {
                var numBins = rhos.Length*zs.Length;
                var muaArray = new double[numBins];
                for (var i = 0; i < zs.Length; i++)
                {
                    var layerIndex = DetectorBinning.WhichBin(zs[i], regions.Select(r => r.ZRange.Stop).ToArray());
                    for (var j = 0; j < rhos.Length; j++)
                    {
                        muaArray[i*rhos.Length + j] = regions[layerIndex].RegionOP.Mua;
                    }
                }
                return muaArray;
            };

        private FluenceSolutionDomainOptionViewModel _AbsorbedEnergySolutionDomainTypeOptionVM;

        // private fields to cache created instances of tissue inputs, created on-demand in GetTissueInputVM (vs up-front in constructor)
        private OpticalProperties _currentHomogeneousOpticalProperties;
        private MultiLayerTissueInput _currentMultiLayerTissueInput;
        private SemiInfiniteTissueInput _currentSemiInfiniteTissueInput;
        private SingleEllipsoidTissueInput _currentSingleEllipsoidTissueInput;
        private FluenceSolutionDomainOptionViewModel _FluenceSolutionDomainTypeOptionVM;
        private OptionViewModel<ForwardSolverType> _ForwardSolverTypeOptionVM;
        private OptionViewModel<MapType> _MapTypeOptionVM;
        private FluenceSolutionDomainOptionViewModel _PhotonHittingDensitySolutionDomainTypeOptionVM;
        //private OptionViewModel<ForwardAnalysisType> _ForwardAnalysisTypeOptionVM;

        private RangeViewModel _RhoRangeVM;
        private double _SourceDetectorSeparation;
        private double _TimeModulationFrequency;

        private object _tissueInputVM;
            // either an OpticalPropertyViewModel or a MultiRegionTissueViewModel is stored here, and dynamically displayed

        private RangeViewModel _ZRangeVM;
        private CancellationTokenSource _currentCancellationTokenSource;
        private bool _canRunSolver;
        private bool _canCancelSolver;
        private MapData mapData;

        public FluenceSolverViewModel()
        {
            RhoRangeVM = new RangeViewModel(new DoubleRange(0.1, 19.9, 100), StringLookup.GetLocalizedString("Measurement_mm"), IndependentVariableAxis.Rho, "");
            ZRangeVM = new RangeViewModel(new DoubleRange(0.1, 19.9, 100), StringLookup.GetLocalizedString("Measurement_mm"), IndependentVariableAxis.Z, "");
            SourceDetectorSeparation = 10.0;
            TimeModulationFrequency = 0.1;
            _tissueInputVM = new OpticalPropertyViewModel(new OpticalProperties(),
                IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(),
                StringLookup.GetLocalizedString("Heading_OpticalProperties"));

            // right now, we're doing manual databinding to the selected item. need to enable databinding 
            // confused, though - do we need to use strings? or, how to make generics work with dependency properties?
            ForwardSolverTypeOptionVM = new OptionViewModel<ForwardSolverType>(
                "Forward Model",
                false,
                new[]
                {
                    ForwardSolverType.DistributedPointSourceSDA,
                    ForwardSolverType.PointSourceSDA,
                    ForwardSolverType.DistributedGaussianSourceSDA,
                    ForwardSolverType.TwoLayerSDA
                }); // explicitly enabling these for the workshop;

            FluenceSolutionDomainTypeOptionVM = new FluenceSolutionDomainOptionViewModel(StringLookup.GetLocalizedString("Heading_FluenceSolutionDomain"),
                FluenceSolutionDomainType.FluenceOfRhoAndZ);
            FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled = false;
            FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled = true;
            AbsorbedEnergySolutionDomainTypeOptionVM =
                new FluenceSolutionDomainOptionViewModel(StringLookup.GetLocalizedString("Heading_AbsorbedEnergySolutionDomain"),
                    FluenceSolutionDomainType.FluenceOfRhoAndZ);
            AbsorbedEnergySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled = false;
            AbsorbedEnergySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled = true;
            PhotonHittingDensitySolutionDomainTypeOptionVM =
                new FluenceSolutionDomainOptionViewModel(StringLookup.GetLocalizedString("Heading_PHDSolutionDomain"),
                    FluenceSolutionDomainType.FluenceOfRhoAndZ);
            PhotonHittingDensitySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled = false;
            PhotonHittingDensitySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled = true;
            PropertyChangedEventHandler updateSolutionDomain = (sender, args) =>
            {
                if (args.PropertyName == "IndependentAxisType")
                {
                    RhoRangeVM = ((FluenceSolutionDomainOptionViewModel) sender).IndependentAxesVMs[0].AxisRangeVM;
                }
                // todo: must this fire on ANY property, or is there a specific one we can listen to, as above?
                OnPropertyChanged("IsTimeFrequencyDomain");
            };
            FluenceSolutionDomainTypeOptionVM.PropertyChanged += updateSolutionDomain;
            AbsorbedEnergySolutionDomainTypeOptionVM.PropertyChanged += updateSolutionDomain;
            PhotonHittingDensitySolutionDomainTypeOptionVM.PropertyChanged += updateSolutionDomain;

            MapTypeOptionVM = new OptionViewModel<MapType>(
                "Map Type",
                new[]
                {
                    MapType.Fluence,
                    MapType.AbsorbedEnergy,
                    MapType.PhotonHittingDensity
                });

            MapTypeOptionVM.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SelectedValues")
                {
                    OnPropertyChanged("IsFluence");
                    OnPropertyChanged("IsAbsorbedEnergy");
                    OnPropertyChanged("IsPhotonHittingDensity");
                    OnPropertyChanged("IsTimeFrequencyDomain");
                    UpdateAvailableOptions();
                }
            };

            ForwardSolverTypeOptionVM.PropertyChanged += (sender, args) =>
            {
                OnPropertyChanged("ForwardSolver");
                OnPropertyChanged("IsGaussianForwardModel");
                OnPropertyChanged("IsMultiRegion");
                OnPropertyChanged("IsSemiInfinite");
                TissueInputVM = GetTissueInputVM(IsMultiRegion ? "MultiLayer" : "SemiInfinite");
                UpdateAvailableOptions();
                OnPropertyChanged("IsTimeFrequencyDomain");
            };

            ExecuteFluenceSolverCommand = new RelayCommand(() => ExecuteFluenceSolver_Executed(null, null));
            CancelFluenceSolverCommand = new RelayCommand(() => CancelFluenceSolver_Executed(null, null));
            _canRunSolver = true;
            _canCancelSolver = false;
        }

        public RelayCommand ExecuteFluenceSolverCommand { get; set; }
        public RelayCommand CancelFluenceSolverCommand { get; set; }

        public bool CanRunSolver
        {
            get { return _canRunSolver; }
            set
            {
                _canRunSolver = value;
                OnPropertyChanged("CanRunSolver");
            }
        }

        public bool CanCancelSolver
        {
            get { return _canCancelSolver; }
            set
            {
                _canCancelSolver = value;
                OnPropertyChanged("CanCancelSolver");
            }
        }
        public IForwardSolver ForwardSolver
        {
            get
            {
                return SolverFactory.GetForwardSolver(
                    ForwardSolverTypeOptionVM.SelectedValue);
            }
        }

        public bool IsGaussianForwardModel
        {
            get { return ForwardSolverTypeOptionVM.SelectedValue.IsGaussianForwardModel(); }
        }

        public bool IsMultiRegion
        {
            get { return ForwardSolverTypeOptionVM.SelectedValue.IsMultiRegionForwardModel(); }
        }

        public bool IsSemiInfinite
        {
            get { return !ForwardSolverTypeOptionVM.SelectedValue.IsMultiRegionForwardModel(); }
        }

        public bool IsFluence
        {
            get { return MapTypeOptionVM.SelectedValue == MapType.Fluence; }
        }

        public bool IsAbsorbedEnergy
        {
            get { return MapTypeOptionVM.SelectedValue == MapType.AbsorbedEnergy; }
        }

        public bool IsPhotonHittingDensity
        {
            get { return MapTypeOptionVM.SelectedValue == MapType.PhotonHittingDensity; }
        }

        public bool IsTimeFrequencyDomain
        {
            get
            {
                return
                    (MapTypeOptionVM.SelectedValue == MapType.Fluence &&
                     FluenceSolutionDomainTypeOptionVM.SelectedValue == FluenceSolutionDomainType.FluenceOfRhoAndZAndFt) ||
                    (MapTypeOptionVM.SelectedValue == MapType.AbsorbedEnergy &&
                     AbsorbedEnergySolutionDomainTypeOptionVM.SelectedValue ==
                     FluenceSolutionDomainType.FluenceOfRhoAndZAndFt) ||
                    (MapTypeOptionVM.SelectedValue == MapType.PhotonHittingDensity &&
                     PhotonHittingDensitySolutionDomainTypeOptionVM.SelectedValue ==
                     FluenceSolutionDomainType.FluenceOfRhoAndZAndFt);
            }
        }

        public OptionViewModel<MapType> MapTypeOptionVM
        {
            get { return _MapTypeOptionVM; }
            set
            {
                _MapTypeOptionVM = value;
                OnPropertyChanged("MapTypeOptionVM");
            }
        }

        public FluenceSolutionDomainOptionViewModel FluenceSolutionDomainTypeOptionVM
        {
            get { return _FluenceSolutionDomainTypeOptionVM; }
            set
            {
                _FluenceSolutionDomainTypeOptionVM = value;
                OnPropertyChanged("FluenceSolutionDomainTypeOptionVM");
            }
        }

        public FluenceSolutionDomainOptionViewModel AbsorbedEnergySolutionDomainTypeOptionVM
        {
            get { return _AbsorbedEnergySolutionDomainTypeOptionVM; }
            set
            {
                _AbsorbedEnergySolutionDomainTypeOptionVM = value;
                OnPropertyChanged("AbsorbedEnergySolutionDomainTypeOptionVM");
            }
        }

        public FluenceSolutionDomainOptionViewModel PhotonHittingDensitySolutionDomainTypeOptionVM
        {
            get { return _PhotonHittingDensitySolutionDomainTypeOptionVM; }
            set
            {
                _PhotonHittingDensitySolutionDomainTypeOptionVM = value;
                OnPropertyChanged("PhotonHittingDensitySolutionDomainTypeOptionVM");
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

        public RangeViewModel RhoRangeVM
        {
            get { return _RhoRangeVM; }
            set
            {
                _RhoRangeVM = value;
                OnPropertyChanged("RhoRangeVM");
            }
        }

        public double SourceDetectorSeparation
        {
            get { return _SourceDetectorSeparation; }
            set
            {
                _SourceDetectorSeparation = value;
                OnPropertyChanged("SourceDetectorSeparation");
            }
        }

        public double TimeModulationFrequency
        {
            get { return _TimeModulationFrequency; }
            set
            {
                _TimeModulationFrequency = value;
                OnPropertyChanged("TimeModulationFrequency");
            }
        }

        public RangeViewModel ZRangeVM
        {
            get { return _ZRangeVM; }
            set
            {
                _ZRangeVM = value;
                OnPropertyChanged("ZRangeVM");
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

        private async void ExecuteFluenceSolver_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //clear the map in case there is no new mapview
            WindowViewModel.Current.MapVM.ClearMap.Execute(null);

            CanRunSolver = false;
            CanCancelSolver = true;
            try
            {
                MainWindow.Current.Wait.Visibility = Visibility.Visible;
                ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Begin();
                await GetMapData();
            }

            catch (OperationCanceledException)
            {
                ((Storyboard) MainWindow.Current.FindResource("WaitStoryboard")).Stop();
                MainWindow.Current.Wait.Visibility = Visibility.Hidden;
                WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute("Operation Cancelled\r");
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

            mapData = await Task.Run(() => ExecuteForwardSolver(_currentCancellationTokenSource.Token));
            //var mapData = ExecuteForwardSolver();
            if (mapData != null)
            {
                WindowViewModel.Current.MapVM.PlotMap.Execute(mapData);
                var opString = OpticalPropertyVM + "\r";
                if (IsMultiRegion)
                {
                    if (ForwardSolver is TwoLayerSDAForwardSolver)
                    {
                        ITissueRegion[] regions = ((MultiRegionTissueViewModel)TissueInputVM).GetTissueInput().Regions;
                        opString = "\rLayer 0: μa=" + regions[0].RegionOP.Mua + " μs'=" + regions[0].RegionOP.Musp + " n=" + regions[0].RegionOP.N + "\r" +
                                   "Layer 1: μa=" + regions[1].RegionOP.Mua + " μs'=" + regions[1].RegionOP.Musp + " n=" + regions[0].RegionOP.N + "\r";
                    }
                }
                WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute(
                    StringLookup.GetLocalizedString("Label_FluenceSolver") + opString);
            }
            return true;
        }

        private void CancelFluenceSolver_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CanCancelSolver = false;
            if (_currentCancellationTokenSource != null)
            {
                _currentCancellationTokenSource.Cancel();
            }
            mapData = null;
            ((Storyboard)MainWindow.Current.FindResource("WaitStoryboard")).Stop();
            MainWindow.Current.Wait.Visibility = Visibility.Hidden;
            WindowViewModel.Current.TextOutputVM.TextOutput_PostMessage.Execute("Canceling... \r");

        }

        private PlotAxesLabels GetPlotLabels()
        {
            var sd = GetSelectedSolutionDomain();

            var axesLabels = new PlotAxesLabels(
                sd.SelectedDisplayName, sd.SelectedValue.GetUnits(),
                sd.IndependentAxesVMs.First(),
                sd.ConstantAxesVMs);

            return axesLabels;
        }

        private void UpdateAvailableOptions()
        {
            if (IsFluence)
            {
                if (ForwardSolverTypeOptionVM.SelectedValue == ForwardSolverType.DistributedGaussianSourceSDA)
                {
                    FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled = false;
                    if (FluenceSolutionDomainTypeOptionVM.SelectedValue ==
                        FluenceSolutionDomainType.FluenceOfRhoAndZAndTime ||
                        FluenceSolutionDomainTypeOptionVM.SelectedValue ==
                        FluenceSolutionDomainType.FluenceOfRhoAndZAndFt)
                    {
                        FluenceSolutionDomainTypeOptionVM.SelectedValue = FluenceSolutionDomainType.FluenceOfRhoAndZ;
                        OnPropertyChanged("FluenceSolutionDomainTypeOptionVM");
                    }
                }
                else
                {
                    FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    FluenceSolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled = true;
                }
            }
            if (IsAbsorbedEnergy)
            {
                if (ForwardSolverTypeOptionVM.SelectedValue ==
                    ForwardSolverType.DistributedGaussianSourceSDA || ForwardSolverTypeOptionVM.SelectedValue ==
                    ForwardSolverType.TwoLayerSDA)
                {
                    AbsorbedEnergySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    AbsorbedEnergySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled = false;
                    if (AbsorbedEnergySolutionDomainTypeOptionVM.SelectedValue ==
                        FluenceSolutionDomainType.FluenceOfRhoAndZAndFt)
                    {
                        AbsorbedEnergySolutionDomainTypeOptionVM.SelectedValue =
                            FluenceSolutionDomainType.FluenceOfRhoAndZ;
                        OnPropertyChanged("AbsorbedEnergySolutionDomainTypeOptionVM");
                    }
                }
                else
                {
                    AbsorbedEnergySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    AbsorbedEnergySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled = true;
                }
            }
            if (IsPhotonHittingDensity)
            {
                if (ForwardSolverTypeOptionVM.SelectedValue ==
                    ForwardSolverType.DistributedGaussianSourceSDA || ForwardSolverTypeOptionVM.SelectedValue ==
                    ForwardSolverType.TwoLayerSDA)
                {
                    PhotonHittingDensitySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    PhotonHittingDensitySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled = false;
                    if (PhotonHittingDensitySolutionDomainTypeOptionVM.SelectedValue ==
                        FluenceSolutionDomainType.FluenceOfRhoAndZAndFt)
                    {
                        PhotonHittingDensitySolutionDomainTypeOptionVM.SelectedValue =
                            FluenceSolutionDomainType.FluenceOfRhoAndZ;
                        OnPropertyChanged("PhotonHittingDensitySolutionDomainTypeOptionVM");
                    }
                }
                else
                {
                    PhotonHittingDensitySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndTimeEnabled = false;
                    PhotonHittingDensitySolutionDomainTypeOptionVM.IsFluenceOfRhoAndZAndFtEnabled = true;
                }
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
                        StringLookup.GetLocalizedString("Heading_OpticalProperties"));
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
                    throw new ArgumentOutOfRangeException();
            }
        }

        // todo: rename? this was to get a concise name for the legend
        private string GetLegendLabel()
        {
            var modelString =
                ForwardSolverTypeOptionVM.SelectedValue == ForwardSolverType.DistributedPointSourceSDA ||
                ForwardSolverTypeOptionVM.SelectedValue == ForwardSolverType.PointSourceSDA ||
                ForwardSolverTypeOptionVM.SelectedValue == ForwardSolverType.DistributedGaussianSourceSDA
                    ? "Model - SDA\r"
                    : "Model - MC scaled\r";
            var opString = "μa=" + OpticalPropertyVM.Mua + "\rμs'=" + OpticalPropertyVM.Musp;
            if (IsMultiRegion)
            {
                ITissueRegion[] regions = null;
                if (ForwardSolver is TwoLayerSDAForwardSolver)
                {
                    regions = ((MultiRegionTissueViewModel) TissueInputVM).GetTissueInput().Regions;
                    opString =
                        "μa1=" + regions[0].RegionOP.Mua + " μs'1=" + regions[0].RegionOP.Musp + "\r" +
                        "μa2=" + regions[1].RegionOP.Mua + " μs'2=" + regions[1].RegionOP.Musp;
                }
            }
            else
            {
                var opticalProperties = ((OpticalPropertyViewModel) TissueInputVM).GetOpticalProperties();
                opString = "μa=" + opticalProperties.Mua + " \rμs'=" + opticalProperties.Musp;
            }

            return modelString + opString;
        }

        private MapData ExecuteForwardSolver(CancellationToken cancellationToken)
            // todo: simplify method calls to ComputationFactory, as with Forward/InverseSolver(s)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var opticalProperties = GetOpticalProperties();
                // could be OpticalProperties[] or IOpticalPropertyRegion[][]

            //double[] rhos = RhoRangeVM.Values.Reverse().Concat(RhoRangeVM.Values).ToArray();
            var rhos = RhoRangeVM.Values.Reverse().Select(rho => -rho).Concat(RhoRangeVM.Values).ToArray();
            var zs = ZRangeVM.Values.ToArray();

            double[][] independentValues = {rhos, zs};

            var sd = GetSelectedSolutionDomain();
            // todo: too much thinking at the VM layer?
            var constantValues = new double[0];

            if (ComputationFactory.IsSolverWithConstantValues(sd.SelectedValue))
            {
                switch (sd.SelectedValue)
                {
                    case FluenceSolutionDomainType.FluenceOfRhoAndZAndFt:
                        constantValues = new[] {TimeModulationFrequency};
                        break;
                    default:
                        constantValues = new[] {sd.ConstantAxesVMs[0].AxisValue};
                        break;
                }
            }

            var independentAxes =
                GetIndependentVariableAxesInOrder(
                    sd.IndependentVariableAxisOptionVM.SelectedValue,
                    IndependentVariableAxis.Z);

            double[] results = null;
            if (ComputationFactory.IsComplexSolver(sd.SelectedValue))
            {
                Complex[] fluence;
                if (IsMultiRegion)
                {
                    fluence =
                        ComputationFactory.ComputeFluenceComplex(
                            ForwardSolverTypeOptionVM.SelectedValue,
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
                            ForwardSolverTypeOptionVM.SelectedValue,
                            sd.SelectedValue,
                            independentAxes,
                            independentValues,
                            ((OpticalProperties[]) opticalProperties)[0],
                            constantValues);
                }

                switch (MapTypeOptionVM.SelectedValue)
                {
                    case MapType.Fluence:
                        results = fluence.Select(f => f.Magnitude).ToArray();
                        break;
                    case MapType.AbsorbedEnergy:
                        results =
                            ComputationFactory.GetAbsorbedEnergy(fluence,
                                ((OpticalProperties[]) opticalProperties)[0].Mua).Select(a => a.Magnitude).ToArray();
                            // todo: is this correct?? DC 12/08/12
                        break;
                    case MapType.PhotonHittingDensity:
                        switch (PhotonHittingDensitySolutionDomainTypeOptionVM.SelectedValue)
                        {
                            case FluenceSolutionDomainType.FluenceOfRhoAndZAndFt:
                                results = ComputationFactory.GetPHD(
                                    ForwardSolverTypeOptionVM.SelectedValue,
                                    fluence.ToArray(),
                                    SourceDetectorSeparation,
                                    TimeModulationFrequency,
                                    (OpticalProperties[]) opticalProperties,
                                    independentValues[0],
                                    independentValues[1]).ToArray();
                                break;
                            case FluenceSolutionDomainType.FluenceOfFxAndZAndFt:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("FluenceSolutionDomainType");
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("MapType");
                }
            }
            else
            {
                double[] fluence;
                if (IsMultiRegion)
                {
                    fluence = ComputationFactory.ComputeFluence(
                        ForwardSolverTypeOptionVM.SelectedValue,
                        sd.SelectedValue,
                        independentAxes,
                        independentValues,
                        ((IOpticalPropertyRegion[][]) opticalProperties)[0],
                        constantValues).ToArray();
                }
                else
                {
                    fluence = ComputationFactory.ComputeFluence(
                        ForwardSolverTypeOptionVM.SelectedValue,
                        sd.SelectedValue,
                        independentAxes,
                        independentValues,
                        (OpticalProperties[]) opticalProperties,
                        constantValues).ToArray();
                }

                switch (MapTypeOptionVM.SelectedValue)
                {
                    case MapType.Fluence:
                        results = fluence;
                        break;
                    case MapType.AbsorbedEnergy:
                        if (IsMultiRegion)
                        {
                            if (ForwardSolver is TwoLayerSDAForwardSolver)
                            {
                                var regions = ((MultiRegionTissueViewModel) TissueInputVM).GetTissueInput().Regions
                                    .Select(region => (ILayerOpticalPropertyRegion) region).ToArray();
                                var muas = getRhoZMuaArrayFromLayerRegions(regions, rhos, zs);
                                results = ComputationFactory.GetAbsorbedEnergy(fluence, muas).ToArray();
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            // Note: the line below was originally overwriting the multi-region results. I think this was a bug (DJC 7/11/14)
                            results =
                                ComputationFactory.GetAbsorbedEnergy(fluence,
                                    ((OpticalProperties[]) opticalProperties)[0].Mua).ToArray();
                        }
                        break;
                    case MapType.PhotonHittingDensity:
                        switch (PhotonHittingDensitySolutionDomainTypeOptionVM.SelectedValue)
                        {
                            case FluenceSolutionDomainType.FluenceOfRhoAndZ:
                                if (IsMultiRegion)
                                {
                                    var nop = (IOpticalPropertyRegion[][]) opticalProperties;
                                    results = ComputationFactory.GetPHD(
                                        ForwardSolverTypeOptionVM.SelectedValue,
                                        fluence,
                                        SourceDetectorSeparation,
                                        (from LayerTissueRegion tissue in nop[0] select tissue.RegionOP).ToArray(),
                                        independentValues[0],
                                        independentValues[1]).ToArray();
                                }
                                else
                                {
                                    results = ComputationFactory.GetPHD(
                                        ForwardSolverTypeOptionVM.SelectedValue,
                                        fluence,
                                        SourceDetectorSeparation,
                                        (OpticalProperties[]) opticalProperties,
                                        independentValues[0],
                                        independentValues[1]).ToArray();
                                }
                                break;
                            case FluenceSolutionDomainType.FluenceOfFxAndZ:
                                break;
                            case FluenceSolutionDomainType.FluenceOfRhoAndZAndTime:
                                break;
                            case FluenceSolutionDomainType.FluenceOfFxAndZAndTime:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(
                                    "PhotonHittingDensitySolutionDomainTypeOptionVM.SelectedValue");
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("MapTypeOptionVM.SelectedValue");
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

            var dRho = 1D;
            var dZ = 1D;
            var dRhos = rhos.Select(rho => 2*Math.PI*Math.Abs(rho)*dRho).ToArray();
            var dZs = zs.Select(z => dZ).ToArray();
            //var twoRhos = Enumerable.Concat(rhos.Reverse(), rhos).ToArray();
            //var twoDRhos = Enumerable.Concat(dRhos.Reverse(), dRhos).ToArray();

            return new MapData(destinationArray, rhos, zs, dRhos, dZs);
        }

        private static IndependentVariableAxis[] GetIndependentVariableAxesInOrder(params IndependentVariableAxis[] axes)
        {
            if (axes.Length <= 0)
                throw new ArgumentNullException("axes");

            var sortedAxes = axes.OrderBy(ax => ax.GetMaxArgumentLocation()).ToArray();

            return sortedAxes;
        }

        private FluenceSolutionDomainOptionViewModel GetSelectedSolutionDomain()
        {
            switch (MapTypeOptionVM.SelectedValue)
            {
                case MapType.Fluence:
                    return FluenceSolutionDomainTypeOptionVM;
                case MapType.AbsorbedEnergy:
                    return AbsorbedEnergySolutionDomainTypeOptionVM;
                case MapType.PhotonHittingDensity:
                    return PhotonHittingDensitySolutionDomainTypeOptionVM;
                default:
                    throw new ArgumentException(StringLookup.GetLocalizedString("Error_NoSolutionDomainExists"),
                        "MapTypeOptionVM.SelectedValue");
            }
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