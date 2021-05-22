using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend
{
    public class MessageEventArgs : EventArgs
    {
        public Tuple<SimVariable,string> Message { get; set; }
    }
    public abstract class Connection
    {
        public event EventHandler MessageHandler;
        public abstract bool IsConnected();
        public abstract Client GetClient();
        protected virtual void OnMessage(MessageEventArgs e)
        {
            EventHandler handler = MessageHandler;
            handler?.Invoke(this, e);
        }
        public virtual void SendData(SimVarMessage[] messages)
        {
          
        }
    }
}
