using System.Collections.Generic;
using Vts.Common;

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary>
    ///     View model exposing the DoubleRange model class with change notification
    /// </summary>
    public class RangeViewModel : BindableObject
    {
        private IndependentVariableAxis _axisType;
        private bool _enableNumber;
        // Range model - backing store for public properties
        private readonly DoubleRange _range;
        private string _title;

        private string _units;

        public RangeViewModel(DoubleRange range, string units, IndependentVariableAxis axisType, string title,
            bool enableNumber)
        {
            _range = range;
            Units = units;
            Title = title;
            _enableNumber = enableNumber;
            _axisType = axisType;

            _range.PropertyChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(Stop));
                OnPropertyChanged(nameof(Number));
            };
        }

        public RangeViewModel(DoubleRange range, string units, IndependentVariableAxis axisType, string title)
            : this(range, units, axisType, title, true)
        {
        }

        public RangeViewModel() : this(new DoubleRange(1.0, 6.0, 60), "mm", IndependentVariableAxis.Rho, "Range:", true)
        {
        }

        /// <summary>
        ///     A double representing the start of the range
        /// </summary>
        public double Start
        {
            get => _range.Start;
            set
            {
                _range.Start = value;
                OnPropertyChanged(nameof(Start));
            }
        }

        /// <summary>
        ///     A double representing the end of the range
        /// </summary>
        public double Stop
        {
            get => _range.Stop;
            set
            {
                _range.Stop = value;
                OnPropertyChanged(nameof(Stop));
            }
        }

        /// <summary>
        ///     An integer representing the number of values in the range
        /// </summary>
        public int Number
        {
            get => _range.Count;
            set
            {
                _range.Count = value;
                OnPropertyChanged(nameof(Number));
            }
        }

        public string Units
        {
            get => _units;
            set
            {
                _units = value;
                OnPropertyChanged(nameof(Units));
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(ShowTitle));
            }
        }

        public bool EnableNumber
        {
            get => _enableNumber;
            set
            {
                _enableNumber = value;
                OnPropertyChanged(nameof(EnableNumber));
            }
        }

        public bool ShowTitle => Title.Length > 0;

        public IndependentVariableAxis AxisType
        {
            get => _axisType;
            set
            {
                _axisType = value;
                OnPropertyChanged(nameof(AxisType));
            }
        }

        public IEnumerable<double> Values => _range;
    }
}