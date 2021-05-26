using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{

    public class Client
    {
        public ConnectionType ConnectionType { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Adress { get; set; }
        public bool StaticPort { get; set; }
        public string Description { get; set; }
        public int Baud { get; set; }
        public object Port { get; set; }
        public Binding[] Bindings { get; set; }
        public Input[] Inputs { get; set; }
    }

    public class Binding
    {
        public string ValueName { get; set; }
        public int Type { get; set; }
        public bool Input { get; set; }
        public SimVarBinding SimVar { get; set; }
        public int UpdateRate { get; set; } = 1;
    }
    public class SimVarBinding
    {
        public SimVarBinding(string identfier, int index)
        {
            Identfier = identfier;
            Index = index;
        }

        public string Identfier { get; set; }
        public int Index { get; set; }


    }
    public class SimVarMessage
    {
        public SimVarMessage(string identfier, int index, string message)
        {
            Identfier = identfier;
            Index = index;
            Message = message;
        }

        public string Identfier { get; set; }
        public int Index { get; set; }
        public string Message { get; set; }


    }

    public class Input
    {
        public string Key { get; set; }
        public string Identfier { get; set; }
        public int Type { get; set; }
    }

}
