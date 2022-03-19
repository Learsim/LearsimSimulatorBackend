using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{
    /// <summary>
    /// The class for the client containg configuration, needs refreactor
    /// </summary>
    public class Client
    {
        /// <summary>
        /// The connecteiontype, a enum
        /// </summary>
        public ConnectionType ConnectionType { get; set; }
        /// <summary>
        /// Guid of the client, maybe should use GUID class instead of string
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// The name of the client
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Adress of the client, ("COM10, 192.168.1.3")
        /// </summary>
        public string Adress { get; set; }
        /// <summary>
        /// If the SerialPort is static, that it is set and not floating. Floating: it can be plugged in any port.
        /// </summary>
        public bool StaticPort { get; set; }
        /// <summary>
        /// A small summary of the function of the client function.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// If Serial this is the baudrate it is communicating in
        /// </summary>
        public int Baud { get; set; }
        /// <summary>
        /// This is the port if it is a TCP client e.t.c.
        /// </summary>
        
        public string Port { get; set; }
        /// <summary>
        /// The bindings it is subscribing to 
        /// </summary>
        public Binding[] Bindings { get; set; }
        /// <summary>
        /// The inputs it have
        /// </summary>
        public Input[] Inputs { get; set; }

        /// <summary>
        /// If the client should have a custom data handler
        /// </summary>
        public string CustomHandler { get; set; }
    }
    /// <summary>
    /// A binding for the client, that the connectionhandler sends to the client
    /// </summary>
    public class Binding
    {
        /// <summary>
        /// The name of the value (maybe not to be used)
        /// </summary>
        public string ValueName { get; set; }
        /// <summary>
        /// The type of value, (Bool,String,Number(int,enum,float,double))
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// If the binding also can be used as a input (Not sure)
        /// </summary>
        public bool Input { get; set; }
        /// <summary>
        /// The SimVar that the binding is bound to
        /// </summary>
        public SimVarBinding SimVar { get; set; }
        /// <summary>
        /// The rate that it should be sent to the client, e.g. the time is not necessary to send more then 1 time every second.
        /// </summary>
        public double UpdateRate { get; set; } = 1;
        /// <summary>
        /// The DateTime of the last sent message
        /// </summary>
        public DateTime LastSend { get; set; } = DateTime.Now;
    }
    /// <summary>
    /// SimVar binding for the client
    /// </summary>
    public class SimVarBinding
    {
        /// <summary>
        /// Construnctor for SimVarBinging
        /// </summary>
        /// <param name="identfier"></param>
        /// <param name="index"></param>
        public SimVarBinding(string identfier, int index)
        {
            Identfier = identfier;
            Index = index;
        }
        /// <summary>
        /// The SimVar Identfier
        /// </summary>
        public string Identfier { get; set; }
        /// <summary>
        /// Index should be nullable?
        /// </summary>
        public int Index { get; set; }


    }
    /// <summary>
    /// A message containing a SimVar and a value 
    /// </summary>
    public class SimVarMessage
    {
        /// <summary>
        /// Constructor for SimVarMessage
        /// </summary>
        /// <param name="identfier"></param>
        /// <param name="index"></param>
        /// <param name="message"></param>
        public SimVarMessage(string identfier, int index, string message)
        {
            Identfier = identfier;
            Index = index;
            Message = message;
        }
        /// <summary>
        /// The SimVar Identfier
        /// </summary>
        public string Identfier { get; set; }
        /// <summary>
        /// Index should be nullable?
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// The value of the message, always string. Then casted on the client
        /// </summary>
        public string Message { get; set; }


    }
    /// <summary>
    /// Input for the client
    /// </summary>
    public class Input
    {
        /// <summary>
        /// The key for the event to call
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Identifer, maybe should be removed
        /// </summary>
        public string Identfier { get; set; }
        /// <summary>
        /// Type, (Bool,String,Number(Int,Float,Double))
        /// </summary>
        public int Type { get; set; }
    }

}
