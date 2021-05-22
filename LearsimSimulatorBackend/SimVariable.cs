using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{
    
    public class SimVariable
    {
        public SimVar[] SimVars { get; set; }
    }

    public class SimVar
    {
        public string SimulationVariable { get; set; }
        public string Unit { get; set; }
        public bool Settable { get; set; }
        public bool Indexable { get; set; }
    }

}
