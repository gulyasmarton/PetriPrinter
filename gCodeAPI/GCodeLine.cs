using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeAPI
{
    /// <summary>
    /// This class wraps the G0 and G1 g-codes which provide the printer track of line.
    /// </summary>
    [Serializable]
    public class GCodeLine : IGCode
    {
        /// <summary>
        /// Returns the name of IGCode
        /// </summary>
        public string Name { get { return "Move (" + ToString() + ")"; } }

        /// <summary>
        /// This enum wraps Rapid linear Move (G0, Fast) and Linear Move (G1, Slow)
        /// </summary>
        public enum SpeedModes { Fast, Slow };

        public double? x, y, z, f, e;

        /// <summary>
        /// Sets or returns the current speed mode [Rapid linear Move (G0, Fast) or Linear Move (G1, Slow)]
        /// </summary>
        public SpeedModes SpeedMode { get; set; }

        /// <summary>
        /// Returns true if this line contain x, y or z moving command.
        /// </summary>
        public bool isMoving { get { return (x != null || y != null || z != null); } }

        /// <summary>
        /// Returns true if this line contain x or y moving command.
        /// </summary>
        public bool isMovingXY { get { return (x != null || y != null); } }

        /// <summary>
        /// Returns true if this line contain z moving command.
        /// </summary>
        public bool isMovingZ { get { return (z != null); } }

        /// <summary>
        /// Returns a Point which contains the x y positions or null if there is not x-y positions.
        /// </summary>
        public Point Position { get { return (x!=null && y!=null)? new Point(x.Value, y.Value):null; } }

        /// <summary>
        /// Constructs a new GCodeLine object with the specified parameters.
        /// </summary>
        /// <param name="x">The position to move to on the X axis</param>
        /// <param name="y">The position to move to on the Y axis</param>
        /// <param name="z">The position to move to on the Z axis</param>
        /// <param name="speed">The feed-rate per minute of the move between the starting point and ending point</param>
        /// <param name="extrude">The amount to extrude between the starting point and ending point</param>
        /// <param name="mode">Rapid linear Move (G0, Fast) and Linear Move (G1, Slow)</param>
        public GCodeLine(double? x = null, double? y = null, double? z = null, double? speed = null, double? extrude = null, SpeedModes mode = SpeedModes.Slow)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.f = speed;
            this.e = extrude;
            this.SpeedMode = mode;
        }

        /// <summary>
        /// Returns the g-code string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string s;
            if (SpeedMode == SpeedModes.Fast)
                s = "G0";
            else
                s = "G1";

            if (f != null)
                s += " F" + f.Value.ToString("F6", CultureInfo.InvariantCulture);

            if (x != null)
                s += " X" + x.Value.ToString("F6", CultureInfo.InvariantCulture);
            if (y != null)
                s += " Y" + y.Value.ToString("F6", CultureInfo.InvariantCulture);
            if (z != null)
                s += " Z" + z.Value.ToString("F6", CultureInfo.InvariantCulture);

            if (e != null)
                s += " E" + e.Value.ToString("F6", CultureInfo.InvariantCulture);

            return s;
        }

        /// <summary>
        /// Return a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public IGCode Duplicate()
        {
            return new GCodeLine(x, y, z, f, e, SpeedMode);
        }
    }
}
