using System.Reflection;
using Vts.Gui.Wpf.ViewModel.Controls;
using Vts.Gui.Wpf.ViewModel.Panels;
using Vts.Gui.Wpf.ViewModel.Panels.MonteCarlo;

namespace Vts.Gui.Wpf.ViewModel;

/// <summary>
///     View model implementing highest-level panel functionality for the general-purpose ATK
/// </summary>
public class WindowViewModel : BindableObject
{
    public WindowViewModel()
    {
        Current = this;

        ForwardSolverVm = new ForwardSolverViewModel();
        InverseSolverVm = new InverseSolverViewModel();
        FluenceSolverVm = new FluenceSolverViewModel();
        MonteCarloSolverVm = new MonteCarloSolverViewModel();
        SpectralMappingVm = new SpectralMappingViewModel();
        PlotVm = new PlotViewModel();
        MapVm = new MapViewModel();
        TextOutputVm = new TextOutputViewModel();
    }

    public static WindowViewModel Current { get; set; }

    public ForwardSolverViewModel ForwardSolverVm { get; private set; }
    public InverseSolverViewModel InverseSolverVm { get; private set; }
    public FluenceSolverViewModel FluenceSolverVm { get; private set; }
    public MonteCarloSolverViewModel MonteCarloSolverVm { get; private set; }
    public SpectralMappingViewModel SpectralMappingVm { get; private set; }
    public PlotViewModel PlotVm { get; private set; }
    public MapViewModel MapVm { get; private set; }
    public TextOutputViewModel TextOutputVm { get; private set; }

    public static string Version
    {
        get
        {
            var currentVersion = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
            return currentVersion.Version.Major + "." + currentVersion.Version.Minor + "." +
                   currentVersion.Version.Build;
        }
    }
}