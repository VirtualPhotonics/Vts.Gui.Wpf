﻿namespace Vts.Gui.Wpf.ViewModel
{
    public class IndependentAxisViewModel : BindableObject
    {
        private string _axisLabel;
        private RangeViewModel _axisRangeVM;
        private IndependentVariableAxis _axisType;
        private string _axisUnits;

        public IndependentVariableAxis AxisType
        {
            get { return _axisType; }
            set
            {
                _axisType = value;
                OnPropertyChanged("AxisType");
            }
        }

        public string AxisLabel
        {
            get { return _axisLabel; }
            set
            {
                _axisLabel = value;
                OnPropertyChanged("AxisLabel");
            }
        }

        public string AxisUnits
        {
            get { return _axisUnits; }
            set
            {
                _axisUnits = value;
                OnPropertyChanged("AxisUnits");
            }
        }

        public RangeViewModel AxisRangeVM
        {
            get { return _axisRangeVM; }
            set
            {
                _axisRangeVM = value;
                OnPropertyChanged("AxisRangeVM");
            }
        }
    }
}