using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{
    /// <summary>
    /// Holds the configuration for the server
    /// </summary>
    public class Configuration
    {

        /// <summary>
        /// The port of the http API
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// The rate it should pull from SimConnect in ms
        /// </summary>
        public int PullingRate { get; set; }
        /// <summary>
        /// The hostname of the server, this is important if ran in not admin mod or if it should only listen on specified adress
        /// </summary>
        public string Hostname { get; set; }
        /// <summary>
        /// All the clients that is configured
        /// </summary>
        public List<Client> Clients { get; set; }
        /// <summary>
        /// Values that the server itself is subscribing to, could be for the UI e.t.c.
        /// </summary>
        public List<SimVarBinding> StandaloneValues { get; set; }
        /// <summary>
        /// The Arduino configurations for clients created in the UI.
        /// </summary>
        public List<ArduinoConfiguration> ArduinoConfigurations { get; set; }

        /// <summary>
        /// Sets the port for the Server
        /// </summary>
        /// <param name="port"></param>
        public void SetPort(int port)
        {
            Port = port;
            SaveConfig();
        }

        /// <summary>
        /// Sets the hostname for the Server
        /// </summary>
        /// <param name="hostname"></param>
        public void SetHostname(string hostname)
        {
            Hostname = hostname;
            SaveConfig();
        }

        /// <summary>
        /// Updates a client
        /// </summary>
        /// <param name="guid"></param>
        public void UpdateClient(Guid guid)
        {

            SaveConfig();
        }

        /// <summary>
        /// Saves the config to %appdata%\Learsim\LearsimServer\Config.json
        /// </summary>
        public void SaveConfig()
        {
            File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Learsim\\LearsimServer\\Config.json", JsonConvert.SerializeObject(this));
        }

        /// <summary>
        /// Removes a client
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveClient(Guid guid)
        {
            SaveConfig();
        }
    }
}
