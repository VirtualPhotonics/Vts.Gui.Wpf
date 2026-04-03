using System;
using System.Linq;
using Vts.Gui.Wpf.Model;

namespace Vts.Gui.Wpf.ViewModel.Helpers;

/// <summary>
///     An internal class that separates out the providing of sample (test) data.
/// </summary>
/// <remarks>
///     Helps to separate desired behavior of above class from any data-specific concerns.
/// </remarks>
internal static class SampleBitmapDataProvider
{
    private static double _xPhase;
    private static double _yPhase = Math.PI;

    public static MapData GetSampleBitmapData()
    {
        var tempData = new double[600, 600];

        _xPhase -= 21 / 180.0 * Math.PI;
        _yPhase += 7 / 180.0 * Math.PI;

        var width = tempData.GetLength(0);
        var height = tempData.GetLength(1);
        for (var col = 0; col < height; col++)
        {
            for (var row = 0; row < width; row++)
            {
                var x = .01 * col;
                var y = .02 * row;

                tempData[row, col] =
                    (Math.Sin(Math.Pow(_yPhase / Math.PI * x, 2) - Math.Pow(_xPhase / Math.PI * y, 2)) + _xPhase +
                     _yPhase) *
                    Math.Cos(x + _xPhase) * Math.Sin(y + _yPhase);
            }
        }

        return MapData.Create(tempData,
            [.. Enumerable.Range(0, width).Select(i => (double)i)],
            [.. Enumerable.Range(0, height).Select(i => (double)i)],
            [.. Enumerable.Range(0, width).Select(_ => 1D)],
            [.. Enumerable.Range(0, height).Select(_ => 1D)]);
    }
}