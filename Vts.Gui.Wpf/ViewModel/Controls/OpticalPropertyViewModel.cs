namespace Vts.Gui.Wpf.ViewModel;

/// <summary>
///     View model exposing the OpticalProperties model class with change notification
/// </summary>
public class OpticalPropertyViewModel : BindableObject
{
    private bool _enableG;
    private bool _enableMua;
    private bool _enableMusp;
    private bool _enableN;

    public OpticalPropertyViewModel()
        : this(
            new OpticalProperties(0.01, 1.0, 0.8, 1.4),
            IndependentVariableAxisUnits.InverseMM.GetInternationalizedString(),
            "")
    {
    }

    public OpticalPropertyViewModel(OpticalProperties opticalProperties, string units, string title)
        : this(opticalProperties, units, title, true, true, false, true)
    {
    }

    public OpticalPropertyViewModel(OpticalProperties opticalProperties, string units, string title,
        bool enableMua, bool enableMusp, bool enableG, bool enableN)
    {
        OpticalProperties = opticalProperties;
        Units = units;
        Title = title;

        _enableMua = enableMua;
        _enableMusp = enableMusp;
        _enableG = enableG;
        _enableN = enableN;
    }

    public double Mua
    {
        get => OpticalProperties.Mua;
        set
        {
            OpticalProperties.Mua = value;
            OnPropertyChanged(nameof(Mua));
        }
    }

    public double Musp
    {
        get => OpticalProperties.Musp;
        set
        {
            OpticalProperties.Musp = value;
            OnPropertyChanged(nameof(Musp));
        }
    }

    public double G
    {
        get => OpticalProperties.G;
        set
        {
            OpticalProperties.G = value;
            OnPropertyChanged(nameof(G));
        }
    }

    public double N
    {
        get => OpticalProperties.N;
        set
        {
            OpticalProperties.N = value;
            OnPropertyChanged(nameof(N));
        }
    }

    public string Units
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Units));
        }
    }

    public string Title
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Title));
        }
    }

    public bool ShowTitle => Title != "";

    public bool EnableMua
    {
        get => _enableMua;
        set
        {
            _enableMua = value;
            OnPropertyChanged(nameof(EnableMua));
        }
    }

    public bool EnableMusp
    {
        get => _enableMusp;
        set
        {
            _enableMusp = value;
            OnPropertyChanged(nameof(EnableMusp));
        }
    }

    public bool EnableG
    {
        get => _enableG;
        set
        {
            _enableG = value;
            OnPropertyChanged(nameof(EnableG));
        }
    }

    public bool EnableN
    {
        get => _enableN;
        set
        {
            _enableN = value;
            OnPropertyChanged(nameof(EnableN));
        }
    }

    private OpticalProperties OpticalProperties { get; }

    /// <summary>
    ///     Helper method. Can't be bound to.
    /// </summary>
    /// <returns></returns>
    public OpticalProperties GetOpticalProperties()
    {
        return OpticalProperties;
    }

    /// <summary>
    ///     Helper method.
    /// </summary>
    /// <returns></returns>
    public void SetOpticalProperties(OpticalProperties op)
    {
        OpticalProperties.Mua = op.Mua;
        OpticalProperties.Musp = op.Musp;
        OpticalProperties.G = op.G;
        OpticalProperties.N = op.N;

        OnPropertyChanged(nameof(Mua));
        OnPropertyChanged(nameof(Musp));
        OnPropertyChanged(nameof(G));
        OnPropertyChanged(nameof(N));
    }

    public override string ToString()
    {
        return OpticalProperties + "; Units = " + Units;
    }
}