using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearsimSimulatorBackend.DataHandlers
{
   
    internal class Clock : DataHandler
    {
        string YEAR = "2000";
        string MONTH = "01";
        string DAY = "01";

        public Clock() 
        {
        }

        public override string HandleData(SimVarMessage message,Binding binding)
        {
            string retMessage ="";
            if(binding.ValueName == "00" || binding.ValueName == "01") {
                retMessage = binding.ValueName+"*";
                retMessage += "4*";
                TimeSpan t = TimeSpan.FromSeconds(int.Parse(message.Message.Split(',')[0]));
                retMessage += $"{t.Hours.ToString("00")}:{t.Minutes.ToString("00")};";
            
            }else
            {
                retMessage = "02*4*";

                switch (binding.ValueName)
                {
                    case "YEAR":
                        YEAR = message.Message;
                        break;
                    case "MONTH":
                        MONTH= message.Message;
                        break;
                    case "DAY":
                        DAY = message.Message;
                        break;
                    default:

                        break;
                }
                DateTime dateTime = DateTime.Now;
                if (YEAR == "0")
                {
                    YEAR = dateTime.Year.ToString();
                }
                if (MONTH == "0")
                {
                    MONTH = dateTime.Month.ToString("00");
                }
                if (DAY == "0")
                {
                    DAY = dateTime.Day.ToString("00");
                }
                retMessage += $"{int.Parse(YEAR).ToString("0000")}-{int.Parse(MONTH).ToString("00")}-{int.Parse(DAY).ToString("00")};";
            }
            return retMessage;
            
        }
    }
}
