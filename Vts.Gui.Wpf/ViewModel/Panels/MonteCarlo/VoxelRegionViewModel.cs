using Vts.Common;
using Vts.Gui.Wpf.Extensions;
using Vts.Gui.Wpf.ViewModel.Controls;
using Vts.MonteCarlo.Extensions;
using Vts.MonteCarlo.Tissues;

namespace Vts.Gui.Wpf.ViewModel.Panels.MonteCarlo;

public class VoxelRegionViewModel : BindableObject
{
    private string _name;
    private OpticalPropertyViewModel _opticalPropertyVm;
    private readonly VoxelTissueRegion _region;

    public VoxelRegionViewModel(VoxelTissueRegion region, string name)
    {
        _region = region;
        _name = name ?? "";
        _opticalPropertyVm = new OpticalPropertyViewModel(_region.RegionOP, StringLookup.GetLocalizedString("Measurement_Inv_mm"), "", true, true, true, false);
    }

    public VoxelRegionViewModel() : this(new VoxelTissueRegion(), "")
    {
    }

    public string Name
    {
        get => _name + (_region.IsAir() ? StringLookup.GetLocalizedString("Label_Air") : StringLookup.GetLocalizedString("Label_Tissue"));
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public DoubleRange X
    {
        get => _region.X;
        set
        {
            _region.X = value;
            OnPropertyChanged(nameof(X));
        }
    }

    public DoubleRange Y
    {
        get => _region.Y;
        set
        {
            _region.Y = value;
            OnPropertyChanged(nameof(Y));
        }
    }

    public DoubleRange Z
    {
        get => _region.Z;
        set
        {
            _region.Z = value;
            OnPropertyChanged(nameof(Z));
        }
    }

    public OpticalPropertyViewModel OpticalPropertyVm
    {
        get => _opticalPropertyVm;
        set
        {
            _opticalPropertyVm = value;
            OnPropertyChanged(nameof(OpticalPropertyVm));
        }
    }

    public string Units => StringLookup.GetLocalizedString("Measurement_mm");

    public bool IsLayer => false;

    public bool IsVoxel => true;

}