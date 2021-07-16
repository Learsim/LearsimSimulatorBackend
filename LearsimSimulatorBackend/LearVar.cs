using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{
    /// <summary>
    /// This is a variable that can be read and changed by clients, nothing to do with the simulator. For example light sensors in the cockpit or temrapure etc.
    /// </summary>
    class LearVar
    {
        private string m_identifier;

        public string Identifier
        {
            get { return m_identifier; }
            set { m_identifier = value; }
        }
        private string m_value;

        public string Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public LearVar(string identifier, string value)
        {
            m_identifier = identifier;
            m_value = value;
        }
    }
}
