using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace StageMapper
{
    internal class Debugger
    {
        public static Debugger _debug = new Debugger(Modes.None, Modes.None);
        public static Debugger Debug { get { return _debug; } }

        private static ReaderWriterLockSlim _readWriteLock1 = new ReaderWriterLockSlim();
        private static ReaderWriterLockSlim _readWriteLock2 = new ReaderWriterLockSlim();

        public enum Modes { File, Console, None }

        public Modes DebugMode { get; set; }
        public Modes ErrorMode { get; set; }

        public string Folder { get; set; }

        public Debugger(Modes debug, Modes error)
        {
            DebugMode = debug;
            ErrorMode = error;
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folder = Path.Combine(folder, "PetriPrinter");
            Folder = folder;
            Directory.CreateDirectory(folder);

        }

        public void WriteError(string msg, params object[] obj)
        {
            WriteError(string.Format(msg, obj));
        }
        public void WriteError(string msg)
        {
            msg = String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now) + "; " + msg;
            if (ErrorMode == Modes.File)
            {
                _readWriteLock1.EnterWriteLock();
                try
                {
                    string path = Path.Combine(Folder, "error-" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                    using (StreamWriter w = new StreamWriter(path, true))
                    {
                        w.WriteLine(msg);
                    }
                }
                finally
                {
                    _readWriteLock1.ExitWriteLock();
                }
            }
            else if (ErrorMode == Modes.Console)
            {
                Console.WriteLine(msg);
            }
        }
        public void WriteDebug(string msg, params object[] obj)
        {
            WriteDebug(string.Format(msg, obj));
        }

        public void WriteDebug(string msg)
        {
            msg = String.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now) + "; " + msg;
            if (DebugMode == Modes.File)
            {
                _readWriteLock2.EnterWriteLock();

                try
                {
                    string path = Path.Combine(Folder, "debug-" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                    using (StreamWriter w = new StreamWriter(path, true))
                    {
                        w.WriteLine(msg);
                    }
                }
                finally
                {
                    _readWriteLock2.ExitWriteLock();
                }
            }
            else if (DebugMode == Modes.Console)
            {
                Console.WriteLine(msg);
            }

        }
    }
}
