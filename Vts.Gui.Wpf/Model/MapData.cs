using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Vts.Common.Math;
using Vts.Extensions;
using Vts.Gui.Wpf.Extensions;

[assembly: InternalsVisibleTo("Vts.Gui.Wpf.Test")]

namespace Vts.Gui.Wpf.Model;

/// <summary>
///     Model object to represent 2D data for plotting
/// </summary>
/// <param name="rawData">1D row-major array of 2D data</param>
/// <param name="xValues">The 1D horizontal axis independent values</param>
/// <param name="yValues">The 1D vertical axis independent values</param>
/// <param name="dxValues">
///     Values representing the area or length of each horizontal independent value, for calculation of
///     expectation
/// </param>
/// <param name="dyValues">
///     Values representing the area or length of each vertical independent value, for calculation of
///     expectation
/// </param>
/// <remarks>Example of dx, dy values for curvilinear coordinates: dx=(2*Pi*rho*drho), dy=dz</remarks>
public class MapData(double[] rawData, double[] xValues, double[] yValues, double[] dxValues, double[] dyValues) : BindableObject
{
    public int Width => XValues.Length;

    public int Height => YValues.Length;

    public double[] RawData { get; } = rawData;
    public double[] XValues { get; } = xValues;
    public double[] YValues { get; } = yValues;
    public double[] DxValues { get; } = dxValues;
    public double[] DyValues { get; } = dyValues;
    public double Min { get; private set; } = rawData.Min();
    public double Max { get; private set; } = rawData.Max();

    public double YExpectationValue => Statistics.MeanSamplingDepth(RawData, XValues, YValues, DxValues, DyValues);

    internal static MapData Create(double[,] rawData, double[] x, double[] y, double[] dx, double[] dy)
    {
        if (rawData.GetLength(0) != x.Length || rawData.GetLength(1) != y.Length)
            throw new ArgumentException(StringLookup.GetLocalizedString("Exception_MismatchedArrays"));

        return new MapData(rawData.ToEnumerable<double>().ToArray(), x, y, dx, dy);
    }

    public static MapData Create(double[] rawData, double[] x, double[] y, double[] dx, double[] dy)
    {
        return rawData.Length != x.Length*y.Length ? throw new ArgumentException(StringLookup.GetLocalizedString("Exception_MismatchedArrays")) : new MapData(rawData.ToArray(), x, y, dx, dy);
    }
}