using Vts.Gui.Wpf.Input;

namespace Vts.Gui.Wpf.ViewModel
{
    public class ConstantAxisViewModel : BindableObject
    {
        private string _axisLabel;
        private IndependentVariableAxis _axisType;
        private string _axisUnits;
        private double _axisValue;
        private int _imageHeight = 50;

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

        public double AxisValue
        {
            get { return _axisValue; }
            set
            {
                _axisValue = value;
                if (AxisType == IndependentVariableAxis.Wavelength)
                {
                    // update the world that this has changed, and react to it if desired (e.g. in Spectral Panel)
                    Commands.SetWavelength.Execute(AxisValue, null);
                }
                OnPropertyChanged("AxisValue");
            }
        }

        public int ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                _imageHeight = value;
                OnPropertyChanged("ImageHeight");
            }
        }
    }
}