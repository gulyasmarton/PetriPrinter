using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCodeAPI
{
    /// <summary>
    /// This class is a collection of different g-code transformation functions.
    /// </summary>
    public static class GCodeTransforms
    {
        /// <summary>
        /// Returns the bounding rectangle of the specified g-code collection.
        /// </summary>
        /// <param name="code">Specified g-code collection</param>
        /// <returns></returns>
        public static Rect bound(GCodeCollector code)
        {
            double minx = Double.MaxValue;
            double miny = Double.MaxValue;
            double maxx = Double.MinValue;
            double maxy = Double.MinValue;
            GCodeLine last = null;
            foreach (var c in code.Codes)
            {
                var l = c as GCodeLine;
                if (l != null)
                {
                    last = l;
                    if (l.x != null)
                    {
                        if (l.x.Value > maxx) maxx = l.x.Value;
                        if (l.x.Value < minx) minx = l.x.Value;
                    }
                    if (l.y != null)
                    {
                        if (l.y.Value > maxy) maxy = l.y.Value;
                        if (l.y.Value < miny) miny = l.y.Value;
                    }
                }

                var els = c as GCodeEllipse;
                if (els != null)
                {
                    foreach (var o in els.Code.Codes)
                    {
                        l = (GCodeLine)o;
                        last = l;
                        if (l.x != null)
                        {
                            if (l.x.Value > maxx) maxx = l.x.Value;
                            if (l.x.Value < minx) minx = l.x.Value;
                        }
                        if (l.y != null)
                        {
                            if (l.y.Value > maxy) maxy = l.y.Value;
                            if (l.y.Value < miny) miny = l.y.Value;
                        }
                    }
                }

                var e = c as GCodeCircle;
                if (last != null && e != null)
                {
                    var rec = e.Bound;
                    if (last.x != null)
                    {
                        if (last.x.Value + rec.MaxX > maxx) maxx = last.x.Value + rec.MaxX;
                        if (last.x.Value + rec.MinX < minx) minx = last.x.Value + rec.MinX;
                    }
                    if (last.y != null)
                    {
                        if (last.y.Value + rec.MaxY > maxy) maxy = last.y.Value + rec.MaxY;
                        if (last.y.Value + rec.MinY < miny) miny = last.y.Value + rec.MinY;
                    }
                }
            }
            return new Rect(minx, miny, maxx, maxy);
        }

        /// <summary>
        /// Shifts the center of specified g-code collection to the specified position.
        /// </summary>
        /// <param name="x">New X center of specified g-code collection</param>
        /// <param name="y">New Y center of specified g-code collection</param>
        /// <param name="code">specified g-code collection</param>
        /// <returns></returns>
        public static GCodeCollector codeShiftCenterTo(double x, double y, GCodeCollector code)
        {
            var center = getCenter(code);
            return codeShift(x - center.X, y - center.Y, code);
        }

        /// <summary>
        /// Shifts specified g-code collection with the specified value.
        /// </summary>
        /// <param name="x">Added this value to all x positions.</param>
        /// <param name="y">Added this value to all y positions.</param>
        /// <param name="incode">specified g-code collection</param>
        /// <returns></returns>
        public static GCodeCollector codeShift(double x, double y, GCodeCollector incode)
        {
            GCodeCollector backcode = new GCodeCollector(incode.Codes.Count);

            foreach (var c in incode.Codes)
            {
                var l = c as GCodeLine;
                var els = c as GCodeEllipse;
                if (l != null)
                {
                    if (l.x != null || l.y != null)
                    {
                        double? xk = l.x != null ? l.x + x : null;
                        double? yk = l.y != null ? l.y + y : null;

                        GCodeLine nl = new GCodeLine(xk, yk, l.z, l.f, l.e, l.SpeedMode);
                        backcode.addCode(nl);
                        continue;
                    }
                    backcode.addCode(l);
                }
                else if (els != null)
                {
                    els = (GCodeEllipse)els.Duplicate();
                    foreach (var o in els.Code.Codes)
                    {
                        l = (GCodeLine)o;
                        if (l.x != null || l.y != null)
                        {
                            double? xk = l.x != null ? l.x + x : null;
                            double? yk = l.y != null ? l.y + y : null;
                            l.x = xk;
                            l.y = yk;
                        }
                    }
                    backcode.addCode(els);
                }
                else
                {
                    backcode.addCode(c);
                }
                
            }

            return backcode;
        }

        /// <summary>
        /// Shifts specified g-code collection with the specified value.
        /// </summary>
        /// <param name="z">Added this value to all z positions.</param>
        /// <param name="incode">specified g-code collection</param>
        /// <returns></returns>
        public static GCodeCollector codeShift(double z, GCodeCollector incode)
        {
            GCodeCollector backcode = new GCodeCollector(incode.Codes.Count);

            foreach (var c in incode.Codes)
            {
                var l = c as GCodeLine;
                if (l != null)
                {
                    if (l.z != null)
                    {
                        GCodeLine nl = new GCodeLine(l.x, l.y, l.z + z, l.f, l.e, l.SpeedMode);
                        backcode.addCode(nl);
                        continue;
                    }
                }
                backcode.addCode(c);
            }

            return backcode;
        }

        /// <summary>
        /// Shifts the specified g-code collection into the center.
        /// </summary>
        /// <param name="code">specified g-code collection</param>
        /// <returns></returns>
        public static GCodeCollector setCorner(GCodeCollector code)
        {
            var b = bound(code);
            return codeShift(-b.X1, -b.Y1, code);
        }

        /// <summary>
        /// Returns the center of bounding rectangle.
        /// </summary>
        /// <param name="code">specified g-code collection</param>
        /// <returns></returns>
        public static Point getCenter(GCodeCollector code)
        {
            return bound(code).Center;
        }

        /// <summary>
        /// Returns the minimum Z position of specified g-code collection.
        /// </summary>
        /// <param name="code">specified g-code collection</param>
        /// <returns></returns>
        public static double getZNull(GCodeCollector code)
        {
            double z = double.MaxValue;
            foreach (var c in code.Codes)
            {
                var l = c as GCodeLine;
                if (l != null && l.z != null && l.z.Value < z)
                    z = l.z.Value;

            }
            return z;
        }
        /// <summary>
        /// Shifts the specified g-code collection to zero Z axis position.
        /// </summary>
        /// <param name="code">specified g-code collection</param>
        /// <returns></returns>
        public static GCodeCollector codeZnull(GCodeCollector code)
        {
            return codeShift(-getZNull(code), code);
        }

    }


}
