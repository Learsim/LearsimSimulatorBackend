using System;
using System.IO.Ports;
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
        public override Client GetClient()
        {
            return Config;
        }
        public override bool IsConnected() { return Serial.IsOpen; }
        public bool IsOpen { get { return Serial.IsOpen; } }
        public SerialHandler(Client clientconfig)
        {
            BaudRate = clientconfig.Baud;
            PortName = clientconfig.Adress;
            Config = clientconfig;
            Serial = new SerialPort(PortName, BaudRate);
            Serial.DataReceived += RevicedData;
            OpenConnection();
        }
        private void RevicedData(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            string data = serialPort.ReadExisting();
            Buffer += data;
            if (Buffer.Contains(StopChar))
            {
                MessageEventArgs DataToSend = new MessageEventArgs();
                DataToSend.Message = new Tuple<SimVariable, string>(new SimVariable(), Buffer.Replace(StopChar, String.Empty).Replace("\n", ""));
                OnMessage(DataToSend);
                Buffer = "";
            }

        }

        public override void SendData(SimVarMessage[] messages)
        {
            string DataToSend = JsonConvert.SerializeObject(messages);
            Serial.WriteLine(DataToSend);
        }

        void OpenConnection()
        {
            try
            {
                Serial.Open();
            }
            catch
            {
                Console.WriteLine($"Failed to open {PortName}");
            }
        }
        void CloseConnection()
        {
            Serial.Close();
        }
    }
}
