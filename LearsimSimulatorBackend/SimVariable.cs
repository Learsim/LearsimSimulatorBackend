using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{
    /// <summary>
    /// A feature class for handeling more advanced SimVar specification 
    /// </summary>
    public class SimVariable
    {
        /// <summary>
        /// Collection of all available SimVars
        /// </summary>
        public SimVar[] SimVars { get; set; }
    }
    /// <summary>
    /// A SimVar
    /// </summary>
    public class SimVar
    {
        /// <summary>
        /// The Identifier of the SimVar
        /// </summary>
        public string SimulationVariable { get; set; }
        /// <summary>
        /// The unit of the SimVar, Feet, Meter, e.t.c.
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// If it can be set or not
        /// </summary>
        public bool Settable { get; set; }
        /// <summary>
        /// If it can be indexed.
        /// </summary>
        public bool Indexable { get; set; }
    }

}
