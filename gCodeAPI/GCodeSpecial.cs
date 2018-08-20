using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeAPI
{
    [Serializable]
    public class GCodeSpecial : IGCode
    {
        /// <summary>
        /// Default extruding ratio.
        /// </summary>
        public static double ExtrudeRatio = 0.009d;

        /// <summary>
        /// Enum of axis.
        /// </summary>
        public enum Axis { X, Y, Z, E }

        /// <summary>
        /// Returns the name of IGCode.
        /// </summary>
        public string Name { get; private set; }
        string s;

        private GCodeSpecial(string s, string name) { this.s = s; Name = name; }

        /// <summary>
        /// Returns the g-code string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return s;
        }

        /// <summary>
        /// Sets units to inches.
        /// </summary>
        /// <returns></returns>
        public static GCodeSpecial setInchValues()
        {
            return new GCodeSpecial("G20", "inch values");
        }

        /// <summary>
        /// Sets units to millimeters.
        /// </summary>
        /// <returns></returns>
        public static GCodeSpecial setMetricValues()
        {
            return new GCodeSpecial("G21", "metric values");
        }

        /// <summary>
        /// Sets to absolute positioning.
        /// </summary>
        /// <returns></returns>
        public static GCodeSpecial setAbsolutePositionMode()
        {
            return new GCodeSpecial("G90", "absolute positioning");
        }

        /// <summary>
        /// Sets to relative positioning.
        /// </summary>
        /// <returns></returns>
        public static GCodeSpecial setRelavitvePositionMode()
        {
            return new GCodeSpecial("G91", "relative positioning");
        }

        /// <summary>
        /// Sets extruder to absolute mode.
        /// </summary>
        /// <returns></returns>
        public static GCodeSpecial setExtruderAbsoluteMode()
        {
            return new GCodeSpecial("M82", "sets extruder to absolute mode");
        }

        /// <summary>
        /// Sets extruder to relative mode.
        /// </summary>
        /// <returns></returns>
        public static GCodeSpecial setExtruderRelativeMode()
        {
            return new GCodeSpecial("M83", "sets extruder to relative mode");
        }

        /// <summary>
        /// Sets fan off.
        /// </summary>
        /// <returns></returns>
        public static GCodeSpecial FanOff()
        {
            return new GCodeSpecial("M107", "fan off");
        }

        /// <summary>
        /// Sets absolute zero position on the Z axis.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IGCode ResetToZ(double value)
        {
            return new ResetTo(value, Axis.Z);
        }

        /// <summary>
        /// Sets absolute zero position of extruder.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IGCode ResetToExtrude(double value)
        {
            return new ResetTo(value, Axis.E);
        }

        /// <summary>
        /// Sets extruder temperature and wait.
        /// </summary>
        /// <param name="temperature">temperature in Celsius</param>
        /// <returns></returns>
        public static IGCode HeaterAndWait(double temperature)
        {
            return new HeaterAndWait(temperature);
        }

        /// <summary>
        /// Turns off heater.
        /// </summary>
        /// <returns></returns>
        public static GCodeSpecial HeaterOff()
        {
            return new GCodeSpecial("M104 S0", "heater off");
        }

        /// <summary>
        /// Turns off motors.
        /// </summary>
        /// <returns></returns>
        public static GCodeSpecial StepperOff()
        {
            return new GCodeSpecial("M84", "motors off");
        }

        /// <summary>
        /// Returns the necessary extruding length.
        /// </summary>
        /// <param name="radius">radius of circle</param>
        /// <returns></returns>
        public static double ExtrudeCircle(double radius)
        {
            return ExtrudeLine(2 * radius * Math.PI);
        }

        /// <summary>
        /// Returns the necessary extruding length.
        /// </summary>
        /// <param name="length">length of line</param>
        /// <returns></returns>
        public static double ExtrudeLine(double length)
        {
            return length * ExtrudeRatio;
        }

        /// <summary>
        /// Returns the necessary extruding length.
        /// </summary>
        /// <param name="x1">X position of Start point</param>
        /// <param name="y1">Y position of Start point</param>
        /// <param name="x2">X position of End point</param>
        /// <param name="y2">Y position of End point</param>
        /// <returns></returns>
        public static double ExtrudeLine(double x1, double y1, double x2, double y2)
        {
            double dist = Point.Distance(x1, y1, x2, y2);
            return ExtrudeLine(dist);
        }

        /// <summary>
        /// Returns the necessary extruding length.
        /// </summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End Point</param>
        /// <returns></returns>
        public static double ExtrudeLine(Point p1, Point p2)
        {
            double dist = Point.Distance(p1, p2);
            return ExtrudeLine(dist);
        }

        /// <summary>
        /// Sets speed of the fan.
        /// </summary>
        /// <param name="speed">0-255 value</param>
        /// <returns></returns>
        public static IGCode FanOn(double speed)
        {
            return new FanOn(speed);
        }

        /// <summary>
        /// Goes to home specified axis.
        /// </summary>
        /// <param name="x">Goes to home on X axis</param>
        /// <param name="y">Goes to home on Y axis</param>
        /// <param name="z">Goes to home on Z axis</param>
        /// <returns></returns>
        public static IGCode GoToHome(bool x = false, bool y = false, bool z = false)
        {
            return new GoToHome(x, y, z);
        }

        /// <summary>
        /// Return a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public IGCode Duplicate()
        {
            return new GCodeSpecial(s, Name);
        }


    }
    /// <summary>
    /// Wrapper class for uncoded g-code lines. This class stores strings which are not translated by GCodeReader.
    /// </summary>
    [Serializable]
    public class UnknownGCode : IGCode
    {
        /// <summary>
        /// Returns the name of IGCode
        /// </summary>
        public string Name { get { return "Unknown"; } }
        public string Code { get; set; }

        /// <summary>
        /// Constructs a new UnknownGCode object with the specified string.
        /// </summary>
        /// <param name="code"></param>
        public UnknownGCode(string code)
        {
            this.Code = code;
        }

        /// <summary>
        /// Returns with the original string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Code;
        }

        /// <summary>
        /// Returns a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public IGCode Duplicate()
        {
            return new UnknownGCode(Code);
        }
    }

    #region Special g-codes with parameter

    /// <summary>
    /// Sets the speed of fan.
    /// </summary>
    [Serializable]
    public class FanOn : IGCode
    {
        /// <summary>
        /// Returns the name of IGCode
        /// </summary>
        public string Name { get { return "fan on"; } }

        /// <summary>
        /// Sets of gets speed of fan.
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Constructs a new FanOn class with the specified speed.
        /// </summary>
        /// <param name="speed"></param>
        public FanOn(double speed)
        {
            Speed = speed;

        }

        /// <summary>
        /// Returns the g-code string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "M106 S" + Speed.ToString("F6", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public IGCode Duplicate()
        {
            return new FanOn(Speed);
        }
    }

    /// <summary>
    /// Goes to home specified axis.
    /// </summary>
    [Serializable]
    public class GoToHome : IGCode
    {
        /// <summary>
        /// Returns the name of IGCode
        /// </summary>
        public string Name
        {
            get
            {
                string b = X ? "X" : "";
                b += Y ? "Y" : "";
                b += Z ? "Z" : "";
                if (b.Equals("")) b = "XYZ";
                return "go to home (" + b + ")";
            }
        }

        /// <summary>
        /// Gets or sets X axis reseting.
        /// </summary>
        public bool X { get; set; }

        /// <summary>
        /// Gets or sets Y axis reseting.
        /// </summary>
        public bool Y { get; set; }

        /// <summary>
        /// Gets or sets Z axis reseting.
        /// </summary>
        public bool Z { get; set; }

        /// <summary>
        /// Constructs a new GoToHome object with the specified axis settings.
        /// </summary>
        /// <param name="x">Goes to home on X axis</param>
        /// <param name="y">Goes to home on Y axis</param>
        /// <param name="z">Goes to home on Z axis</param>
        public GoToHome(bool x = false, bool y = false, bool z = false)
        {
            X = x;
            Y = y;
            Z = z;
            string s = "G28";
            if (x) s += " X0";
            if (y) s += " Y0";
            if (z) s += " Z0";
            if (s.Length == 3) s = "";
        }

        /// <summary>
        /// Returns the g-code string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string s = "G28";
            if (X) s += " X0";
            if (Y) s += " Y0";
            if (Z) s += " Z0";
            //if (s.Length == 3) s = "";
            return s;
        }

        /// <summary>
        /// Returns a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public IGCode Duplicate()
        {
            return new GoToHome(X, Y, Z);
        }
    }

    /// <summary>
    /// Sets extruder temperature and wait.
    /// </summary>
    [Serializable]
    public class HeaterAndWait : IGCode
    {
        /// <summary>
        /// Returns the name of IGCode
        /// </summary>
        public string Name { get { return "Heat and wait (T=" + Temperature + "°C)"; } }

        /// <summary>
        /// Sets or gets temperature of heater.
        /// </summary>
        double Temperature { get; set; }

        /// <summary>
        /// Constructs a new HeaterAndWait object with the specified temperature value.
        /// </summary>
        /// <param name="temperature">temperature in Celsius</param>
        public HeaterAndWait(double temperature)
        {
            Temperature = temperature;

        }

        /// <summary>
        /// Returns the g-code string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "M109 S" + Temperature.ToString("F6", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public IGCode Duplicate()
        {
            return new HeaterAndWait(Temperature);
        }
    }

    /// <summary>
    /// Sets absolute zero position on the specified axis.
    /// </summary>
    [Serializable]
    public class ResetTo : IGCode
    {
        /// <summary>
        /// Returns the name of IGCode
        /// </summary>
        public string Name
        {
            get
            {
                string b = "";

                switch (Axis)
                {
                    case GCodeSpecial.Axis.X:
                        b = "X";
                        break;
                    case GCodeSpecial.Axis.Y:
                        b = "Y";
                        break;
                    case GCodeSpecial.Axis.Z:
                        b = "Z";
                        break;
                    case GCodeSpecial.Axis.E:
                        b = "Extrude";
                        break;
                }

                return "Reset " + b;
            }
        }

        /// <summary>
        /// Sets or gets the specified axis.
        /// </summary>
        public GCodeSpecial.Axis Axis { get; set; }

        /// <summary>
        /// Set or gets the new absolute zero position.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Constructs a new ResetTo object with the specified value on the specified axis.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="axis"></param>
        public ResetTo(double value, GCodeSpecial.Axis axis)
        {
            Value = value;
            Axis = axis;
        }

        /// <summary>
        /// Returns the g-code string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var s = "";
            switch (Axis)
            {
                case GCodeSpecial.Axis.X:
                    s = "G92 X" + Value.ToString("F6", CultureInfo.InvariantCulture);
                    break;
                case GCodeSpecial.Axis.Y:
                    s = "G92 Y" + Value.ToString("F6", CultureInfo.InvariantCulture);
                    break;
                case GCodeSpecial.Axis.Z:
                    s = "G92 Z" + Value.ToString("F6", CultureInfo.InvariantCulture);
                    break;
                case GCodeSpecial.Axis.E:
                    s = "G92 E" + Value.ToString("F6", CultureInfo.InvariantCulture);
                    break;
            }
            return s;
        }

        /// <summary>
        /// Returns a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public IGCode Duplicate()
        {
            return new ResetTo(Value, Axis);
        }
    }

    /// <summary>
    /// Pause the machine for a period of time
    /// </summary>
    [Serializable]
    public class Wait : IGCode
    {
        public string Name { get { return string.Format("Wait {0} seconds", Time); } }

        /// <summary>
        /// Set or gets the time to wait, in milliseconds.
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Default constroctor
        /// </summary>
        /// <param name="msec">Time to wait, in milliseconds </param>
        public Wait(int msec)
        {
            Time = msec;
        }

        /// <summary>
        /// Returns the g-code string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "G4 P" + Time;
        }

        /// <summary>
        /// Returns a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public IGCode Duplicate()
        {
            return new Wait(Time);
        }
    }
    #endregion
}
