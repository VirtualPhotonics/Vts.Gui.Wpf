using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Vts.Extensions;
using Vts.Gui.Wpf.Extensions;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.ViewModel;

public class MultiRegionTissueViewModel : BindableObject
{
    private int _currentRegionIndex;
    private readonly ITissueInput _input;

    private ObservableCollection<object> _regionsVm;

    public MultiRegionTissueViewModel(ITissueInput input)
    {
        _currentRegionIndex = 0;
        _input = input;

        switch (input.TissueType)
        {
            case "MultiLayer":
                var multiLayerTissueInput = (MultiLayerTissueInput) _input;
                _regionsVm = new ObservableCollection<object>(
                    multiLayerTissueInput.Regions.Select((r, i) => (object) new LayerRegionViewModel(
                        (LayerTissueRegion) r,
                        StringLookup.GetLocalizedString("Label_Layer") + i)));
                break;
            case "SingleEllipsoid":
                var singleEllipsoidTissueInput = (SingleEllipsoidTissueInput) _input;
                _regionsVm = new ObservableCollection<object>(
                    singleEllipsoidTissueInput.LayerRegions
                        .Select((r, i) => (object) new LayerRegionViewModel(
                            (LayerTissueRegion) r,
                            StringLookup.GetLocalizedString("Label_Layer") + i))
                        .Concat(
                            new EllipsoidRegionViewModel(
                                (EllipsoidTissueRegion) singleEllipsoidTissueInput.EllipsoidRegion,
                                StringLookup.GetLocalizedString("Label_EllipsoidRegion"))));
                break;
            case "SingleVoxel":
                var singleVoxelTissueInput = (SingleVoxelTissueInput)_input;
                _regionsVm = new ObservableCollection<object>(
                    singleVoxelTissueInput.LayerRegions
                        .Select((r, i) => (object)new LayerRegionViewModel(
                            (LayerTissueRegion)r,
                            StringLookup.GetLocalizedString("Label_Layer") + i))
                        .Concat(
                            new VoxelRegionViewModel(
                                (VoxelTissueRegion)singleVoxelTissueInput.VoxelRegion,
                                StringLookup.GetLocalizedString("Label_VoxelRegion"))));
                break;
            default:
                throw new InvalidEnumArgumentException(StringLookup.GetLocalizedString("Error_NoTissueTypeExists"));
        }
    }

    public MultiRegionTissueViewModel()
        : this(new MultiLayerTissueInput())
    {
    }

    public ObservableCollection<object> RegionsVm
    {
        get => _regionsVm;
        set
        {
            _regionsVm = value;
            OnPropertyChanged(nameof(RegionsVm));
        }
    }

    public int CurrentRegionIndex
    {
        get => _currentRegionIndex;
        set
        {
            if ((value < _regionsVm.Count) && (value >= 0))
            {
                _currentRegionIndex = value;
                OnPropertyChanged(nameof(CurrentRegionIndex));
                OnPropertyChanged(nameof(MinimumRegionIndex));
                OnPropertyChanged(nameof(MaximumRegionIndex));
            }
        }
    }

    public int MinimumRegionIndex => 0;

    public int MaximumRegionIndex => _regionsVm != null ? _regionsVm.Count - 1 : 0;

    public ITissueInput GetTissueInput()
    {
        return _input;
    }
}