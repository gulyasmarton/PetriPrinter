using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCodeAPI
{
    [Serializable]
    public class GCodeEllipse : IGCode
    {
        public string Name { get { return "Ellipse"; } }

        /// <summary>
        /// Minimum length in mm
        /// </summary>
        public static double MinArc = 1d;

        Point center;

        double AxisA, AxisB, Angle;
        double? Speed;

        bool needExtrude;

        bool isClockwise;

        double startDeg, stopDeg;
        GCodeCollector code;
        public GCodeCollector Code { get { if (code == null) Generate(); return code; } }

        public GCodeEllipse(Point center, double axisA, double axisB, double startDeg, double stopDeg, bool isClockwise = true, double Angle = 0, double? Speed = null, bool needExtrude = false)
        {
            this.center = new Point(center.X, center.Y);
            AxisA = axisA;
            AxisB = axisB;
            Angle = MathExtension.DegreeToRadian(Angle);
            this.isClockwise = isClockwise;
            this.startDeg = startDeg;
            this.stopDeg = stopDeg;
            this.Speed = Speed;
            this.needExtrude = needExtrude;
            Generate();
        }

        private void Generate()
        {
            code = new GCodeCollector();
            double dist = 0;


            if (isClockwise)
            {
                if (stopDeg < startDeg)
                {
                    dist = startDeg - stopDeg;
                }
                else
                {
                    dist = 360 - (stopDeg - startDeg);
                }
            }
            else
            {
                if (stopDeg < startDeg)
                {
                    dist = 360 - startDeg + stopDeg;
                }
                else
                {
                    dist = stopDeg - startDeg;
                }
            }
            if (dist == 0) dist = 360;

            var startRad = MathExtension.DegreeToRadian(startDeg);
            var stopRad = MathExtension.DegreeToRadian(stopDeg);



            double sign = isClockwise ? -1 : 1;

            Point p = new Point(center.X + AxisA / 2d * Math.Cos(startRad), center.Y + AxisB / 2d * Math.Sin(startRad));

            Code.addCode(new GCodeLine(x: p.X, y: p.Y, speed: Speed, extrude: null));
            double a = AxisA / 2;
            double b = AxisB / 2;
            double c = Math.PI * (3 * (a + b) - Math.Sqrt((3 * a + b) * (a + 3 * b)));
            double degStep = Math.Min(360d / 8d, dist / (c * dist / 360 / MinArc));

            for (double i = degStep; i < dist; i += degStep)
            {

                p = new Point(center.X + AxisA / 2d * Math.Cos(MathExtension.DegreeToRadian(startDeg + i * sign)), center.Y + AxisB / 2d * Math.Sin(MathExtension.DegreeToRadian(startDeg + i * sign)));
                Code.addCode(new GCodeLine(x: p.X, y: p.Y, speed: Speed, extrude: needExtrude ? GCodeSpecial.ExtrudeLine(p, Code.LastPoint) : (double?)null));
            }

            p = new Point(center.X + AxisA / 2d * Math.Cos(stopRad), center.Y + AxisB / 2d * Math.Sin(stopRad));
            Code.addCode(new GCodeLine(x: p.X, y: p.Y, speed: Speed, extrude: needExtrude ? GCodeSpecial.ExtrudeLine(p, Code.LastPoint) : (double?)null));
        }

        public IGCode Duplicate()
        {
            var ellipse = new GCodeEllipse(center, AxisA, AxisB, startDeg, stopDeg, isClockwise, Angle, Speed, needExtrude);
            if (this.code != null)
                ellipse.code = this.code.Duplicate();
            return ellipse;
        }

        public override string ToString()
        {
            return Code.ToString().TrimEnd();
        }
    }
}
