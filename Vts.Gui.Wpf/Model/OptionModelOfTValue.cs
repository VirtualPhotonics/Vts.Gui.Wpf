using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Model;

/// <summary>
///     Represents a value with a user-friendly name that can be selected by the user.
///     From Josh Smith &amp; Karl Schifflett's Code Project article on localization:
///     <see cref="http://www.codeproject.com/KB/WPF/InternationalizedWizard.aspx">
///     "http://www.codeproject.com/KB/WPF/InternationalizedWizard.aspx"</see>
/// </summary>
/// <typeparam name="TValue">The type of value represented by the option.</typeparam>
public class OptionModel<TValue>(
    string displayName,
    TValue value,
    int id,
    string groupName,
    bool enableMultiSelect,
    int sortValue)
    : OptionModel(displayName, id, groupName, enableMultiSelect, sortValue)
{
    public OptionModel(string displayName, TValue value, int id, string groupName, bool enableMultiSelect)
        : this(displayName, value, id, groupName, enableMultiSelect, UnsetSortValue)
    {
    }

    /// <summary>
    ///     Returns the user-friendly name of this option.
    /// </summary>
    public TValue Value { get; } = value;

    public int CompareTo(OptionModel<TValue> other)
    {
        if (other == null)
            return -1;

        if (SortValue == UnsetSortValue && other.SortValue == UnsetSortValue)
        {
            return string.Compare(DisplayName, other.DisplayName, StringComparison.Ordinal);
        }
        if (SortValue != UnsetSortValue && other.SortValue != UnsetSortValue)
        {
            return SortValue.CompareTo(other.SortValue);
        }
        if (SortValue != UnsetSortValue && other.SortValue == UnsetSortValue)
        {
            return -1;
        }
        return +1;
    }

    /// <summary>
    /// Creates a Dictionary of options.
    /// If no options (params TValue[] values) are specified, it will use all the
    /// available choices
    /// </summary>
    /// <param name="handler">Property changed even handler</param>
    /// <param name="groupName">Name of the option group</param>
    /// <param name="initialValue">Initial value</param>
    /// <param name="allValues">All possible options</param>
    /// <param name="enableMultiSelect">Allows multiple options to be selected</param>
    /// <returns>A dictionary of options</returns>
    public static Dictionary<TValue, OptionModel<TValue>> CreateAvailableOptions(
        PropertyChangedEventHandler handler, string groupName, TValue initialValue, TValue[] allValues,
        bool enableMultiSelect)
    {
        var enumType = typeof(TValue);
        var isStringEnum = enumType == typeof(string);
        if (!enumType.IsEnum && !isStringEnum)
        {
            throw new ArgumentException(enumType.Name + StringLookup.GetLocalizedString("Exception_EnumOrString"));
        }

        var list = new List<OptionModel<TValue>>();
        if (allValues == null || allValues.Length == 0)
        {
            allValues = isStringEnum ? [] : EnumHelper.GetValues<TValue>();
        }
        var names =
            allValues.Select(value => isStringEnum ? value as string : (value as Enum).GetInternationalizedString())
                .ToArray();

        for (var i = 0; i < allValues.Length; i++)
        {
            var name = names[i].Length > 0 ? names[i] : allValues[i].ToString();
            var option = new OptionModel<TValue>(name, allValues[i], i, groupName, enableMultiSelect);
            option.PropertyChanged += handler;
            list.Add(option);
        }

        //removed call to sort, which was re-arranging the enum choices
        //convention is to let the first enum choice be the default selection list.Sort()

        if (list.Count > 0)
        {
            var option = list.FirstOrDefault(
                optionModel => EqualityComparer<TValue>.Default.Equals(optionModel.Value, initialValue));
            if (option == null) return list.ToDictionary(item => item.Value);
            option.IsSelected = true;
            option.IsEnabled = !enableMultiSelect;
        }
        return list.ToDictionary(item => item.Value);
    }
}