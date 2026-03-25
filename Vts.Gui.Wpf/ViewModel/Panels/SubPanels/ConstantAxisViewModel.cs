using Vts.Gui.Wpf.Input;

namespace Vts.Gui.Wpf.ViewModel;

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
            if (AxisType == IndependentVariableAxis.Wavelength)
            {
                // update the world that this has changed, and react to it if desired (e.g. in Spectral Panel)
                Commands.SetWavelength.Execute(AxisValue, null);
            }

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