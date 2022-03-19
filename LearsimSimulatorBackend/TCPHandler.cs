using LearsimSimulatorBackend.DataHandlers;
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
        private DataHandler dataHandler;
        
        public override void Connect()
        {
            if (IsConnected())
            {
                return;
            }
            
            
        }

        public override Client GetClient()
        {
            return Config;
        }

        public override bool IsConnected()
        {
            return true;
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
            if(Config.CustomHandler != null)
            {
                dataHandler = (DataHandler)Activator.CreateInstance(type:Type.GetType("LearsimSimulatorBackend.DataHandlers." + Config.CustomHandler));
            }

        }
        public override void SendData(SimVarMessage[] messages)
        {
            try
            {
             
                TcpClient client = new TcpClient(Config.Adress, int.Parse(Config.Port));

            
                NetworkStream stream = client.GetStream();
                foreach (SimVarMessage item in messages)
                {
                   
                    Binding binding = Config.Bindings.Where(b => b.SimVar.Identfier.Replace('_', ' ') == item.Identfier && b.SimVar.Index == item.Index).FirstOrDefault();
                  
                    if (dataHandler != null) {
                        string DataToSend = dataHandler.HandleData(item, binding);
                        Byte[] buffer = Encoding.ASCII.GetBytes(DataToSend);
                        stream.Write(buffer, 0, buffer.Length);

                        Byte[] buf = new Byte[100];
                        stream.Read(buf, 0, 0);

                    }
                    else { 
                        string DataToSend = $"{binding.ValueName}:{binding.Type}:{item.Message.Replace(',', '.')};";
                        Byte[] buffer = Encoding.ASCII.GetBytes(DataToSend);
                        stream.Write(buffer, 0, buffer.Length);
                        buffer = new Byte[256];

                        // String to store the response ASCII representation.
                        String responseData = String.Empty;

                        // Read the first batch of the TcpServer response bytes.
                        Int32 bytes = stream.Read(buffer, 0, buffer.Length);
                        responseData = System.Text.Encoding.ASCII.GetString(buffer, 0, bytes);
                        Console.WriteLine("Received: {0}", responseData);

                    }
                }
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

    }
}
