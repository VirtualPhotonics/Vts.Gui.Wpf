using Vts.Common;

namespace Vts.Gui.Wpf.ViewModel;

/// <summary>
///     View model exposing the Position model class with change notification
/// </summary>
public class PositionViewModel : BindableObject
{
    // Position model - backing store for public properties
    private readonly Position _position;

    public PositionViewModel() : this(new Position(0, 0, 0), "mm", "Position:")
    {
    }

    public PositionViewModel(Position position, string units, string title)
    {
        _position = position;
        Units = units;
        Title = title;
    }

    /// <summary>
    ///     A double representing the x-component of the position
    /// </summary>
    public double X
    {
        get => _position.X;
        set
        {
            _position.X = value;
            OnPropertyChanged(nameof(X));
        }
    }

    /// <summary>
    ///     A double representing the y-component of the position
    /// </summary>
    public double Y
    {
        get => _position.Y;
        set
        {
            _position.Y = value;
            OnPropertyChanged(nameof(Y));
        }
    }

    /// <summary>
    ///     A double representing the z-component of the position
    /// </summary>
    public double Z
    {
        get => _position.Z;
        set
        {
            _position.Z = value;
            OnPropertyChanged(nameof(Z));
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
            OnPropertyChanged(nameof(ShowTitle));
        }
    }

    public bool ShowTitle => Title.Length > 0;

    public Position GetPosition()
    {
        return _position;
    }
}