using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PetriPrinter.Model
{
    [Serializable]
    public struct PrinterConfig
    {
        public double Temperature { get; set; }
        public double Zoff { get; set; }

        public PrinterConfig(double temp, double z)
        {
            Temperature = temp;
            Zoff = z;
        }
    }
}
