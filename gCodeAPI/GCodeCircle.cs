using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeAPI
{
    /// <summary>
    /// This class wraps the G2 and G3 g-codes which provide the printer track of circle.
    /// </summary>
    [Serializable]
    public class GCodeCircle : IGCode
    {
        /// <summary>
        /// Returns the name of IGCode
        /// </summary>
        public string Name { get { return "Circle"; } }
        double? x, y, f, e;
        double i, j;
        bool isClockwise;

        /// <summary>
        /// Returns this circle's bounding rectangle.
        /// </summary>
        public Rect Bound
        {
            get
            {
                double dist = Math.Sqrt(i * i + j * j);
                double minX = 0 + i - dist;
                double minY = 0 + j - dist;

                double maxX = 0 + i + dist;
                double maxY = 0 + j + dist;

                return new Rect(minX, minY, maxX, maxY);
            }
        }

        /// <summary>
        /// Default constructor (http://reprap.org/wiki/G-code#G2_.26_G3:_Controlled_Arc_Move)
        /// </summary>
        /// <param name="f">The feed-rate per minute of the move between the starting point and ending point</param>
        /// <param name="x">The position to move to on the X axis</param>
        /// <param name="y">The position to move to on the Y axis</param>
        /// <param name="i">The point in X space from the current X position to maintain a constant distance from</param>
        /// <param name="j">The point in Y space from the current Y position to maintain a constant distance from</param>
        /// <param name="e">The amount to extrude between the starting point and ending point</param>
        /// <param name="isClockwise">direction (Clockwise G2, Counter-Clockwise G3)</param>
        public GCodeCircle(double i, double j, double? x = null, double? y = null, double? f = null, double? e = null, bool isClockwise = true)
        {
            this.e = e;
            this.f = f;
            this.x = x;
            this.y = y;
            this.i = i;
            this.j = j;
            this.isClockwise = isClockwise;
        }

        /// <summary>
        /// Returns a printer track of specified circle. The circle path is defined by using a line.
        /// The start point of the line is the current position of printer head (use the GCodeLine class to define it).
        /// The line is defined by angle with horizontal axis and length of line (2x radius). 
        /// </summary>
        /// <param name="axisAngle">angle with horizontal axis</param>
        /// <param name="radius">radius of circle</param>
        /// <param name="isClockwise">direction (Clockwise G2, Counter-Clockwise G3)</param>
        /// <param name="speed">The feed-rate per minute of the move between the starting point and ending point</param>
        /// <param name="extrude">The amount to extrude between the starting point and ending point</param>
        /// <returns></returns>
        public static GCodeCircle getCircleByRadius(double axisAngle, double radius, bool isClockwise = true, double? speed = null, double? extrude = null)
        {

            if (axisAngle < 0)
            {
                axisAngle -= (Math.Floor(axisAngle / 360d) + 1) * 360d;
                axisAngle = 360 + axisAngle;
            }
            else if (axisAngle > 360)
                axisAngle -= Math.Floor(axisAngle / 360) * 360;
            double degree = (Math.PI / 180) * axisAngle;

            double x = Math.Cos(degree) * radius;
            double y = Math.Sin(degree) * radius;

            return new GCodeCircle(x, y, null, null, speed, extrude, isClockwise);
        }

        /// <summary>
        /// Returns the g-code string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string s;
            if (isClockwise)
                s = "G2";
            else
                s = "G3";

            if (f != null)
                s += " F" + f.Value.ToString("F6", CultureInfo.InvariantCulture);

            if (x != null)
                s += " X" + x.Value.ToString("F6", CultureInfo.InvariantCulture);
            if (y != null)
                s += " Y" + x.Value.ToString("F6", CultureInfo.InvariantCulture);

            s += " I" + i.ToString("F6", CultureInfo.InvariantCulture);
            s += " J" + j.ToString("F6", CultureInfo.InvariantCulture);

            if (e != null)
                s += " E" + e.Value.ToString("F6", CultureInfo.InvariantCulture);

            return s;
        }

        /// <summary>
        /// Returns a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public IGCode Duplicate()
        {
            return new GCodeCircle(i, j, x, y, f, e, isClockwise);
        }

        /// <summary>
        /// Add a curve to the input GCodeCollector;
        /// </summary>
        /// <param name="code">input GCodeCollector</param>
        /// <param name="centerX">center X of curve</param>
        /// <param name="centerY">center y of curve</param>
        /// <param name="radius">radius of curve</param>
        /// <param name="startDegree">start degree of curve</param>
        /// <param name="stopDegree">start degree of curve</param>
        /// <param name="speed">The feed-rate per minute of the move between the starting point and ending point</param>
        /// <param name="needExtrude">set true for extruding</param>
        public static void Arc(GCodeCollector code, double centerX, double centerY, double radius, double startDegree, double stopDegree, double? speed = null, bool needExtrude = false, bool isClockwise = true)
        {
            
            double dist = 0;
            if (isClockwise)
            {
                if (stopDegree < startDegree)
                {
                    dist = startDegree-stopDegree;
                }
                else
                {
                    dist = 360- stopDegree + startDegree;
                }
            }
            else
            {
                if (stopDegree < startDegree)
                {
                    dist = 360-startDegree + stopDegree;
                }
                else
                {
                    dist = stopDegree - startDegree;
                }
            }


            //double dist = Math.Abs(stopDegree - startDegree);

            double sign = isClockwise ? -1 : 1;



            var p = Point.PolarToCartesian(radius, startDegree, centerX, centerY);

            code.addCode(new GCodeLine(x: p.X, y: p.Y, speed: speed, extrude: needExtrude ? GCodeSpecial.ExtrudeLine(p, code.LastPoint) : (double?)null));

            for (int i = 1; i < dist; i += 5)
            {
                p = Point.PolarToCartesian(radius, startDegree + sign * i, centerX, centerY);
                code.addCode(new GCodeLine(x: p.X, y: p.Y, speed: speed, extrude: needExtrude ? GCodeSpecial.ExtrudeLine(p, code.LastPoint) : (double?)null));
            }
            p = Point.PolarToCartesian(radius, stopDegree, centerX, centerY);
            code.addCode(new GCodeLine(x: p.X, y: p.Y, speed: speed, extrude: needExtrude ? GCodeSpecial.ExtrudeLine(p, code.LastPoint) : (double?)null));

        }
        public static void ArcByPoints(GCodeCollector code, double startX, double startY, double stopX, double stopY, double radius, double? speed = null, bool needExtrude = false, bool isClockwise = true)
        {
        }
    }
}
