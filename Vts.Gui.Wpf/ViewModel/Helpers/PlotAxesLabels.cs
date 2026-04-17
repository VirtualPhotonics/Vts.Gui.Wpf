using Vts.Gui.Wpf.ViewModel.Panels.SubPanels;

namespace Vts.Gui.Wpf.ViewModel.Helpers;

/// <summary>
///     Data structure to hold data for plot information
/// </summary>
public class PlotAxesLabels(
    string dependentAxisName, string dependentAxisUnits,
    IndependentAxisViewModel independentAxis,
    ConstantAxisViewModel[] constantAxes = null)
{
    public string DependentAxisUnits { get; set; } = dependentAxisUnits;
    public string DependentAxisName { get; set; } = dependentAxisName;
    public IndependentAxisViewModel IndependentAxis { get; set; } = independentAxis;
    public ConstantAxisViewModel[] ConstantAxes { get; set; } = constantAxes ?? [];
    public bool IsComplexPlot { get; set; }
}