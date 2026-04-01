namespace Vts.Gui.Wpf.View.Helpers;

internal abstract class ColormapLookUpTable(double[] r, double[] g, double[] b)
{

    #region Properties

    public double[] Red { get; } = r;

    public double[] Green { get; } = g;

    public double[] Blue { get; } = b;

    public int Length => Red?.Length ?? 0;

    #endregion
}