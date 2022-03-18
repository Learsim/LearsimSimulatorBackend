using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Makaretu.Dns;
using System.Diagnostics;

namespace LearsimSimulatorBackend
{
    class API
    {
        Configuration Configuration;
        SimConnectHandler SimConnection;
        List<Connection> connections;
        List<Node> Nodes;
        public static Dictionary<SimVarBinding, string> CachedData = new Dictionary<SimVarBinding, string>();
        Thread Server;
        public API(Configuration configuration, SimConnectHandler simConnect, List<Connection> clientConnections, List<Node> nodes)
        {
            connections = clientConnections;
            SimConnection = simConnect;
            Configuration = configuration;
            Nodes = nodes;
            Server = new Thread(new ThreadStart(StartServer));
            Server.Start();
        }



        public void UpdateCache(List<KeyValuePair<string, string>> values)
        {
            Dictionary<SimVarBinding, string> newChache = new Dictionary<SimVarBinding, string>();
            foreach (var value in values)
            {
                string[] Value = value.Key.Split(':');
                int index = 0;
                if (Value.Length > 1)
                {
                    Int32.TryParse(Value[1], out index);
                }
                newChache[new SimVarBinding(Value[0], index)] = value.Value;
            }
            CachedData = newChache;
        }
        public void StartServer()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://{Configuration.Hostname}:{Configuration.Port}/");
            Console.WriteLine($"Server Started at http://{ Configuration.Hostname}:{Configuration.Port}/");
            listener.Start();
            var service = new ServiceProfile("learsim.local", "learsim._api", (ushort)Configuration.Port);
            var sd = new ServiceDiscovery();
            sd.Advertise(service);

            while (true)
            {


                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                string responseString = "";
                //Console.WriteLine(request.HttpMethod);
                //Console.WriteLine(request.RawUrl);
                //Console.WriteLine(request.UserAgent);
                //Console.WriteLine(request.RemoteEndPoint);

                if (request.HttpMethod == "GET")
                {
                    if (request.RawUrl.StartsWith("/api/clients"))
                    {
                        responseString += JsonConvert.SerializeObject(connections);
                        Console.WriteLine(responseString);
                    }
                    else if (request.RawUrl.StartsWith("/api/startSim"))
                    {
                        Process.Start(@"shell:AppsFolder\Microsoft.FlightSimulator_8wekyb3d8bbwe!App");
                    }
                    else if (request.RawUrl.StartsWith("/api/status"))
                    {
                        responseString = "{\"SimConnection\":" + SimConnection.Connected.ToString().ToLower() + "}";

                    }
                    else if (request.RawUrl.StartsWith("/api/config"))
                    {
                        responseString = JsonConvert.SerializeObject(Configuration);

                    }
                    else if (request.RawUrl.StartsWith("/api/nodes"))
                    {
                        responseString = JsonConvert.SerializeObject(Nodes);

                    }
                    else if (request.RawUrl.StartsWith("/api/getEnums"))
                    {
                        responseString += JsonConvert.SerializeObject(Enum.GetNames(typeof(SimVars)));


                    }
                    else if (request.RawUrl.StartsWith("/api/getValues"))
                    {
                        responseString += "{\"SimVars\":";
                        responseString += JsonConvert.SerializeObject(CachedData.ToList());
                        responseString += $",\"LearVars\":";
                        responseString += JsonConvert.SerializeObject(new LearVar[] { new LearVar("Test", "Test"), new LearVar("Test", "0") });
                        responseString += "}";




                    }
                    else
                    {
                        responseString = File.ReadAllText("index.html");
                    }
                }
                else if (request.HttpMethod == "POST")
                {

                    if (request.RawUrl.StartsWith("/api/client"))
                    {
                        try
                        {

                            Client client = new Client();
                            client.Description = request.QueryString.Get("Description");
                            client.Name = request.QueryString.Get("Name");
                            client.Baud = Convert.ToInt32(request.QueryString.Get("Baud"));
                            client.Guid = request.QueryString.Get("Guid");
                            client.StaticPort = Convert.ToBoolean(request.QueryString.Get("StaticPort"));
                            client.ConnectionType = (ConnectionType)Convert.ToInt32(request.QueryString.Get("ConnectionType"));
                            client.Adress = request.QueryString.Get("Adress");
                            client.Bindings = JsonConvert.DeserializeObject<Binding[]>(request.QueryString.Get("Bindings"));
                            client.Inputs = JsonConvert.DeserializeObject<Input[]>(request.QueryString.Get("Inputs"));
                            responseString = JsonConvert.SerializeObject(client);
                            ArduinoConfiguration arduinoConfiguration;
                            arduinoConfiguration = JsonConvert.DeserializeObject<ArduinoConfiguration>(request.QueryString.Get("ArduinoBinings"));
                            Configuration.Clients.Add(client);
                            if (arduinoConfiguration.ArduinoBindings != null)
                                Configuration.ArduinoConfigurations.Add(arduinoConfiguration);
                            Configuration.SaveConfig();
                            response.StatusCode = 201;

                        }
                        catch (Exception e)
                        {
                            responseString = e.ToString();
                            response.StatusCode = 304;
                        }
                    }
                    else if (request.RawUrl.StartsWith("/api/simconnect/connect"))
                    {
                        SimConnection.Connect();
                        responseString = JsonConvert.SerializeObject(SimConnection.Connected);
                        response.StatusCode = SimConnection.Connected ? 200 : 400;
                    }
                    else if (request.RawUrl.StartsWith("/api/simconnect/disconnect"))
                    {
                        SimConnection.Disconnect();
                        response.StatusCode = !SimConnection.Connected ? 200 : 400;
                    }

                }
                else if (request.HttpMethod == "DELETE")
                {
                    response.StatusCode = 304;
                    if (request.RawUrl.StartsWith("/api/clients"))
                    {
                        Configuration.RemoveClient(Guid.Parse(request.Headers.Get("guid")));
                    }

                }
                else if (request.HttpMethod == "PUT")
                {

                }
                else if (request.HttpMethod == "PATCH")
                {
                    response.StatusCode = 304;
                    if (request.RawUrl.StartsWith("/api/clients"))
                    {
                        if (request.Headers.Keys.ToString().Contains("guid"))
                        {
                            if (request.Headers.Keys.ToString().Contains("Name"))
                            {
                            }
                            if (request.Headers.Keys.ToString().Contains("ConnectionType"))
                            {
                                int value;
                                Int32.TryParse(request.Headers.Get("ConnectionType"), out value);

                            }
                            if (request.Headers.Keys.ToString().Contains("Adress"))
                            {
                            }
                            if (request.Headers.Keys.ToString().Contains("StaticPort"))
                            {
                            }
                            if (request.Headers.Keys.ToString().Contains("Baud"))
                            {
                                int value;
                                Int32.TryParse(request.Headers.Get("Baud"), out value);
                            }
                            if (request.Headers.Keys.ToString().Contains("Port"))
                            {
                            }
                        }

                    }
                    else if (request.RawUrl.StartsWith("/api/config"))
                    {
                        if (request.Headers.Get("Port") != null)
                        {
                            int port = Configuration.Port;
                            Int32.TryParse(request.Headers.Get("Port"), out port);
                            Configuration.SetPort(port);

                        }
                        if (request.Headers.Get("Hostname") != null)
                        {
                            Configuration.SetHostname(request.Headers.Get("Hostname"));

                        }
                        responseString = JsonConvert.SerializeObject(Configuration);

                    }

                }
                response.AppendHeader("Access-Control-Allow-Origin", "*");

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();

                GC.Collect();

            }
        }
    }
}
