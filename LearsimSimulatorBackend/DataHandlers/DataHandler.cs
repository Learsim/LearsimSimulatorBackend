using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend.DataHandlers
{
    /// <summary>
    /// Datahandler
    /// </summary>
    public abstract class DataHandler
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract string HandleData(SimVarMessage message,Binding binding);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bindings"></param>
        public  DataHandler() { 
        }
    }
}
