using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
namespace LearsimSimulatorBackend
{
    class SerialHandler : Connection
    {
        int BaudRate;
        string PortName;
        private SerialPort Serial;
        string StopChar = "`";
        string Buffer = "";
        public Client Config;
        bool LookingForPort = false;
        public override Client GetClient()
        {
            return Config;
        }
        public override bool IsConnected() { return Serial.IsOpen && !LookingForPort; }
        public bool IsOpen { get { return Serial.IsOpen && !LookingForPort; } }
        public SerialHandler(Client clientconfig)
        {
            BaudRate = clientconfig.Baud;
            PortName = clientconfig.Adress;
            Config = clientconfig;
            Serial = new SerialPort(PortName, BaudRate);
            Serial.DataReceived += RevicedData;
            if (Config.StaticPort)
                OpenConnection();
        }
        private void RevicedData(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            string data = serialPort.ReadExisting();
            Buffer += data;
            if (Buffer.Contains('\u0018') && !LookingForPort)
            {

                MessageEventArgs DataToSend = new MessageEventArgs();
                DataToSend.Message = new Tuple<SimVariable, string>(new SimVariable(), Buffer.Replace(StopChar, String.Empty).Replace("\n", ""));
                OnMessage(DataToSend);
                Buffer = "";
            }

        }

        internal bool FindSerial()
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string portName in ports)
            {
                try
                {
                    LookingForPort = true;

                    PortName = portName;
                    Serial = new SerialPort(PortName, BaudRate);
                    Serial.DataReceived += RevicedData;

                    OpenConnection();
                    Serial.Write("00:4:GETID;");

                    DateTime sentTime = DateTime.Now;
                    while (!Buffer.Contains(StopChar) && sentTime.Ticks + 15000000 > DateTime.Now.Ticks) ;
                    Console.WriteLine(Buffer);
                    string id = Buffer.Split('\u0018')[0];
                    if (id == Config.Guid)
                    {
                        Config.Adress = portName;
                        LookingForPort = false;
                        Buffer = "";
                        return true;
                    }
                }
                catch (Exception e)
                {
                    CloseConnection();
                    Buffer = "";
                    LookingForPort = false;
                }
            }
            Console.WriteLine($"Failed to find port for {Config.Name}");
            CloseConnection();
            Buffer = "";
            LookingForPort = false;
            return false;

        }

        public override void SendData(SimVarMessage[] messages)
        {
            foreach (SimVarMessage item in messages)
            {
                Binding binding  = Config.Bindings.Where(b=> b.SimVar.Identfier.Replace('_',' ') == item.Identfier && b.SimVar.Index == item.Index).FirstOrDefault();
                if(item.Message.Length > 4)
                {
                    item.Message = item.Message.Substring(0, 4);
                }
                string DataToSend = $"{binding.ValueName}:{binding.Type}:{item.Message.Replace(',','.')};";

                Serial.WriteLine(DataToSend);
                Console.WriteLine(DataToSend);
            }
            
        }

        void OpenConnection()
        {
            try
            {
                Serial.Open();

            }
            catch (Exception e)
            {
                //Console.WriteLine($"Could not open {PortName}");
            }
        }
        void CloseConnection()
        {
            Serial.Close();
        }
        public override void Connect()
        {
            OpenConnection();
        }
       
    }
}
