using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PetriPrinter.Model
{
    [Serializable]
    public struct GridConfig
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public double DistanceRows { get; set; }
        public double DistanceColumns { get; set; }

        public double GridOffX { get; set; }
        public double GridOffY { get; set; }

        public GridConfig(int row, int columns, double distanceRows, double distanceColumns, double gridOffX, double gridOffY)
        {
            Rows = row;
            Columns = columns;
            DistanceRows = distanceRows;
            DistanceColumns = distanceColumns;
            GridOffX = gridOffX;
            GridOffY = gridOffY;
        }
    }
}
