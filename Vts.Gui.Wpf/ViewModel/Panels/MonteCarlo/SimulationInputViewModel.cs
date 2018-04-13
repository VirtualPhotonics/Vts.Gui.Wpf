﻿using System;
using System.IO;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

#if WHITELIST
using Vts.Gui.Wpf.ViewModel.Application;
#endif

namespace Vts.Gui.Wpf.ViewModel
{
    public class SimulationInputViewModel : BindableObject
    {
        private SimulationInput _simulationInput;
        private SimulationOptionsViewModel _simulationOptionsVM;

        private object _tissueInputVM;
        private OptionViewModel<string> _tissueTypeVM;

        private string _outputName;

        public SimulationInputViewModel(SimulationInput input)
        {
            _simulationInput = input; // use the property to invoke the appropriate change notification

            _simulationOptionsVM = new SimulationOptionsViewModel(_simulationInput.Options);
            _outputName = input.OutputName;

#if WHITELIST 
            TissueTypeVM = new OptionViewModel<string>("Tissue Type:", true, _simulationInput.TissueInput.TissueType, WhiteList.TissueTypes);
#else
            TissueTypeVM = new OptionViewModel<string>("Tissue Type:", true, _simulationInput.TissueInput.TissueType,
                new[]
                {
                    "MultiLayer",
                    "SingleEllipsoid",
                    "SingleVoxel"
                });
#endif
            _simulationOptionsVM.PropertyChanged += (sender, args) =>
            {
                if (_simulationOptionsVM.TrackStatistics && _simulationOptionsVM.OutputFolder != null)
                {
                    _simulationInput.OutputName = Path.Combine(_simulationOptionsVM.OutputFolder, _outputName) ;
                }
            };
            _tissueTypeVM.PropertyChanged += (sender, args) =>
            {
                switch (_tissueTypeVM.SelectedValue)
                {
                    case "MultiLayer":
                        _simulationInput.TissueInput = new MultiLayerTissueInput();
                        break;
                    case "SingleEllipsoid":
                        _simulationInput.TissueInput = new SingleEllipsoidTissueInput();
                        break;
                    case "SingleVoxel":
                        _simulationInput.TissueInput = new SingleVoxelTissueInput();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                UpdateTissueTypeVM(_simulationInput.TissueInput.TissueType);
            };

            UpdateTissueInputVM(_simulationInput.TissueInput);
        }

        public SimulationInputViewModel()
            : this(new SimulationInput())
        {
        }

        public SimulationInput SimulationInput
        {
            get { return _simulationInput; }
            set
            {
                _simulationInput = value;
                // OnPropertyChanged("SimulationInput");  // nobody binds to this
                OnPropertyChanged("N");
                _simulationOptionsVM.SimulationOptions = _simulationInput.Options;
                UpdateTissueInputVM(_simulationInput.TissueInput);
                _outputName = _simulationInput.OutputName;
            }
        }

        public long N
        {
            get { return _simulationInput.N; }
            set
            {
                _simulationInput.N = value;
                OnPropertyChanged("N");
            }
        }


        public SimulationOptionsViewModel SimulationOptionsVM
        {
            get { return _simulationOptionsVM; }
            set
            {
                _simulationOptionsVM = value;
                OnPropertyChanged("SimulationOptionsVM");
            }
        }

        public OptionViewModel<string> TissueTypeVM
        {
            get { return _tissueTypeVM; }
            set
            {
                _tissueTypeVM = value;
                OnPropertyChanged("TissueTypeVM");
            }
        }

        public object TissueInputVM
        {
            get { return _tissueInputVM; }
            set
            {
                _tissueInputVM = value;
                OnPropertyChanged("TissueInputVM");
            }
        }

        private void UpdateTissueTypeVM(string tissueType)
        {
            switch (tissueType)
            {
                case "SemiInfinite":
                    _simulationInput.TissueInput = new SemiInfiniteTissueInput();
                    break;
                case "MultiLayer":
                    _simulationInput.TissueInput = new MultiLayerTissueInput();
                    break;
                case "SingleEllipsoid":
                    _simulationInput.TissueInput = new SingleEllipsoidTissueInput();
                    break;
                case "SingleVoxel":
                    _simulationInput.TissueInput = new SingleVoxelTissueInput();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TissueTypeVM.Options[tissueType].IsSelected = true;
        }

        private void UpdateTissueInputVM(ITissueInput tissueInput)
        {
            switch (tissueInput.TissueType)
            {
                case "MultiLayer":
                    TissueInputVM = new MultiRegionTissueViewModel((MultiLayerTissueInput) tissueInput);
                    break;
                case "SingleEllipsoid":
                    TissueInputVM = new MultiRegionTissueViewModel((SingleEllipsoidTissueInput) tissueInput);
                    break;
                case "SingleVoxel":
                    TissueInputVM = new MultiRegionTissueViewModel((SingleVoxelTissueInput)tissueInput);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}