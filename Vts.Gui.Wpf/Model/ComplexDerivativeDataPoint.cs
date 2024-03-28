using System;
using System.Collections.Generic;
using System.Numerics;

namespace Vts.Gui.Wpf.Model
{
    public sealed class ComplexDerivativeDataPoint : IDataPoint, IEquatable<ComplexDerivativeDataPoint>, IEqualityComparer<ComplexDerivativeDataPoint>
    {
        private readonly int _hashCode;

        /// <summary>
        /// Constructor for the ComplexDerivativeDataPoint
        /// </summary>
        /// <param name="x">A double representing the x value</param>
        /// <param name="y">A Complex number representing the y value (real, imag)</param>
        /// <param name="dy">A Complex number representing the derivative of the y value (dreal, dimag)</param>
        /// <param name="derivativeVariable">derivative taken with respect to this variable</param>
        public ComplexDerivativeDataPoint(double x, Complex y, Complex dy, ForwardAnalysisType derivativeVariable)
        {
            X = x;
            Y = y;
            Dy = dy;
            DerivativeVariable = derivativeVariable;
            _hashCode = new { x, y, dy, derivativeVariable }.GetHashCode();
        }

        /// <summary>
        /// The X-coordinate value of this ComplexDerivativeDataPoint structure.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The Y-coordinate value of this ComplexDerivativeDataPoint structure.
        /// </summary>
        public Complex Y { get; set; }

        /// <summary>
        /// The Y-coordinate value of the derivative of this ComplexDerivativeDataPoint structure.
        /// </summary>
        public Complex Dy { get; set; }

        /// <summary>
        /// The variable derivative is taken with respect to
        /// </summary>
        public ForwardAnalysisType DerivativeVariable { get; set; }

        public double PhaseDerivative =>
            1 / (1 + Y.Imaginary / Y.Real * (Y.Imaginary / Y.Real)) *
            (1 / Y.Real * Dy.Imaginary - Y.Imaginary / (Y.Real * Y.Real) * Dy.Real);

        public double AmplitudeDerivative =>
            1 / (2 * Math.Sqrt(Y.Real * Y.Real + Y.Imaginary * Y.Imaginary)) * 
            (2 * Y.Real * Dy.Real + 2 * Y.Imaginary * Dy.Imaginary);


        /// <summary>
        /// Determines whether the specified object is a ComplexDerivativeDataPoint and whether
        /// it contains the same values as this ComplexDerivativeDataPoint.
        /// </summary>
        /// <param name="obj">An object to check for equality</param>
        /// <returns>
        /// Returns true if obj is a ComplexDerivativeDataPoint and contains the same ComplexDerivativeDataPoint.X
        /// and ComplexDerivativeDataPoint.Y values as this ComplexDerivativeDataPoint; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ComplexDerivativeDataPoint);
        }

        /// <summary>
        /// Determines whether the specified ComplexDerivativeDataPoint contains the same
        /// values as this ComplexDerivativeDataPoint.
        /// </summary>
        /// <param name="point">An ComplexDerivativeDataPoint to check for equality</param>
        /// <returns>
        /// Returns true if the ComplexDerivativeDataPoint contains the same ComplexDerivativeDataPoint.X
        /// and ComplexDerivativeDataPoint.Y values as this ComplexDerivativeDataPoint; otherwise, false.
        /// </returns>
        public bool Equals(ComplexDerivativeDataPoint point)
        {
            const double tolerance = 1e-6; // small value

            return Math.Abs(X - point!.X) < tolerance 
                   && Math.Abs(Y.Real - point.Y.Real) < tolerance 
                   && Math.Abs(Y.Imaginary - point.Y.Imaginary) < tolerance
                    && Math.Abs(Dy.Real - point.Dy.Real) < tolerance
                    && Math.Abs(Dy.Imaginary - point.Dy.Imaginary) < tolerance
                    && DerivativeVariable == point.DerivativeVariable;
        }

        /// <summary>
        /// Compares two ComplexDerivativeDataPoint structures for equality.
        /// </summary>
        /// <param name="point">The first ComplexDerivativeDataPoint to compare equality.</param>
        /// <param name="comparePoint">The second ComplexDerivativeDataPoint to compare equality.</param>
        /// <returns>
        /// Returns true if both ComplexDerivativeDataPoint structures contain the same ComplexDerivativeDataPoint.X
        /// and ComplexDerivativeDataPoint.Y values; otherwise, false.
        /// </returns>
        public bool Equals(ComplexDerivativeDataPoint point, ComplexDerivativeDataPoint comparePoint)
        {
            const double tolerance = 1e-6; // small value

            return Math.Abs(point!.X - comparePoint!.X) < tolerance 
                   && Math.Abs(point.Y.Real - comparePoint.Y.Real) < tolerance 
                   && Math.Abs(point.Y.Imaginary - comparePoint.Y.Imaginary) < tolerance
                   && Math.Abs(point.Dy.Real - comparePoint.Dy.Real) < tolerance
                   && Math.Abs(point.Dy.Imaginary - comparePoint.Dy.Imaginary) < tolerance
                   && point.DerivativeVariable == comparePoint.DerivativeVariable;
        }

        /// <summary>
        /// Creates a string representation of this ComplexDerivativeDataPoint.
        /// </summary>
        /// <returns>
        /// A string containing the ComplexDerivativeDataPoint.X and ComplexDerivativeDataPoint.Y
        /// values of this ComplexDerivativeDataPoint structure.
        /// </returns>
        public override string ToString()
        {
            return X + ", " + Y;
        }

        /// <summary>
        /// Returns the hash code for this ComplexDerivativeDataPoint.
        /// </summary>
        /// <returns>Returns the hash code for this ComplexDerivativeDataPoint</returns>
        public override int GetHashCode() => _hashCode;

        /// <summary>
        /// Returns the hash code for this ComplexDerivativeDataPoint.
        /// </summary>
        /// <param name="obj">The ComplexDerivativeDataPoint</param>
        /// <returns>Returns the hash code for the parameter ComplexDerivativeDataPoint</returns>
        public int GetHashCode(ComplexDerivativeDataPoint obj)
        {
            var hashCode = new { obj.X, obj.Y, obj.Dy, obj.DerivativeVariable }.GetHashCode();
            return hashCode;
        }
    }
}