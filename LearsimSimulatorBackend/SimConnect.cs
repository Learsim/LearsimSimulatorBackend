using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LearsimSimulatorBackend
{

    public class SimConnectHandler : ObservableObject
    {

        public event EventHandler<Dictionary<string, string>> ValueRecived;
        public int PullingRate = 1000;
        public SimvarRequest oSelectedSimvarRequest
        {
            get { return m_oSelectedSimvarRequest; }
            set { this.SetProperty(ref m_oSelectedSimvarRequest, value); }
        }

        Dictionary<string, string> valueCache;

        private SimvarRequest m_oSelectedSimvarRequest = null;
        public string sUnitRequest
        {
            get { return m_sUnitRequest; }
            set { this.SetProperty(ref m_sUnitRequest, value); }
        }
        private string m_sUnitRequest = null;
        public string sSimvarRequest
        {
            get { return m_sSimvarRequest; }
            set { this.SetProperty(ref m_sSimvarRequest, value); }
        }
        private string m_sSimvarRequest = null;

        public uint iIndexRequest
        {
            get { return m_iIndexRequest; }
            set { this.SetProperty(ref m_iIndexRequest, value); }
        }
        private uint m_iIndexRequest = 0;

        public uint iObjectIdRequest
        {
            get { return m_iObjectIdRequest; }
            set
            {
                this.SetProperty(ref m_iObjectIdRequest, value);
                ClearResquestsPendingState();
            }
        }
        private void ClearResquestsPendingState()
        {
            foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            {
                oSimvarRequest.bPending = false;
                oSimvarRequest.bStillPending = false;
            }
        }
        protected virtual void OnValueRecived(Dictionary<string, string> values)
        {
               Dictionary<string, string> ValuesToUpdate = new Dictionary<string, string>();

            if (valueCache == null)
            {
                valueCache = values;
            }
            foreach (var item in values)
            {
                string value;
                valueCache.TryGetValue(item.Key, out value);
                if (value != item.Value)
                {
                    ValuesToUpdate[item.Key] = item.Value;
                }
            }
            foreach (var item in ValuesToUpdate)
            {
                valueCache[item.Key] = item.Value;
            }
            var handler = ValueRecived;

            handler?.Invoke(this, valueCache);
        }

        private uint m_iObjectIdRequest = 0;

        public bool Connected = false;
        public const int WMUser = 0x0402;
        public ObservableCollection<uint> lObjectIDs { get; private set; }


        internal void InitConfig(Configuration configuration)
        {
            PullingRate = configuration.PullingRate;
            foreach (Client c in configuration.Clients)
            {
                foreach (Binding b in c.bindings)
                {
                    AddRequest((SimVars)Enum.Parse(typeof(SimVars), b.SimVar.Identfier), b.SimVar.Index);

                }
            }
            foreach (SimVarBinding item in configuration.StandaloneValues)
            {
                AddRequest((SimVars)Enum.Parse(typeof(SimVars), item.Identfier), item.Index);

            }
        }


        public bool bObjectIDSelectionEnabled
        {
            get { return m_bObjectIDSelectionEnabled; }
            set { this.SetProperty(ref m_bObjectIDSelectionEnabled, value); }
        }
        public SIMCONNECT_SIMOBJECT_TYPE eSimObjectType
        {
            get { return m_eSimObjectType; }
            set
            {
                this.SetProperty(ref m_eSimObjectType, value);
                bObjectIDSelectionEnabled = (m_eSimObjectType != SIMCONNECT_SIMOBJECT_TYPE.USER);
                ClearResquestsPendingState();
            }
        }
        private SIMCONNECT_SIMOBJECT_TYPE m_eSimObjectType = SIMCONNECT_SIMOBJECT_TYPE.USER;
        private bool m_bObjectIDSelectionEnabled = false;
        private IntPtr hWnd = new IntPtr(0);

        private SimConnect simConnect = null;
        public ObservableCollection<SimvarRequest> lSimvarRequests { get; private set; }


        private uint m_iCurrentDefinition = 0;
        private uint m_iCurrentRequest = 0;
        public void Connect()
        {
            lObjectIDs = new ObservableCollection<uint>();
            lObjectIDs.Add(1);

            lSimvarRequests = new ObservableCollection<SimvarRequest>();
            if (!Connected)
            {
                try
                {
                    simConnect = new SimConnect("Learsim Connect", hWnd, WMUser, null, 0);
                }
                catch (COMException e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }
                Connected = true;
                simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
                simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);
                simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(simconnect_OnRecvException);


                simConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(simconnect_OnRecvSimobjectDataBytype);
            }




            Thread DataThread = new Thread(requestData);
            DataThread.Start();

        }

        private void requestData()
        {
            while (Connected)
            {
                Dictionary<string, string> Values = new Dictionary<string, string>();
                simConnect.ReceiveMessage();
                //Console.Clear();

                foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                {
                    if (!oSimvarRequest.bPending)
                    {
                        simConnect?.RequestDataOnSimObjectType(oSimvarRequest.eRequest, oSimvarRequest.eDef, 0, m_eSimObjectType);
                        oSimvarRequest.bPending = true;
                        //Console.WriteLine(oSimvarRequest.sName+":"+ oSimvarRequest.dValue);
                        Values.Add(oSimvarRequest.sName, oSimvarRequest.dValue.ToString());

                    }
                    else
                    {
                        oSimvarRequest.bStillPending = true;
                    }
                }
                if (Values.Count > 0)
                {
                    OnValueRecived(Values);
                }
                System.Threading.Thread.Sleep(PullingRate);
            }
        }


        private bool RegisterToSimConnect(SimvarRequest _oSimvarRequest)
        {
            if (simConnect != null)
            {
                /// Define a data structure
                simConnect.AddToDataDefinition(_oSimvarRequest.eDef, _oSimvarRequest.sName, _oSimvarRequest.sUnits, SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                /// IMPORTANT: Register it with the simconnect managed wrapper marshaller
                /// If you skip this step, you will only receive a uint in the .dwData field.
                simConnect.RegisterDataDefineStruct<double>(_oSimvarRequest.eDef);

                return true;
            }
            else
            {
                return false;
            }
        }
        public void AddRequest(SimVars simVars, int index)
        {
            string _sOverrideSimvarRequest = simVars.ToString().Replace("_", " ") + (index.Equals(0) ? "" : ":" + index.ToString());
            string _sOverrideUnitRequest = units[Convert.ToInt32(simVars)];
            Console.WriteLine("AddRequest");

            string sNewSimvarRequest = _sOverrideSimvarRequest != null ? _sOverrideSimvarRequest : ((m_iIndexRequest == 0) ? m_sSimvarRequest : (m_sSimvarRequest + ":" + m_iIndexRequest));
            string sNewUnitRequest = _sOverrideUnitRequest != null ? _sOverrideUnitRequest : m_sUnitRequest;

            SimvarRequest oSimvarRequest = new SimvarRequest
            {
                eDef = (DEFINITION)m_iCurrentDefinition,
                eRequest = (REQUEST)m_iCurrentRequest,
                sName = sNewSimvarRequest,
                sUnits = sNewUnitRequest
            };

            oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
            oSimvarRequest.bStillPending = oSimvarRequest.bPending;

            lSimvarRequests.Add(oSimvarRequest);

            ++m_iCurrentDefinition;
            ++m_iCurrentRequest;
        }

        private void RemoveSelectedRequest()
        {
            lSimvarRequests.Remove(oSelectedSimvarRequest);
        }
        void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("Connected to FS2020");
        }

        // The case where the user closes Prepar3D
        void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("Exited FS2020");

            Disconnect();
        }

        void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            Console.WriteLine("Exception received: " + data.dwException);
        }
        public void Disconnect()
        {
            if (simConnect != null)
            {
                simConnect.Dispose();
                simConnect = null;
                Connected = false;
            }
        }
        enum DATA_REQUESTS
        {
            REQUEST_1,
        };
        void simconnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {


            uint iRequest = data.dwRequestID;
            uint iObject = data.dwObjectID;
            if (!lObjectIDs.Contains(iObject))
            {
                lObjectIDs.Add(iObject);
            }
            foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            {
                if (iRequest == (uint)oSimvarRequest.eRequest && (!bObjectIDSelectionEnabled || iObject == m_iObjectIdRequest))
                {
                    double dValue = (double)data.dwData[0];
                    oSimvarRequest.dValue = dValue;
                    oSimvarRequest.bPending = false;
                    oSimvarRequest.bStillPending = false;
                }
            }
        }

        public string[] units = {"Bool",
"Feet per second",
"Percent over 100",
"Number",
"Bool",
"Percent over 100",
"Bool",
"Percent over 100",
"Number",
"Number",
"Bool",
"Gallons",
"Gallons",
"Bool",
"Gallons",
"Gallons",
"String",
"Number",
"Bool/String",
"Bool",
"Feet",
"Number",
"Percent over 100",
"Bool",
"Bool",
"Bool",
"SIMCONNECT_DATA_XYZ",
"Percent over 100",
"Mask",
"Mask",
"SIMCONNECT_DATA_XYZ",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Knots",
"SIMCONNECT_DATA_WAYPOINT",
"Number",
"Degrees",
"Seconds",
"Knots",
"Knots",
"Boolean",
"String",
"String",
"String",
"String",
"String",
"String",
"Seconds",
"Seconds",
"String",
"Number",
"Percent over 100",
"Bool",
"SIMCONNECT_DATA_LATLONALT",
"SIMCONNECT_DATA_LATLONALT",
"SIMCONNECT_DATA_LATLONALT",
"SIMCONNECT_DATA_LATLONALT",
"SIMCONNECT_DATA_LATLONALT",
"SIMCONNECT_DATA_LATLONALT",
"SIMCONNECT_DATA_LATLONALT",
"SIMCONNECT_DATA_LATLONALT",
"SIMCONNECT_DATA_LATLONALT",
"SIMCONNECT_DATA_XYZ",
"SIMCONNECT_DATA_XYZ",
"SIMCONNECT_DATA_XYZ",
"SIMCONNECT_DATA_XYZ",
"SIMCONNECT_DATA_XYZ",
"SIMCONNECT_DATA_XYZ",
"SIMCONNECT_DATA_XYZ",
"SIMCONNECT_DATA_XYZ",
"SIMCONNECT_DATA_XYZ",
"SIMCONNECT_DATA_XYZ",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Number",
"Mask",
"Percent",
"Enum",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Rpm",
"Percent",
"Rpm",
"Percent",
"Percent",
"Percent",
"Bool",
"Rankine",
"Psi",
"Percent",
"Percent",
"Percent",
"Rankine",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Psi",
"Hours",
"Percent",
"Bool",
"Psi",
"Position",
"Percent",
"Bool",
"Bool",
"ft lb per second",
"Foot pound",
"Bool",
"Bool",
"Hours",
"Percent",
"Celsius",
"Celsius",
"Celsius",
"Bool",
"Pounds per hour",
"Enum",
"Mask",
"Number",
"Celsius",
"Ratio",
"Percent",
"Percent",
"Percent",
"Percent",
"Pounds per hour",
"Percent",
"Ratio",
"Rankine",
"Bool",
"Pounds",
"Psi",
"Enum",
"Mask",
"Number",
"Pounds per hour",
"Bool",
"Percent",
"Number",
"Number",
"Percent",
"Bool",
"Pounds per hour",
"Rpm",
"Percent",
"Pounds",
"Radians",
"Bool",
"Bool",
"Position",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Rpm",
"Rpm",
"",
"Pounds per hour",
"Foot pounds",
"Bool",
"Ratio (0-16384)",
"Rankine",
"Percent over 100",
"Rankine",
"Rankine",
"pound-force per square inch",
"Percent over 100",
"pound-force per square inch",
"Percent over 100",
"inHg",
"Number",
"Number",
"Celsius",
"Percent",
"PSI",
"Percent",
"PSI",
"Celsius",
"Percent",
"Rpm",
"Bool",
"Pounds",
"Percentage",
"Percentage",
"Percentage",
"Percentage",
"Percent over 100",
"Bool",
"Bool",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Gallons",
"Pounds",
"Enum",
"Enum",
"Gallons",
"Percent Over 100",
"Gallons",
"Pounds",
"Number",
"Bool",
"Pounds per hour",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Knots",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet per second squared",
"Feet per second squared",
"Feet per second squared",
"Feet per second squared",
"Feet per second squared",
"Feet per second squared",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet",
"Radians",
"Radians",
"Feet",
"Radians",
"Radians",
"Radians",
"Radians",
"Degrees",
"Meters",
"Enum",
"Bool",
"Radians",
"Radians",
"Knots",
"Knots",
"Degrees",
"Knots",
"Mach",
"Feet per second",
"Mach",
"Bool",
"Bool",
"Mach",
"Feet",
"Millibars",
"inHg",
"Radians",
"Radians",
"Percent Over 100",
"Bool",
"Degrees",
"Radians",
"Radians",
"Radians",
"Radians per second",
"Position",
"Radians",
"Feet",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Enum",
"Gforce",
"Gforce",
"inHg",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Frequency BCD16",
"Frequency BCD16",
"Enum",
"Bool",
"MHz",
"MHz",
"Number",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Degrees",
"Degrees",
"Degrees",
"Degrees",
"Degrees",
"Number",
"Number",
"Enum",
"Bool",
"Degrees",
"Nautical miles",
"Knots",
"Frequency ADF BCD32",
"Hz",
"Degrees",
"Number",
"BCO16",
"Enum",
"Bool",
"Bool",
"Bool",
"Degrees",
"Degrees",
"Number",
"Number",
"Bool",
"Bool",
"Enum",
"Bool",
"Degrees",
"Bool",
"Knots",
"Nautical miles",
"Degrees",
"Degrees",
"Meters",
"Radians",
"Bool",
"Bool",
"Bool",
"Bool",
"Meters per second",
"Radians",
"Radians",
"Radians",
"Meters",
"Radians",
"Radians",
"Meters",
"Radians",
"Radians",
"Meters per second",
"Radians",
"Seconds",
"Seconds",
"Degrees",
"Degrees",
"Meters",
"Bool",
"Degrees",
"Degrees",
"Meters",
"Seconds",
"Seconds",
"Radians",
"Number",
"Number",
"Bool",
"Bool",
"Bool",
"Enum",
"Enum",
"Bool",
"Enum",
"Number",
"Enum",
"Number",
"Bool",
"Bool",
"Seconds",
"Number",
"Number",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Frequency BCD16",
"Frequency BCD16",
"String",
"String",
"String",
"String",
"Flags",
"Number",
"Degrees",
"Number",
"String",
"String",
"Meters",
"Meters",
"Position",
"Position",
"Position",
"Position",
"Position",
"Position",
"Radians",
"Position",
"Percent Over 100",
"Position",
"Position",
"Position",
"Position",
"Bool",
"Bool",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Number",
"Number",
"Percent Over 100",
"Percent Over 100",
"Radians",
"Radians",
"Percent Over 100",
"Percent Over 100",
"Radians",
"Radians",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"psf",
"Bool",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Enum",
"Number",
"Percentage",
"Number",
"Percent Over 100",
"Radians",
"Percent Over 100",
"Percentage",
"Percentage",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Radians",
"Percent Over 100",
"Radians",
"Percent Over 100",
"Radians",
"Radians",
"Percent Over 100",
"Radians",
"Percent Over 100",
"Radians",
"Percent Over 100",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Rpm",
"Rpm",
"Rpm",
"Bool",
"Bool",
"Number",
"Bool",
"Bool",
"Bool",
"Degrees",
"Bool",
"Feet",
"Bool",
"Bool",
"Radians",
"Bool",
"Bool",
"Feet/minute",
"Bool",
"Radians",
"Radians",
"Bool",
"Knots",
"Bool",
"Number",
"Bool",
"Number",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Radians",
"Rpm",
"Rpm",
"Radians",
"Radians",
"Radians",
"Radians",
"Radians",
"Bool",
"Enum",
"Bool",
"Bool",
"Percent",
"Percent",
"Percent over 100",
"Slugs per cubic feet",
"Celsius",
"inHg",
"Knots",
"Degrees",
"Meters per second",
"Meters per second",
"Meters per second",
"Mask",
"Knots",
"Knots",
"Knots",
"Millibars",
"Millibars",
"Celsius",
"Bool",
"Bool",
"Meters",
"Rankine",
"Percent Over 100",
"Bool",
"Bool",
"Bool",
"Rankine",
"Bool",
"Bool",
"Bool",
"Percent Over 100",
"Percent Over 100",
"Bool",
"Bool",
"Bool",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Percent Over 100",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Enum",
"Bool",
"Amperes",
"Amperes",
"Volts",
"Volts",
"Amperes",
"Volts",
"Amperes",
"Volts",
"Amperes",
"Volts",
"Amperes",
"Volts",
"Amperes",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Pound force per square foot",
"Percent Over 100",
"Percent Over 100",
"Bool",
"Pounds",
"Pounds",
"Pounds",
"Bool",
"Bool",
"GForce",
"Bool",
"Bool",
"Number",
"Bool",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet per second",
"Feet per second",
"Percent over 100",
"Percent over 100",
"Bool",
"Bool",
"Feet",
"Feet",
"Pounds",
"Number",
"Bool",
"Feet per minute",
"Meters",
"String",
"Number",
"Pounds per square foot",
"Feet per second",
"Knots",
"Feet per second",
"Bool",
"Feet per second",
"Feet per second",
"Meters",
"Degrees",
"Radians per second",
"Bool",
"Position",
"Position",
"Position",
"foot pounds",
"Bool",
"Square feet",
"Feet",
"Radians per second",
"Per radian",
"Radians",
"Radians",
"Percent over 100",
"Percent over 100",
"Machs",
"Machs",
"String",
"Radians",
"Enum",
"Feet",
"Feet",
"Feet",
"Feet",
"Feet",
"slug feet squared",
"slug feet squared",
"slug feet squared",
"slug feet squared",
"slug feet squared",
"slug feet squared",
"slug feet squared",
"slug feet squared",
"Bool",
"Rpm",
"Number",
"Bool",
"Radians",
"Radians",
"Radians",
"Radians",
"Enum",
"String",
"Percent over 100",
"Enum",
"Amps",
"Bool",
"Enum",
"Percent over 100",
"Radians",
"Bool",
"Bool",
"Bool",
"Percent over 100",
"Number",
"Per second",
"Bool",
"Feet",
"Percent over 100",
"Feet",
"Bool",
"Enum",
"Radians",
"Feet",
"Feet",
"Feet",
"Bool",
"Radians",
"Percent over 100",
"Percent over 100",
"Radians",
"Percent over 100",
"Radians",
"Radians",
"Radians",
"Percent over 100",
"Percent over 100",
"Percent over 100",
"SIMCONNECT_DATA_LATLONALT",
"SIMCONNECT_DATA_LATLONALT",
"Feet",
"Radians",
"Enum",
"Enum",
"Percent over 100",
"Bool",
"Percent over 100",
"Percent over 100",
"Volts",
"Bool",
"Bool",
"Bool",
"Feet",
"Feet",
"Feet per second",
"foot pounds",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"Bool",
"String64",
"String64",
"String64",
"String64",
"String8",
"Variable length string",
"String8",
"String",
"String",
"String",
"Seconds",
"Seconds",
"Number",
"Number",
"Number",
"Number",
"Number",
"Seconds",
"Number",
"Number",
"Number",
"Number",
"Number",
"Seconds",
"Enum",
"Number",
"Enum"};
    }
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string _sPropertyName = null)
        {
            PropertyChangedEventHandler hEventHandler = this.PropertyChanged;
            if (hEventHandler != null && !string.IsNullOrEmpty(_sPropertyName))
            {
                hEventHandler(this, new PropertyChangedEventArgs(_sPropertyName));
            }
        }

        protected bool SetProperty<T>(ref T _tField, T _tValue, [CallerMemberName] string _sPropertyName = null)
        {
            return this.SetProperty(ref _tField, _tValue, out T tPreviousValue, _sPropertyName);
        }

        protected bool SetProperty<T>(ref T _tField, T _tValue, out T _tPreviousValue, [CallerMemberName] string _sPropertyName = null)
        {
            if (!object.Equals(_tField, _tValue))
            {
                _tPreviousValue = _tField;
                _tField = _tValue;
                this.OnPropertyChanged(_sPropertyName);
                return true;
            }

            _tPreviousValue = default(T);
            return false;
        }
    }
    public class SimvarRequest : ObservableObject
    {
        public DEFINITION eDef = DEFINITION.Dummy;
        public REQUEST eRequest = REQUEST.Dummy;

        public string sName { get; set; }

        public double dValue
        {
            get { return m_dValue; }
            set { this.SetProperty(ref m_dValue, value); }
        }
        private double m_dValue = 0.0;

        public string sUnits { get; set; }

        public bool bPending = true;
        public bool bStillPending
        {
            get { return m_bStillPending; }
            set { this.SetProperty(ref m_bStillPending, value); }
        }
        private bool m_bStillPending = false;

    };

}