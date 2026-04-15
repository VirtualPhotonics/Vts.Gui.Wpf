using System.ComponentModel;
using System.IO;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.ViewModel.Controls;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;
#if WHITELIST
using Vts.Gui.Wpf.ViewModel.Application;
#endif

namespace Vts.Gui.Wpf.ViewModel.Panels.MonteCarlo;

public class SimulationInputViewModel : BindableObject
{
    private SimulationInput _simulationInput;
    private SimulationOptionsViewModel _simulationOptionsVm;

    private object _tissueInputVm;
    private OptionViewModel<string> _tissueTypeVm;

    private string _outputName;

    public SimulationInputViewModel(SimulationInput input)
    {
        _simulationInput = input; // use the property to invoke the appropriate change notification

        _simulationOptionsVm = new SimulationOptionsViewModel(_simulationInput.Options);
        _outputName = input.OutputName;

#if WHITELIST 
        TissueTypeVm = new OptionViewModel<string>("Tissue Type:", true, _simulationInput.TissueInput.TissueType, WhiteList.TissueTypes);
#else
        TissueTypeVm = new OptionViewModel<string>("Tissue Type:", true, _simulationInput.TissueInput.TissueType,
        [
            "MultiLayer",
            "SingleEllipsoid",
            "SingleVoxel"
            ]);
#endif
        UpdateTissueTypeVm(_simulationInput.TissueInput.TissueType);
        _simulationOptionsVm.PropertyChanged += (_, _) =>
        {
            if (_simulationOptionsVm.TrackStatistics && _simulationOptionsVm.OutputFolder != null)
            {
                _simulationInput.OutputName = Path.Combine(_simulationOptionsVm.OutputFolder, _outputName) ;
            }
        };
        _tissueTypeVm.PropertyChanged += (_, _) =>
        {
            _simulationInput.TissueInput = _tissueTypeVm.SelectedValue switch
            {
                "MultiLayer" => new MultiLayerTissueInput(),
                "SingleEllipsoid" => new SingleEllipsoidTissueInput(),
                "SingleVoxel" => new SingleVoxelTissueInput(),
                _ => throw new InvalidEnumArgumentException(
                    StringLookup.GetLocalizedString("Error_NoTissueTypeExists"))
            };
            UpdateTissueTypeVm(_simulationInput.TissueInput.TissueType);
        };

        UpdateTissueInputVm(_simulationInput.TissueInput);
    }

    public SimulationInputViewModel()
        : this(new SimulationInput())
    {
    }

    public SimulationInput SimulationInput
    {
        get => _simulationInput;
        set
        {
            _simulationInput = value;
            // OnPropertyChanged("SimulationInput");  // nobody binds to this
            OnPropertyChanged(nameof(N));
            _simulationOptionsVm.SimulationOptions = _simulationInput.Options;
            UpdateTissueInputVm(_simulationInput.TissueInput);
            _outputName = _simulationInput.OutputName;
        }
    }

    public long N
    {
        get => _simulationInput.N;
        set
        {
            _simulationInput.N = value;
            OnPropertyChanged(nameof(N));
        }
    }


    public SimulationOptionsViewModel SimulationOptionsVm
    {
        get => _simulationOptionsVm;
        set
        {
            _simulationOptionsVm = value;
            OnPropertyChanged(nameof(SimulationOptionsVm));
        }
    }

    public OptionViewModel<string> TissueTypeVm
    {
        get => _tissueTypeVm;
        set
        {
            _tissueTypeVm = value;
            OnPropertyChanged(nameof(TissueTypeVm));
        }
    }

    public object TissueInputVm
    {
        get => _tissueInputVm;
        set
        {
            _tissueInputVm = value;
            OnPropertyChanged(nameof(TissueInputVm));
        }
    }

    private void UpdateTissueTypeVm(string tissueType)
    {
        _simulationInput.TissueInput = tissueType switch
        {
            "SemiInfinite" => new SemiInfiniteTissueInput(),
            "MultiLayer" => new MultiLayerTissueInput(),
            "SingleEllipsoid" => new SingleEllipsoidTissueInput(),
            "SingleVoxel" => new SingleVoxelTissueInput(),
            _ => _simulationInput.TissueInput
        };

        _tissueInputVm = _simulationInput.TissueInput;
        TissueTypeVm.Options[tissueType].IsSelected = true;
    }

    private void UpdateTissueInputVm(ITissueInput tissueInput)
    {
        TissueInputVm = tissueInput.TissueType switch
        {
            "MultiLayer" => new MultiRegionTissueViewModel((MultiLayerTissueInput)tissueInput),
            "SingleEllipsoid" => new MultiRegionTissueViewModel((SingleEllipsoidTissueInput)tissueInput),
            "SingleVoxel" => new MultiRegionTissueViewModel((SingleVoxelTissueInput)tissueInput),
            _ => TissueInputVm
        };
    }
}