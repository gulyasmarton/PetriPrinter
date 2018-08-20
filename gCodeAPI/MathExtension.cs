using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCodeAPI
{
    /// <summary>
    /// This is an additional class which contains few extra math functions.
    /// </summary>
    public static class MathExtension
    {
        /// <summary>
        /// Converts an angle measured in degrees to an approximately equivalent angle measured in radians.
        /// </summary>
        /// <param name="angle"> an angle, in degrees</param>
        /// <returns></returns>
        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// Converts an angle measured in radians to an approximately equivalent angle measured in degrees. 
        /// </summary>
        /// <param name="angle"> an angle, in radian</param>
        /// <returns></returns>
        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        /// <summary>
        /// Returns the coTan of an angle in radius.
        /// </summary>
        /// <param name="x">an angle, in radian</param>
        /// <returns></returns>
        public static double Cotan(double x)
        {
            return 1 / Math.Tan(x);
        }
    }
}
