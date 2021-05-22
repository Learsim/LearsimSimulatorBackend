using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{

    public class Configuration
    {


        public int Port { get; set; }
        public int PullingRate { get; set; }
        public string Hostname { get; set; }
        public Client[] Clients { get; set; }
        public SimVarBinding[] StandaloneValues { get; set; }
    
        public void SetPort(int port)
        {
            Port = port;
            SaveConfig();
        }
        public void SetHostname(string hostname)
        {
            Hostname = hostname;
            SaveConfig();
        }
        public void UpdateClient(Guid guid)
        {

            SaveConfig();
        }
        public void SaveConfig()
        {
            File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Learsim\\LearsimServer\\Config.json", JsonConvert.SerializeObject(this));
        }

        public void RemoveClient(Guid guid)
        {
            SaveConfig();
        }
    }
}
