using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCodeAPI
{
    /// <summary>
    /// This is an additional class which represents a rectangle. It is defined by two points.
    /// </summary>
    public class Rect
    {
        /// <summary>
        /// Sets or gets the X position of the first point.
        /// </summary>
        public double X1 { get; set; }

        /// <summary>
        /// Sets or gets the Y position of the first point.
        /// </summary>
        public double Y1 { get; set; }

        /// <summary>
        /// Sets or gets the X position of the second point.
        /// </summary>
        public double X2 { get; set; }

        /// <summary>
        /// Sets or gets the Y position of the second point.
        /// </summary>
        public double Y2 { get; set; }

        /// <summary>
        /// Constructs a new Rect object by using two point.
        /// </summary>
        /// <param name="x1">Sets or gets the X position of the first point.</param>
        /// <param name="y1">Sets or gets the Y position of the first point.</param>
        /// <param name="x2">Sets or gets the X position of the second point.</param>
        /// <param name="y2">Sets or gets the Y position of the second point.</param>
        public Rect(double x1, double y1, double x2, double y2)
        {
            X1 = Math.Min(x1, x2);
            Y1 = Math.Min(y1, y2);
            X2 = Math.Max(x1, x2);
            Y2 = Math.Max(y1, y2);
        }

        /// <summary>
        /// Returns the center of the rectangle.
        /// </summary>
        public Point Center
        {
            get
            {
                return new Point((X1 + X2) / 2, (Y1 + Y2) / 2);
            }
        }

        /// <summary>
        /// Returns the minimum X value.
        /// </summary>
        public double MinX
        {
            get
            {
                return Math.Min(X1, X2);
            }

        }

        /// <summary>
        /// Returns the minimum Y value.
        /// </summary>
        public double MinY
        {
            get
            {
                return Math.Min(Y1, Y2);
            }

        }

        /// <summary>
        /// Returns the maximum X value.
        /// </summary>
        public double MaxX
        {
            get
            {
                return Math.Max(X1, X2);
            }

        }

        /// <summary>
        /// Returns the maximum Y value.
        /// </summary>
        public double MaxY
        {
            get
            {
                return Math.Max(Y1, Y2);
            }

        }

        public double Width { get { return Math.Abs(X1 - X2); } }
        public double Height { get { return Math.Abs(Y1 - Y2); } }
    }
}
