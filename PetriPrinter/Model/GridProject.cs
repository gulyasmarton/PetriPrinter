using GCodeAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PetriPrinter.Model
{
    [Serializable]
    public class GridProject : ISerializable
    {
        PetriTask[,] Tasks;

        private GridConfig _grid;
        public GridConfig Grid
        {
            get { return _grid; }
            set
            {

                var grid = new PetriTask[value.Columns, value.Rows];
                int h = Math.Min(value.Columns, _grid.Columns);
                int w = Math.Min(value.Rows, _grid.Rows);
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        grid[x, y] = Tasks[x, y];
                    }
                _grid = value;
                Tasks = grid;
                isChanged = true;
            }
        }

        public List<double?> GridMap { get; set; }

        public string Name
        {
            get
            {

                if (Path == null) return "Unknown";
                return System.IO.Path.GetFileNameWithoutExtension(Path);
            }
        }

        [NonSerialized]
        string _path;
        public string Path { get { return _path; } set { _path = value; } }

        [NonSerialized]
        bool _isChanged;
        public bool isChanged { get { return _isChanged; }
            set {
                _isChanged = value;
                if (Changed != null) Changed(value);
            }
        }

        private PrinterConfig _printer;
        public PrinterConfig Printer { get { return _printer; } set { _printer = value; isChanged = true; } }
   
        public event Action<bool> Changed;

        public GridProject(GridConfig grid, PrinterConfig printer)
        {
            this.Grid = grid;
            this.Printer = printer;
            Tasks = new PetriTask[grid.Columns, grid.Rows];
        }

        protected GridProject(SerializationInfo info, StreamingContext context)
        {
            Tasks = (PetriTask[,])info.GetValue("Tasks",typeof(PetriTask[,]));
            _grid = (GridConfig)info.GetValue("Grid", typeof(GridConfig));
            _printer = (PrinterConfig)info.GetValue("Printer", typeof(PrinterConfig));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Tasks", Tasks);
            info.AddValue("Grid", Grid);
            info.AddValue("Printer", Printer);
        }

        internal PetriTask Get(int x, int y)
        {
            return Tasks[x, y];
        }

        public void Add(PetriTask task, int columns, int rows)
        {
            if (task == null)
                return;
            if (columns < 0 || rows < 0)
                return;
            if (columns >= Grid.Columns || rows >= Grid.Columns)
                return;
            Tasks[columns, rows] = task;

            if (Changed != null) Changed(true);
        }

        public void Remove(int columns, int rows)
        {
            if (columns < 0 || rows < 0)
                return;
            if (columns >= Grid.Columns || rows >= Grid.Columns)
                return;
            Tasks[columns, rows] = null;
            isChanged = true;
            if (Changed != null) Changed(true);
        }

        public void GenerateCode(string path)
        {
            GCodeCollector Code = new GCodeCollector();
            Code.Codes.Clear();
            Code.addCode(GCodeSpecial.HeaterAndWait(Printer.Temperature));
            Code.addCode(GCodeSpecial.setMetricValues());
            Code.addCode(GCodeSpecial.setAbsolutePositionMode());
            Code.addCode(GCodeSpecial.setExtruderRelativeMode());
            Code.addCode(GCodeSpecial.GoToHome(x: true, y: true));
            Code.addCode(GCodeSpecial.GoToHome(z: true));
            Code.addCode(GCodeSpecial.ResetToExtrude(0));
            Code.addCode(new GCodeLine(z: 15, speed: 9000, mode: GCodeLine.SpeedModes.Fast));

            int h = Grid.Rows;
            int w = Grid.Columns;
            int c = 0;

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    c++;
                    var yk = (x % 2 != 0) ?  h - y - 1 : y;

                    Console.WriteLine(x + "; " + yk);

                    var task = Tasks[x, yk];

                    if (task == null)
                        continue;


                    double cX = x * Grid.DistanceColumns + Grid.GridOffX;
                    double cY = yk * Grid.DistanceRows + Grid.GridOffY;

                    double z = Printer.Zoff;
                   
                    c = x + yk * w;

                    if (GridMap != null && GridMap.Count > c && GridMap[c] != null)
                        z = GridMap[c].Value;
                    Console.WriteLine(z);
                    task.Print(Code, cX, cY, z);
                    Code.addCode(new GCodeLine(z: 15, speed: 9000, mode: GCodeLine.SpeedModes.Fast));

                }

            Code.addCode(GCodeSpecial.GoToHome(x: true, y: true));
            Code.addCode(GCodeSpecial.FanOff());
            Code.addCode(GCodeSpecial.HeaterOff());
            Code.addCode(GCodeSpecial.StepperOff());

            Code.SaveTogCodeFile(path);
        }

        
    }
}
