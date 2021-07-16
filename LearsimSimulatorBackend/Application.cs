using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
namespace LearsimSimulatorBackend
{ /// <summary>
  /// This is the main application class
  /// </summary>
    public class Application
    {
        SimConnectHandler simConnect;
        API api;
        List<Connection> ClientConnections = new List<Connection>();
        List<Node> Nodes = new List<Node>() { new Node("192.168.1.4","Captian Server","13253"), new Node("192.168.1.5", "Outside Server", "13253") };
        Dictionary<string, string> ValueCache;
        ///<summary>
        ///This function runs the main thread of the process
        ///</summary>
        public void run()
        {
            Configuration configuration = LoadConfig();
            simConnect = new SimConnectHandler();
            simConnect.ValueRecived += SimConnect_ValueRecived;
            api = new API(configuration, simConnect, ClientConnections, Nodes);
          
            InitClients(configuration);

            
         
            while (true)
            {
                if (!simConnect.Connected)
                {
                    try
                    {
                        simConnect.Connect();

                    }
                    catch (Exception e)
                    {
                        if(e.GetType() == typeof(COMException))
                        {
                            Console.WriteLine("Is FS2020 On?");
                        }
                    }

                    if (simConnect.Connected)
                    {
                        simConnect.InitConfig(configuration);
                    }
                };
                foreach (Connection connection in ClientConnections.Where(c => c.IsConnected() == false))
                {
                    if (typeof(SerialHandler) == connection.GetType())
                    {
                        if (connection.GetClient().StaticPort)
                        {
                            connection.Connect();
                        }
                        else
                        {
                            SerialHandler serialHandler = (SerialHandler)connection;
                            if (!serialHandler.FindSerial())
                            {
                                serialHandler.Config.Adress = "COM1";

                            }

                        }
                    }
                }
                Thread.Sleep(2500);
            }

        }

        private void InitClients(Configuration configuration)
        {

            foreach (Client client in configuration.Clients.Where(x => x.ConnectionType == ConnectionType.SERIAL))
            {
                if (client.StaticPort)
                {
                    ClientConnections.Add(new SerialHandler(client));
                }
                else
                {
                    SerialHandler serialHandler = new SerialHandler(client);
                    if (!serialHandler.FindSerial())
                    {
                        serialHandler.Config.Adress = "COM1";

                    }
                    else
                    {
                        Console.WriteLine("Hello World Arduino from port: " + serialHandler.Config.Adress);

                    }
                    ClientConnections.Add(serialHandler);



                }
            }

        }

        private void GotMessage(object sender, EventArgs e)
        {
            Console.WriteLine(((MessageEventArgs)e).Message.Item2);
        }

        private Configuration LoadConfig()
        {
            string configText = File.ReadAllText("Config.json");
            Configuration configuration;
            try
            {
                configuration = JsonConvert.DeserializeObject<Configuration>(configText);
            }
            catch
            {
                Console.WriteLine("Could not load config");
                throw;
            }
            return configuration;
        }

        private void SimConnect_ValueRecived(object sender, Dictionary<string, string> e)
        {
            NotifyClients(e);
            UpdateAPICache(e.ToList());
        }
        ///<summary>
        ///Updates the cache for the API, so it does not access values from main class
        ///</summary>
        public void UpdateAPICache(List<KeyValuePair<string, string>> lists)
        {
            if (api != null)
            {
                api.UpdateCache(lists);
            }
        }

        private void NotifyClients(Dictionary<string, string> values)
        {

            Dictionary<string, string> ValuesToUpdate = new Dictionary<string, string>();

            if (ValueCache == null)
            {
                ValueCache = new Dictionary<string, string>(values);
                ValuesToUpdate = values;
            }
            else
            {
                foreach (var item in values)
                {
                    string value;
                    ValueCache.TryGetValue(item.Key, out value);
                    if (value != item.Value)
                    {
                        ValuesToUpdate[item.Key] = item.Value;
                    }
                }


            }
            //Console.Clear();
            //Console.WriteLine("\\\\\\\\\\\\\\\\\\\\\\\\");
            foreach (var item in ValuesToUpdate)
            {
                ValueCache[item.Key] = item.Value;
                //Console.WriteLine(item.Key + " : " + item.Value);
            }
            //Console.WriteLine("\\\\\\\\\\\\\\\\\\\\\\\\");
            foreach (var client in ClientConnections.Where(c => c.IsConnected() == true))
            {
                List<SimVarMessage> DataToSend = new List<SimVarMessage>();
                foreach (var item in ValuesToUpdate)
                {
                    Client c = client.GetClient();
                    if (c.Bindings.Where(v => v.SimVar.Identfier.Replace("_", " ").ToLower() == item.Key.Split(':')[0].ToLower()).Count() > 0)
                    {
                        foreach (var simvar in c.Bindings.Where(v => v.SimVar.Identfier.Replace("_", " ").ToLower() == item.Key.Split(':')[0].ToLower()))
                        {
                            var x = item.Key.Split(':')[0].Count();
                            if (simvar.SimVar.Index == 0 && item.Key.Split(':').Count() <= 1)
                            {
                                DataToSend.Add(new SimVarMessage(item.Key.Split(':')[0], 0, item.Value));
                            }
                            else
                            {
                                DataToSend.Add(new SimVarMessage(item.Key.Split(':')[0], Convert.ToInt32(item.Key.Split(':')[1]), item.Value));

                            }
                        }
                    }
                }
                client.SendData(DataToSend.ToArray());
            }

        }
    }
}
