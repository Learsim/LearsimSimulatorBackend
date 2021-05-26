using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace LearsimSimulatorBackend
{
    public class Application
    {

        SimConnectHandler simConnect;
        API api;
        List<Connection> ClientConnections = new List<Connection>();
        Dictionary<string, string> ValueCache;

        public void run()
        {
            Configuration configuration = LoadConfig();
            InitClients(configuration);
            simConnect = new SimConnectHandler();
            simConnect.ValueRecived += SimConnect_ValueRecived;
            simConnect.Connect();
            simConnect.AddRequest(SimVars.ADF_EXT_FREQUENCY, 0);
            simConnect.InitConfig(configuration);
            api = new API(configuration, simConnect, ClientConnections);

        }

        private void InitClients(Configuration configuration)
        {

            foreach (Client client in configuration.Clients.Where(x => x.ConnectionType == ConnectionType.SERIAL))
            {
                if (client.StaticPort)
                {
                    ClientConnections.Add(new SerialHandler(client));
                }
            }
            Console.WriteLine(ClientConnections);

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
