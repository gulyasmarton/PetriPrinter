using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GCodeAPI
{
    /// <summary>
    /// This class provides translation of *.gcode text file to IGCode objects.
    /// </summary>
    public class GCodeReader
    {
        /// <summary>
        /// Returns the collection of the retrieved and translation IGCode objects.
        /// </summary>
        public GCodeCollector Code { get; private set; }

        /// <summary>
        /// Constructs a new GCodeReader from a byte array of g-code text file.
        /// </summary>
        /// <param name="file">byte array of g-code text file</param>
        public GCodeReader(byte[] file) : this(new MemoryStream(file)) { }

        /// <summary>
        /// Constructs a new GCodeReader from a text file.
        /// </summary>
        /// <param name="path">Path of text file</param>
        public GCodeReader(String path) : 
            this(File.Open(path,FileMode.Open)) { }

        /// <summary>
        /// Constructs a new GCodeReader from a Stream object of g-code text file. 
        /// </summary>
        /// <param name="file">Stream object of g-code text file</param>
        public GCodeReader(Stream file)
        {
           
            Code = new GCodeCollector();
            string line;


            using (StreamReader text = new StreamReader(file))
            {
                while ((line = text.ReadLine()) != null)
                {
                    IGCode row = Read(line);
                    if (row != null) Code.addCode(row);   
                }
            }

        }

        /// <summary>
        /// Translates and returns an IGCode object of specified string.
        /// </summary>
        /// <param name="line">String to be translated</param>
        /// <returns></returns>
        public static IGCode Read(string line)
        {
            string org = line;
            string[] part = line.Split(';');
            line = part[0].Trim();
            if (line.Length == 0)
                return null;
            part = part[0].Split(' ');

            Regex regex = new Regex(@"M\d+");
            Match match = regex.Match(part[0]);
            if (match.Success)
            {
                switch (match.Value)
                {
                    case "M82":
                        return GCodeSpecial.setExtruderAbsoluteMode();
                    case "M83":
                        return GCodeSpecial.setExtruderRelativeMode();
                    case "M84":
                        return GCodeSpecial.StepperOff();
                    case "M104":
                        return GCodeSpecial.HeaterOff();
                    case "M106":
                        double? s = getValue(part, "S");
                        if (s != null)
                            return GCodeSpecial.FanOn(s.Value);
                        break;
                    case "M107":
                        return GCodeSpecial.FanOff();
                    case "M109":
                        regex = new Regex(@"T\d+");
                        
                        double? s1 = getValue(part, "S");
                        if (s1 != null)
                        {                          
                            return GCodeSpecial.HeaterAndWait(s1.Value);
                        }
                        break;
                    case "M117":
                        return null;
                    case "M140":
                        return null;

                }
            }

            regex = new Regex(@"G\d+");
            match = regex.Match(part[0]);
            if (match.Success)
            {
                var xg0 = getValue(part, "X");
                var yg0 = getValue(part, "Y");
                var zg0 = getValue(part, "Z");
                var fg0 = getValue(part, "F");
                var eg0 = getValue(part, "E");
                switch (match.Value)
                {
                    case "G0":
                        return new GCodeLine(xg0, yg0, zg0, fg0, eg0, GCodeLine.SpeedModes.Fast);
                    case "G1":
                        return new GCodeLine(xg0, yg0, zg0, fg0, eg0, GCodeLine.SpeedModes.Slow);
                    case "G21":
                        return GCodeSpecial.setMetricValues();
                    case "G28":
                        var x1 = getValue(part, "X");
                        var y1 = getValue(part, "Y");
                        var z1 = getValue(part, "Z");
                        //if (x1 != null || y1 != null || z1 != null)
                        return GCodeSpecial.GoToHome(x1 != null, y1 != null, z1 != null);
                    case "G90":
                        return GCodeSpecial.setAbsolutePositionMode();
                    case "G91":
                        return GCodeSpecial.setRelavitvePositionMode();
                    case "G92":
                        var v1 = getValue(part, "Z");
                        var v2 = getValue(part, "E");
                        if (v1 != null) return GCodeSpecial.ResetToZ(v1.Value);
                        if (v2 != null) return GCodeSpecial.ResetToExtrude(v2.Value);
                        break;

                }
            }

            if (part[0].Equals("T0"))
                return null;

            return new UnknownGCode(line);
        }

        protected static double? getValue(string[] tags, string flag)
        {
            var regex = new Regex(flag + @"[+-]?\d*(\.\d+)?");
            foreach (var s in tags)
            {
                Match match = regex.Match(s);
                if (match.Success)
                {
                    string num = match.Value.Substring(flag.Length);
                    return Double.Parse(num, CultureInfo.InvariantCulture);
                }
            }
            return null;
        }
    }
}
