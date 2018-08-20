using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GCodeAPI
{
    /// <summary>
    /// This is a g-code collection which help to manage IGCode objects.
    /// </summary>
    [Serializable]
    public class GCodeCollector
    {
        /// <summary>
        /// Gets the list of IGCode objects.
        /// </summary>
        public List<IGCode> Codes { get; private set; }

        /// <summary>
        /// Returns the XY position of last GCodeLine object.
        /// </summary>
        public Point LastPoint
        {
            get
            {
                var _lastPoint = new Point(0, 0);
                foreach (var code in Codes)
                    if (code is GCodeLine)
                    {
                        var line = (GCodeLine)code;
                        if (line.x != null)
                            _lastPoint.X = line.x.Value;
                        if (line.y != null)
                            _lastPoint.Y = line.y.Value;
                    }
                    else if (code is GCodeEllipse)
                        _lastPoint = ((GCodeEllipse)code).Code.LastPoint;
                return _lastPoint;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GCodeCollector()
        {
            Codes = new List<IGCode>();
        }

        /// <summary>
        /// Constructs a new GCodeCollector and sets the capacity of its list.
        /// </summary>
        /// <param name="capacity"> The number of elements that the new list can initially store.</param>
        public GCodeCollector(int capacity)
        {
            Codes = new List<IGCode>(capacity);
        }

        /// <summary>
        /// Constructs a new GCodeCollector and sets its list.
        /// </summary>
        /// <param name="code">list of IGCode objects</param>
        public GCodeCollector(List<IGCode> code)
        {
            Codes = code;
        }

        /// <summary>
        /// Appends the specified element to the end of this list.
        /// </summary>
        /// <param name="code">element to be appended to this list</param>
        public void addCode(IGCode code)
        {
            Codes.Add(code);
        }

        /// <summary>
        /// Appends all of the elements in the specified GCodeCollector to the end of this list.
        /// </summary>
        /// <param name="codes"> GCodeCollector containing elements to be added to this list</param>
        public void addCode(GCodeCollector codes)
        {
            Codes.AddRange(codes.Codes);
        }

        /// <summary>
        /// Returns to the g-code string of all elements.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sbr = new StringBuilder(Codes.Count * 70);
            foreach (var c in Codes)
                sbr.AppendLine(c.ToString());
            return sbr.ToString();
        }

        /// <summary>
        /// Saves the g-code strings as a text file.
        /// </summary>
        /// <param name="path"></param>
        public void SaveTogCodeFile(string path)
        {
            using (StreamWriter outfile = new StreamWriter(path))
            {
                outfile.Write(ToString());
            }
        }

        /// <summary>
        /// Return a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        public GCodeCollector Duplicate()
        {
            GCodeCollector collector = new GCodeCollector(Codes.Count);
            foreach (var code in Codes)
            {
                collector.addCode(code.Duplicate());
            }
            return collector;
        }
    }
}
