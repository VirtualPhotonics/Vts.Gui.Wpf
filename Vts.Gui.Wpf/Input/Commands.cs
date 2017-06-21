using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Vts;
using Vts.Gui.Wpf.ViewModel;

namespace Vts.Gui.Wpf.Input
{
    public static class Commands
    {
        static Commands()
        {
            SetWavelength = new RoutedCommand();
            UpdateWavelength = new RelayCommand<double>(UpdateWavelength_Executed);
            UpdateOpticalProperties = new RoutedCommand();
        }

        //Spectra view commands
        public static RoutedCommand SetWavelength { get; private set; }
        public static RelayCommand<double> UpdateWavelength { get; private set; }
        public static RoutedCommand UpdateOpticalProperties { get; private set; }

        private static void UpdateWavelength_Executed(double wavelength)
        {
        }
    }
}
