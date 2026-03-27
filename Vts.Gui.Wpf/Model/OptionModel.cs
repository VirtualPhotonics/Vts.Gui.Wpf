namespace Vts.Gui.Wpf.Model;

public abstract class OptionModel : BindableObject
{
    protected const int UnsetSortValue = int.MinValue;

    protected OptionModel(string displayName, int id, string groupName, bool enableMultiSelect, int sortValue)
    {
        DisplayName = displayName;
        GroupName = groupName;
        Id = id;
        SortValue = sortValue;
        MultiSelectEnabled = enableMultiSelect;
        IsEnabled = true;
    }

    /// <summary>
    ///     Returns the user-friendly name of this option.
    /// </summary>
    public string DisplayName { set; get; }

    /// <summary>
    ///     Returns the user-friendly name of this option.
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    ///     Gets/sets whether this option is in the selected state.
    ///     When this property is set to a new value, this object's
    ///     PropertyChanged event is raised.
    /// </summary>
    public bool IsSelected
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    /// <summary>
    ///     Gets/sets whether this option is in the enabled state.
    ///     When this property is set to a new value, this object's
    ///     PropertyChanged event is raised.
    /// </summary>
    public bool IsEnabled
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(IsEnabled));
        }
    }

    /// <summary>
    ///     Returns the value used to sort this option.
    ///     The default sort value is Int32.MinValue.
    /// </summary>
    public int SortValue { get; set; }

    /// <summary>
    ///     Returns the value used to identify this option.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     multi-select
    /// </summary>
    public bool MultiSelectEnabled { get; set; }
}