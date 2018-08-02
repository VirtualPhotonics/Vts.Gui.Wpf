using Vts.Common;
using Vts.Gui.Wpf.Extensions;
using Vts.MonteCarlo.Extensions;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.ViewModel
{
    public class VoxelRegionViewModel : BindableObject
    {
        private string _name;
        private OpticalPropertyViewModel _opticalPropertyVM;
        private readonly VoxelTissueRegion _region;
        private string _units;

        public VoxelRegionViewModel(VoxelTissueRegion region, string name)
        {
            _region = region;
            _name = name ?? "";
            _opticalPropertyVM = new OpticalPropertyViewModel(_region.RegionOP, StringLookup.GetLocalizedString("Measurement_Inv_mm"), "", true, true, true, false);
        }

        public VoxelRegionViewModel() : this(new VoxelTissueRegion(), "")
        {
        }

        public string Name
        {
            get { return _name + (_region.IsAir() ? StringLookup.GetLocalizedString("Label_Air") : StringLookup.GetLocalizedString("Label_Tissue")); }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public DoubleRange X
        {
            get { return _region.X; }
            set
            {
                _region.X = value;
                OnPropertyChanged("X");
            }
        }

        public DoubleRange Y
        {
            get { return _region.Y; }
            set
            {
                _region.Y = value;
                OnPropertyChanged("Y");
            }
        }

        public DoubleRange Z
        {
            get { return _region.Z; }
            set
            {
                _region.Z = value;
                OnPropertyChanged("Z");
            }
        }

        public OpticalPropertyViewModel OpticalPropertyVM
        {
            get { return _opticalPropertyVM; }
            set
            {
                _opticalPropertyVM = value;
                OnPropertyChanged("OpticalPropertyVM");
            }
        }

        public string Units
        {
            get { return StringLookup.GetLocalizedString("Measurement_mm"); }
        }

        public bool IsLayer
        {
            get { return false; }
        }

        public bool IsVoxel
        {
            get { return true; }
        }

    }
}