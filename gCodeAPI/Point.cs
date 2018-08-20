using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeAPI
{
    /// <summary>
    /// This is an additional class which represents a Point.
    /// </summary>
    [Serializable]
    public class Point
    {
        /// <summary>
        /// Sets or gets the X position.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Sets or gets the Y position.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Constructs a new Point object with the specified points.
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <returns></returns>
        public static double Distance(Point p1, Point p2)
        {
            return Distance(p1.X, p1.Y, p2.X, p2.Y);
        }

        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        /// <param name="x1">X position of first point.</param>
        /// <param name="y1">Y position of first point.</param>
        /// <param name="x2">X position of second point.</param>
        /// <param name="y2">Y position of second point.</param>
        /// <returns></returns>
        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        /// <summary>
        /// Converts a point from Polar coordinate system to Cartesian coordinate system
        /// </summary>        
        /// <param name="r">distance</param>
        /// <param name="deg">angle in degree</param>
        /// <param name="xOffset">x offset</param>
        /// <param name="yOffset">y offest</param>
        /// <returns></returns>
        public static Point PolarToCartesian(double r, double deg, double xOffset = 0, double yOffset = 0)
        {
            double rad = MathExtension.DegreeToRadian(deg);

            double yk = r * Math.Sin(rad);
            double xk = r * Math.Cos(rad);

            return new Point(xk + xOffset, yk + yOffset);
        }

        /// <summary>
        /// Converts a point from Cartesian coordinate system to Polar coordinate system
        /// </summary>
        /// <param name="x">Cartesian X coordinate</param>
        /// <param name="y">Cartesian Y coordinate</param>
        /// <param name="xOffset">x offset</param>
        /// <param name="yOffset">y offset</param>
        /// <returns>double[2] array where first is radius and second angle in degree</returns>
        public static double[] CartesianToPolar(double x, double y, double xOffset = 0, double yOffset = 0)
        {
            x = x - xOffset;
            y = y - yOffset;

            double r = Math.Sqrt(x * x + y * y);

            if (x == 0)
            {
                return new double[] { r, (y >= 0 ? 0 : 180) };
            }

            double deg = MathExtension.RadianToDegree(Math.Atan(y / x));

            if (x < 0 && y > 0)
                deg += 180;
            else if (x < 0 && y < 0)
                deg += 180;
            else if (x > 0 && y < 0)
                deg += 360;

            return new double[] { r, deg };
        }

        public override string ToString()
        {
            return Math.Round(X * 100) / 100 + " " + Math.Round(Y * 100) / 100;
        }
    }
}
