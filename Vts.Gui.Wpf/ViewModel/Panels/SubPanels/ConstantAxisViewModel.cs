namespace Vts.Gui.Wpf.ViewModel.Panels.SubPanels;

public class ConstantAxisViewModel : BindableObject
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

    public double AxisValue
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(AxisValue));
        }
    }

    public int ImageHeight
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ImageHeight));
        }
    } = 50;
}