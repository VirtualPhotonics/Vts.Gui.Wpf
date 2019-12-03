using System;
using System.Collections.Generic;
using System.Numerics;

namespace Vts.Gui.Wpf.Model
{
    public sealed class ComplexDataPoint : IDataPoint, IEquatable<ComplexDataPoint>, IEqualityComparer<ComplexDataPoint>
    {
        /// <summary>
        /// Constructor for the ComplexDataPoint
        /// </summary>
        /// <param name="x">A double representing the x value</param>
        /// <param name="y">A Complex number representing the y value</param>
        public ComplexDataPoint(double x, Complex y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// The X-coordinate value of this ComplexDataPoint structure.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The SY-coordinate value of this ComplexDataPoint structure.
        /// </summary>
        public Complex Y { get; set; }

        /// <summary>
        /// Determines whether the specified object is a ComplexDataPoint and whether
        /// it contains the same values as this ComplexDataPoint.
        /// </summary>
        /// <param name="obj">An object to check for equality</param>
        /// <returns>
        /// Returns true if obj is a ComplexDataPoint and contains the same ComplexDataPoint.X
        /// and ComplexDataPoint.Y values as this ComplexDataPoint; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ComplexDataPoint);
        }

        /// <summary>
        /// Determines whether the specified ComplexDataPoint contains the same
        /// values as this ComplexDataPoint.
        /// </summary>
        /// <param name="value">An ComplexDataPoint to check for equality</param>
        /// <returns>
        /// Returns true if the ComplexDataPoint contains the same ComplexDataPoint.X
        /// and ComplexDataPoint.Y values as this ComplexDataPoint; otherwise, false.
        /// </returns>
        public bool Equals(ComplexDataPoint value)
        {
            return X == value.X && Y == value.Y;
        }

        /// <summary>
        /// Compares two ComplexDataPoint structures for equality.
        /// </summary>
        /// <param name="x">The first ComplexDataPoint to compare equality.</param>
        /// <param name="y">The second ComplexDataPoint to compare equality.</param>
        /// <returns>
        /// Returns true if both ComplexDataPoint structures contain the same ComplexDataPoint.X
        /// and ComplexDataPoint.Y values; otherwise, false.
        /// </returns>
        public bool Equals(ComplexDataPoint x, ComplexDataPoint y)
        {
            return x.X == y.X && x.Y == y.Y;
        }

        /// <summary>
        /// Creates a string representation of this ComplexDataPoint.
        /// </summary>
        /// <returns>
        /// A string containing the ComplexDataPoint.X and ComplexDataPoint.Y
        /// values of this ComplexDataPoint structure.
        /// </returns>
        public override string ToString()
        {
            return X + ", " + Y;
        }

        /// <summary>
        /// Returns the hash code for this ComplexDataPoint.
        /// </summary>
        /// <returns>Returns the hash code for this ComplexDataPoint</returns>
        public override int GetHashCode()
        {
            var hashCode = 76543890;
            hashCode = hashCode * -5678 + Y.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Returns the hash code for this ComplexDataPoint.
        /// </summary>
        /// <param name="obj">The ComplexDataPoint</param>
        /// <returns>Returns the hash code for the parameter ComplexDataPoint</returns>
        public int GetHashCode(ComplexDataPoint obj)
        {
            var hashCode = 76543890;
            hashCode = hashCode * -5678 + obj.Y.GetHashCode();
            return hashCode;
        }
    }
}