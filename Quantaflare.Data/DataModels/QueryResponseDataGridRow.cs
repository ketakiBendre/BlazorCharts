using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QFDomain.Entities.Data.Read
{
    public class QueryResponseDataGridRow
    {
        public long TimeStamp { get; set; }
        public Dictionary<string, object> KeyValues { get; set; } = new Dictionary<string, object>();
    }
}

