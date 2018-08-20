using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace gCodeEditor.Model
{
    public class IOTools
    {
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

        public static T Open<T>(String path)
        {
            var formatter = new BinaryFormatter();
            try
            {
                using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    return (T)formatter.Deserialize(stream);
            }
            catch (Exception e) { Console.WriteLine(e.Message); Console.WriteLine(e.StackTrace); }
            return default(T);
        }
    }
}
