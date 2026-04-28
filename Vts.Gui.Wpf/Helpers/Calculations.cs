using System;

namespace Vts.Gui.Wpf.Helpers;

internal static class Calculations
{

    /// <summary>
    /// Compares two doubles using a small tolerance to avoid exact equality checks.
    /// </summary>
    internal static bool AreApproximatelyEqual(double a, double b, double tolerance = 1e-9)
    {
        return Math.Abs(a - b) <= tolerance;
    }

}