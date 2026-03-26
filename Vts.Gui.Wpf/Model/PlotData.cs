using System.Linq;
using System.Windows;

namespace Vts.Gui.Wpf.Model;

/// <summary>
///     Class to hold all data necessary for describing a single line plot
///     Needs to be bolstered to allow for multiple descriptors, powerful enough
///     to support a view of a particular data set
/// </summary>
public class PlotData(IDataPoint[] points, string title)
{
    // todo: temp to get things working...want to eventually remove
    public PlotData(Point[] points, string title)
        : this(points.Select(point => new DoubleDataPoint(point.X, point.Y)).ToArray<IDataPoint>(), title)
    {
    }

    public IDataPoint[] Points { get; set; } = points;
    public string Title { get; set; } = title;
}