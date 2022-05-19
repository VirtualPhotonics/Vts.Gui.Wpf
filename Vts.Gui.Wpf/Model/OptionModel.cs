using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.Model
{
    public abstract class OptionModel : BindableObject
    {
        protected const int UNSET_SORT_VALUE = int.MinValue;

        private bool _isSelected;
        private bool _isEnabled;

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
            get => _isSelected;
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        /// <summary>
        ///     Gets/sets whether this option is in the enabled state.
        ///     When this property is set to a new value, this object's
        ///     PropertyChanged event is raised.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
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

    /// <summary>
    ///     Represents a value with a user-friendly name that can be selected by the user.
    ///     From Josh Smith &amp; Karl Schifflett's Code Project article on localization:
    ///     <see cref="http://www.codeproject.com/KB/WPF/InternationalizedWizard.aspx">"http://www.codeproject.com/KB/WPF/InternationalizedWizard.aspx"</see>
    /// </summary>
    /// <typeparam name="TValue">The type of value represented by the option.</typeparam>
    public class OptionModel<TValue> :
        OptionModel
    {
        public OptionModel(string displayName, TValue value, int id, string groupName, bool enableMultiSelect)
            : this(displayName, value, id, groupName, enableMultiSelect, UNSET_SORT_VALUE)
        {
        }

        public OptionModel(string displayName, TValue value, int id, string groupName, bool enableMultiSelect,
            int sortValue)
            : base(displayName, id, groupName, enableMultiSelect, sortValue)
        {
            Value = value;
        }

        /// <summary>
        ///     Returns the user-friendly name of this option.
        /// </summary>
        public TValue Value { get; }

        public int CompareTo(OptionModel<TValue> other)
        {
            if (other == null)
                return -1;

            if (SortValue == UNSET_SORT_VALUE && other.SortValue == UNSET_SORT_VALUE)
            {
                return DisplayName.CompareTo(other.DisplayName);
            }
            if (SortValue != UNSET_SORT_VALUE && other.SortValue != UNSET_SORT_VALUE)
            {
                return SortValue.CompareTo(other.SortValue);
            }
            if (SortValue != UNSET_SORT_VALUE && other.SortValue == UNSET_SORT_VALUE)
            {
                return -1;
            }
            return +1;
        }

        /// <summary>
        /// Creates a Dictionary of options.
        /// If no options (params TValue[] values) are specified, it will use all of the
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
                allValues = isStringEnum ? new TValue[0] : EnumHelper.GetValues<TValue>();
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
}