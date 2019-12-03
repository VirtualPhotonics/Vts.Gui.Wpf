using System;
using System.Collections.Generic;

namespace Vts.Gui.Wpf.Model
{
    public sealed class DoubleDataPoint : IDataPoint, IEquatable<DoubleDataPoint>, IEqualityComparer<DoubleDataPoint>
    {
        /// <summary>
        /// The constructor for the DoubleDataPoint that initializes a DoubleDataPoint object
        /// with the specified x and y values.
        /// </summary>
        /// <param name="x">The x-coordinate value of the DoubleDataPoint</param>
        /// <param name="y">The y-coordinate value of the DoubleDataPoint</param>
        public DoubleDataPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// The X-coordinate value of this DoubleDataPoint
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The Y-coordinate value of this DoubleDataPoint
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Determines whether the specified object is a DoubleDataPoint and whether
        /// it contains the same values as this DoubleDataPoint.
        /// </summary>
        /// <param name="obj">The object to compare for equality</param>
        /// <returns>
        /// Returns true if obj is a DoubleDataPoint and contains the same DoubleDataPoint.X
        /// and DoubleDataPoint.Y values as this DoubleDataPoint; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals((DoubleDataPoint) obj);
        }

        /// <summary>
        /// Compares two DoubleDataPoint structures for equality.
        /// </summary>
        /// <param name="other">The DoubleDataPoint to compare to this instance.</param>
        /// <returns>
        /// Returns true if both DoubleDataPoint structures contain the same DoubleDataPoint.X
        /// and DoubleDataPoint.Y values; otherwise, false.
        /// </returns>
        public bool Equals(DoubleDataPoint other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Compares two DoubleDataPoint structures for equality.
        /// </summary>
        /// <param name="x">The first DoubleDataPoint to compare equality.</param>
        /// <param name="y">The second DoubleDataPoint to compare equality.</param>
        /// <returns>
        /// Returns true if both DoubleDataPoint structures contain the same DoubleDataPoint.X
        /// and DoubleDataPoint.Y values; otherwise, false.
        /// </returns>
        public bool Equals(DoubleDataPoint x, DoubleDataPoint y)
        {
            return x.X == y.X && x.Y == y.Y;
        }

        /// <summary>
        /// Creates a string representation of this DoubleDataPoint
        /// </summary>
        /// <returns>
        /// Returns a string containing the DoubleDataPoint.X and DoubleDataPoint.Y
        /// values of this DoubleDataPoint object.
        /// </returns>
        public override string ToString()
        {
            return X + ", " + Y;
        }

        /// <summary>
        /// Returns the hash code for this DoubleDataPoint
        /// </summary>
        /// <returns>Returns the hash code for this DoubleDataPoint</returns>
        public override int GetHashCode()
        {
            var hashCode = 76543890;
            hashCode = hashCode * -5678 + X.GetHashCode();
            hashCode = hashCode * -5678 + Y.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Returns the hash code for this DoubleDataPoint
        /// </summary>
        /// <param name="obj">The DoubleDataPoint</param>
        /// <returns>Returns the hash code for this DoubleDataPoint</returns>
        public int GetHashCode(DoubleDataPoint obj)
        {
            var hashCode = 76543890;
            hashCode = hashCode * -5678 + obj.X.GetHashCode();
            hashCode = hashCode * -5678 + obj.Y.GetHashCode();
            return hashCode;
        }
    }
}