namespace Vts.Gui.Wpf.ViewModel;

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

    public RangeViewModel AxisRangeVM
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(AxisRangeVM));
        }
    }
}