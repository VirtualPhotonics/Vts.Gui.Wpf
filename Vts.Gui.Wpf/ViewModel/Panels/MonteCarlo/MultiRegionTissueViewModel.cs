using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Vts.Extensions;
using Vts.Gui.Wpf.Extensions;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.ViewModel
{
    public class MultiRegionTissueViewModel : BindableObject
    {
        private int _currentRegionIndex;
        private readonly ITissueInput _input;

        private ObservableCollection<object> _regionsVM;

        public MultiRegionTissueViewModel(ITissueInput input)
        {
            _input = input;

            switch (input.TissueType)
            {
                case "MultiLayer":
                    var multiLayerTissueInput = (MultiLayerTissueInput) _input;
                    _regionsVM = new ObservableCollection<object>(
                        multiLayerTissueInput.Regions.Select((r, i) => (object) new LayerRegionViewModel(
                            (LayerTissueRegion) r,
                            StringLookup.GetLocalizedString("Label_Layer") + i)));
                    break;
                case "SingleEllipsoid":
                    var singleEllipsoidTissueInput = (SingleEllipsoidTissueInput) _input;
                    _regionsVM = new ObservableCollection<object>(
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
                    _regionsVM = new ObservableCollection<object>(
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

            _currentRegionIndex = 0;
        }

        public MultiRegionTissueViewModel()
            : this(new MultiLayerTissueInput())
        {
        }

        public ObservableCollection<object> RegionsVM
        {
            get => _regionsVM;
            set
            {
                _regionsVM = value;
                //OnPropertyChanged("LayerRegionsVM");
                OnPropertyChanged("RegionsVM"); // ckh: RegionsVM is what is bound in xaml, LayerRegionsVM is in old comment
            }
        }

        public int CurrentRegionIndex
        {
            get => _currentRegionIndex;
            set
            {
                if ((value < _regionsVM.Count) && (value >= 0))
                {
                    _currentRegionIndex = value;
                    OnPropertyChanged("CurrentRegionIndex");
                    OnPropertyChanged("MinimumRegionIndex");
                    OnPropertyChanged("MaximumRegionIndex");
                }
            }
        }

        public int MinimumRegionIndex => 0;

        public int MaximumRegionIndex => _regionsVM != null ? _regionsVM.Count - 1 : 0;

        public ITissueInput GetTissueInput()
        {
            return _input;
        }
    }
}