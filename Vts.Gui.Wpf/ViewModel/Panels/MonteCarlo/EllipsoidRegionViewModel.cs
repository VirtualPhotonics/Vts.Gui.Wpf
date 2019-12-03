using Vts.Gui.Wpf.Extensions;
using Vts.MonteCarlo.Extensions;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.ViewModel
{
    public class EllipsoidRegionViewModel : BindableObject
    {
        private string _name;
        private OpticalPropertyViewModel _opticalPropertyVm;
        private readonly EllipsoidTissueRegion _region;

        public EllipsoidRegionViewModel(EllipsoidTissueRegion region, string name)
        {
            _region = region;
            _name = name ?? "";
            _opticalPropertyVm = new OpticalPropertyViewModel(_region.RegionOP, StringLookup.GetLocalizedString("Measurement_Inv_mm"), "", true, true, true, false);
        }

        public EllipsoidRegionViewModel() : this(new EllipsoidTissueRegion(), "")
        {
        }

        public string Name
        {
            get => _name + (_region.IsAir() ? StringLookup.GetLocalizedString("Label_Air") : StringLookup.GetLocalizedString("Label_Tissue"));
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public double X
        {
            get => _region.Center.X;
            set
            {
                _region.Center.X = value;
                OnPropertyChanged("X");
            }
        }

        public double Y
        {
            get => _region.Center.Y;
            set
            {
                _region.Center.Y = value;
                OnPropertyChanged("Y");
            }
        }

        public double Z
        {
            get => _region.Center.Z;
            set
            {
                _region.Center.Z = value;
                OnPropertyChanged("Z");
            }
        }

        public double Dx
        {
            get => _region.Dx;
            set
            {
                _region.Dx = value;
                OnPropertyChanged("Dx");
            }
        }

        public double Dy
        {
            get => _region.Dy;
            set
            {
                _region.Dy = value;
                OnPropertyChanged("Dy");
            }
        }

        public double Dz
        {
            get => _region.Dz;
            set
            {
                _region.Dz = value;
                OnPropertyChanged("Dz");
            }
        }

        public OpticalPropertyViewModel OpticalPropertyVm
        {
            get => _opticalPropertyVm;
            set
            {
                _opticalPropertyVm = value;
                OnPropertyChanged("OpticalPropertyVm");
            }
        }

        public string Units => StringLookup.GetLocalizedString("Measurement_mm");

        public bool IsLayer => false;

        public bool IsEllipsoid => true;
    }
}