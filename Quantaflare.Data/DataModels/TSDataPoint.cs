using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QFDomain.Entities.Data.Read
{
    public class TSDataPoint
    {
        public long TimeStamp { get; set; }
        public List<KeyValuePair<object, object>> StreamKeyValues { get; set; } = new List<KeyValuePair<object, object>>();
    }

    //This object is used to show the results of raw data in the report. The UitStreamDatapoint is other object to really move the data
    //for ex. imagine a stream Motor, that has 2 keys. Voltage and RPM  value at perticular timestamp
    //You will have 1 timestamp. and list will have 
    //{
    //<obj,obj> = <voltage id, voltage Value>
    //<obj,obj> = <RPM id, RPM Value>
}
