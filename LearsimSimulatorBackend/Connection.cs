using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{
    /// <summary>
    /// Eventargs that the client triggers when getting a SimVar
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// The Message from the client
        /// </summary>
        public Tuple<SimVariable, string> Message { get; set; }
    }
    /// <summary>
    /// Baseclass for a connection used by all the connection classes
    /// </summary>
    public abstract class Connection
    {
        /// <summary>
        /// Tries to open a connection
        /// </summary>
        public abstract void Connect();
        /// <summary>
        /// The handler that handles the messages from the client
        /// </summary>
        public event EventHandler MessageHandler;
        /// <summary>
        /// Returns if the server is connected to the client
        /// </summary>
        /// <returns></returns>
        public abstract bool IsConnected();
        /// <summary>
        /// Gets the client from the connection class
        /// </summary>
        /// <returns></returns>
        public abstract Client GetClient();
        /// <summary>
        /// The event that triggers MessageEvent
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnMessage(MessageEventArgs e)
        {
            EventHandler handler = MessageHandler;
            handler?.Invoke(this, e);
        }
        /// <summary>
        /// Sends data to the connectionhandler
        /// </summary>
        /// <param name="messages"></param>
        public virtual void SendData(SimVarMessage[] messages)
        {

        }
    }
}
