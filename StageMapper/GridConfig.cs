using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StageMapper
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
        public double GridOffZ { get; set; }

        public double startZ { get; set; }
    }
}
