using GCodeAPI;
using IronPython.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace PetriPrinter.Model
{
    public static class PetriIO
    {
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static string DefaultFolder()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PetriPrinter");
            Directory.CreateDirectory(path);
            return path;
        }

        public static GridConfig LoadDefaultGridConfig()
        {

            var path = Path.Combine(DefaultFolder(), "gridconfig.bin");
            var obj = Open(path);
            if (obj is GridConfig)
                return (GridConfig)obj;
            return new GridConfig(5, 5, 38d, 38d, 29d, 29d);
        }

        public static void SaveDefaultGridConfig(GridConfig config)
        {

            var path = Path.Combine(DefaultFolder(), "gridconfig.bin");
            Save(path, config);
        }

        public static PrinterConfig LoadDefaultPrinterConfig()
        {

            var path = Path.Combine(DefaultFolder(), "priterconfig.bin");
            var obj = Open(path);
            if (obj is PrinterConfig)
                return (PrinterConfig)obj;
            return new PrinterConfig(230d, 1.4);
        }

        public static void SaveProject(string path, GridProject project)
        {
            Save(path, project);
        }

        public static GridProject OpenProject(string path)
        {
            return Open(path) as GridProject;
        }

        public static void SaveDefaultPrinterConfig(PrinterConfig config)
        {

            var path = Path.Combine(DefaultFolder(), "priterconfig.bin");
            Save(path, config);
        }

        public static List<PetriTask> LoadAvailablePetriTaskList()
        {
            //var files = Directory.GetFiles(DefaultFolder(), "*.ptf", SearchOption.AllDirectories);

            var files = Directory.EnumerateFiles(DefaultFolder()).Where(file => file.ToLower().EndsWith("ptf") || file.ToLower().EndsWith("py")).ToList();

            List<PetriTask> list = new List<PetriTask>();

            foreach (var file in files)
            {
                PetriTask task = null;
                if (file.Contains("ptf"))
                    task = OpenPetriTask(file);
                if (file.Contains("py"))
                    task = OpenPyton(file);

                if (task != null)
                    list.Add(task);
            }

            return list;
        }

        public static PetriTask OpenPyton(string file)
        {
            try
            {
                var ipy = Python.CreateRuntime();
                dynamic test = ipy.UseFile(file);

                string name = test.Name();
                string description = test.Description();
                GCodeCollector code = test.Code();

                return new PetriTask(name, description, code);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        private static PetriTask OpenPetriTask(string file)
        {
            return PetriTask.Load(file);
        }


        public static void Save(String path, object obj)
        {
            var formatter = new BinaryFormatter();
            try
            {
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    formatter.Serialize(stream, obj);
            }
            catch (Exception e) { Console.WriteLine(e.Message); Console.WriteLine(e.StackTrace); }
        }

        public static object Open(String path)
        {
            var formatter = new BinaryFormatter();
            try
            {
                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    return formatter.Deserialize(stream);
            }
            catch (Exception e) { Console.WriteLine(e.Message); Console.WriteLine(e.StackTrace); }
            return null;
        }

        internal static void CopyTaskFile(string path)
        {
            var path2 = Path.Combine(DefaultFolder(), Path.GetFileName(path));
            File.Copy(path, path2, true);
        }
    }
}
