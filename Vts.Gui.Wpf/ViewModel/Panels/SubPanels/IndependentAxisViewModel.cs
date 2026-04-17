using Vts.Gui.Wpf.ViewModel.Controls;

namespace Vts.Gui.Wpf.ViewModel.Panels.SubPanels;

public class IndependentAxisViewModel : BindableObject
{
    public IndependentVariableAxis AxisType
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(AxisType));
        }
    }

    public string AxisLabel
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(AxisLabel));
        }
    }

    public string AxisUnits
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(AxisUnits));
        }
    }

    public RangeViewModel AxisRangeVm
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(AxisRangeVm));
        }
    }
}