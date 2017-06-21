using System.Reflection;
using Vts;
using Vts.Gui.Wpf.Extensions;

namespace Vts.Gui.Wpf.ViewModel
{
    /// <summary>
    /// View model implementing highest-level panel functionality for the general-purpose ATK
    /// </summary>
    public class WindowViewModel : BindableObject
    {
        public WindowViewModel()
        {
            Current = this;

            ForwardSolverVM = new ForwardSolverViewModel();
            InverseSolverVM = new InverseSolverViewModel();
            FluenceSolverVM = new FluenceSolverViewModel();
            MonteCarloSolverVM = new MonteCarloSolverViewModel();
            SpectralMappingVM = new SpectralMappingViewModel();
            PlotVM = new PlotViewModel();
            MapVM = new MapViewModel();
            TextOutputVM = new TextOutputViewModel();
        }

        public static WindowViewModel Current { get; set; }

        public ForwardSolverViewModel ForwardSolverVM { get; private set; }
        public InverseSolverViewModel InverseSolverVM { get; private set; }
        public FluenceSolverViewModel FluenceSolverVM { get; private set; }
        public MonteCarloSolverViewModel MonteCarloSolverVM { get; private set; }
        public SpectralMappingViewModel SpectralMappingVM { get; private set; }
        public PlotViewModel PlotVM { get; private set; }
        public MapViewModel MapVM { get; private set; }
        public TextOutputViewModel TextOutputVM { get; private set; }
        public string Version
        {
            get
            {
                var currentVersion = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
                return currentVersion.Version.Major.ToString() + "." + currentVersion.Version.Minor.ToString() + "." + currentVersion.Version.Build.ToString();
                //              return currentVersion.Version.ToString(); // This line returns all 4 version numbers Major.Minor.Build.Revision
            }
        }
    }
}
