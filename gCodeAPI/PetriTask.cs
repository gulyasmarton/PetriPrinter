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
    /// This class represents a object which is prepared to print into a petri dish.
    /// </summary>
    [Serializable]
    public class PetriTask
    {
        /// <summary>
        /// Name of object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of object
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This is the main code collection. During the reposition process (Print functions) this code collection is centered also.
        /// </summary>
        public GCodeCollector Code { get; set; }
        
        /// <summary>
        /// This function shifts to the equivalent position the codes of the object.
        /// </summary>
        /// <param name="code">Input code collection (byRef). The new codes are appended to this collection.</param>
        /// <param name="stagePositonX">X positon of center of the printing area</param>
        /// <param name="stagePositonY">Y positon of center of the printing area</param>
        /// <param name="zOff">z offset value</param>
        public void Print(GCodeCollector code, double stagePositonX, double stagePositonY, double zOff)
        {
            Print(code, new Point(stagePositonX, stagePositonY), zOff);
        }

        /// <summary>
        /// This function shifts to the equivalent position the codes of the object.
        /// </summary>
        /// <param name="code">Input code collection (byRef). The new codes are appended to this collection.</param>
        /// <param name="stagePositon">The position of center of the printing area</param>
        /// <param name="zOff">z offset value</param>
        public void Print(GCodeCollector code, Point stagePositon, double zOff)
        {
            var cc = GCodeTransforms.codeShift(zOff, Code.Duplicate());
            code.addCode(GCodeTransforms.codeShift(stagePositon.X, stagePositon.Y, cc));

        }

        public PetriTask(string name) : this(name, "No description") { }

        public PetriTask(string name, string description) : this(name, description, new GCodeCollector()) { }

        public PetriTask(string name, string description, GCodeCollector mainCode)
        {
            this.Name = name;
            this.Description = description;
            Code = mainCode;
        }

        /// <summary>
        /// Returns the name of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Serializes the object into a binary file.
        /// </summary>
        /// <param name="path">File's path.</param>
        public void Save(string path)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(stream, this);
            }
        }

        /// <summary>
        /// Returns the memory stream of binarized object.
        /// </summary>
        /// <returns></returns>
        public Stream GetStream()
        {
            var formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, this);
            return stream;
        }

      
        /// <summary>
        /// Opens a binarized object.
        /// </summary>
        /// <param name="p">File's path</param>
        /// <returns></returns>
        public static PetriTask Load(string p)
        {
            PetriTask obj = null;
            var formatter = new BinaryFormatter();
            try
            {
                using (Stream stream = new FileStream(p, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    obj = (PetriTask)formatter.Deserialize(stream);
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return obj;
        }  
    }
}
