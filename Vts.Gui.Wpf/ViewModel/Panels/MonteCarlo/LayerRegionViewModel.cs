using Vts.Gui.Wpf.Extensions;
using Vts.MonteCarlo.Extensions;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.ViewModel
{
    public class LayerRegionViewModel : BindableObject
    {
        private string _name;
        private OpticalPropertyViewModel _opticalPropertyVM;
        private readonly LayerTissueRegion _region;
        private RangeViewModel _zRangeVM;

        public LayerRegionViewModel(LayerTissueRegion region, string name)
        {
            _region = region;
            _name = name ?? "";
            _zRangeVM = new RangeViewModel(_region.ZRange, StringLookup.GetLocalizedString("Measurement_mm"), IndependentVariableAxis.Z, "", false);
            _opticalPropertyVM = new OpticalPropertyViewModel(_region.RegionOP, StringLookup.GetLocalizedString("Measurement_Inv_mm"), "", true, true, true, true);
            _opticalPropertyVM.PropertyChanged += (s, a) => OnPropertyChanged("Name");
        }

        //public LayerRegionViewModel(LayerRegion region) : this(region, "")
        //{
        //}

        public LayerRegionViewModel()
            : this(new LayerTissueRegion(), "")
        {
        }

        public RangeViewModel ZRangeVM
        {
            get { return _zRangeVM; }
            set
            {
                _zRangeVM = value;
                OnPropertyChanged("ZRangeVM");
            }
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

        public OpticalPropertyViewModel OpticalPropertyVM
        {
            get { return _opticalPropertyVM; }
            set
            {
                _opticalPropertyVM = value;
                OnPropertyChanged("OpticalPropertyVM");
            }
        }

        public bool IsLayer
        {
            get { return true; }
        }

        public bool IsEllipsoid
        {
            get { return false; }
        }
    }
}