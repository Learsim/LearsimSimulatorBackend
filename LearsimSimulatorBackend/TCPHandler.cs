using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{
    class TCPHandler : Connection
    {
        private Int32 Port;
        private IPAddress Adress;
        private TcpClient tcpClient;
        private Client Config;
        
        public override void Connect()
        {
            if (IsConnected())
            {
                return;
            }
            
            IPEndPoint endPoint = new IPEndPoint(Adress, Port);
            tcpClient = new TcpClient("192.168.0.165", 12511);
            Console.WriteLine($"Open connection to {Config.Name}");
            SimVarMessage[] data = { new SimVarMessage("GENERAL ENG THROTTLE LEVER POSITION", 1, "test") };
            SendData(data);
        }

        public override Client GetClient()
        {
            return Config;
        }

        public override bool IsConnected()
        {
            if (tcpClient != null) {
                return tcpClient.Connected;
            }
            return false;
        }
        public bool IsOpen()
        {
            return IsConnected();
        }
        public TCPHandler(Client clientconfig)
        {
            Int32.TryParse(clientconfig.Port,out Port);
            Adress = IPAddress.Parse(clientconfig.Adress);
            Config = clientconfig;

        }
        public override void SendData(SimVarMessage[] messages)
        {
            if(tcpClient.Connected)
            {
                Stream stream = tcpClient.GetStream();
                foreach (SimVarMessage item in messages)
                {
                    Binding binding = Config.Bindings.Where(b => b.SimVar.Identfier.Replace('_', ' ') == item.Identfier && b.SimVar.Index == item.Index).FirstOrDefault();
                    if (item.Message.Length > 4)
                    {
                        item.Message = item.Message.Substring(0, 4);
                    }
                    string DataToSend = $"{binding.ValueName}:{binding.Type}:{item.Message.Replace(',', '.')};";
                    Byte[] buffer = Encoding.ASCII.GetBytes(DataToSend);
                    stream.Write(buffer,0,buffer.Length);
                }
                stream.Close();
            }
        }

    }
}
