using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCodeAPI
{
    public interface IGCode
    {
        /// <summary>
        /// Returns the name of IGCode
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns a deep copy of the object.
        /// </summary>
        /// <returns></returns>
        IGCode Duplicate();
    }
}
