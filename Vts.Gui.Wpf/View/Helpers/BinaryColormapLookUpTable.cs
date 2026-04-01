namespace Vts.Gui.Wpf.View.Helpers;

internal class BinaryColormapLookUpTable : ColormapLookUpTable
{
    internal BinaryColormapLookUpTable()
        : base(R, G, B)
    {
    }

    #region Colormap Definitions

    private static readonly double[] R =
    [
        0,
        1
    ];

    private static readonly double[] G =
    [
        0,
        1
    ];

    private static readonly double[] B =
    [
        0,
        1
    ];

    #endregion
}