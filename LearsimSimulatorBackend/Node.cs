using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{
    class Node
    {
        public string IP;
        public string Name;
        public string Port;
        private bool connected = false;

        public bool Connected
        {
            get { return connected; }
            set { connected = value; }
        }

        public Node(string iP, string name, string port)
        {
            IP = iP;
            Name = name;
            Port = port;
        }

    }
}
