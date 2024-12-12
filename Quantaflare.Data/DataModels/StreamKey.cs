using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QFDomain.Entities.Data.Read
{
    public class StreamKey
    {
        public int StreamKeyId { get; set; }
        public int StreamId { get; set; }
        //key is the text value
        public string Key { get; set; }
        //note - no key value here yet
        public string DataType { get; set; }

        public bool IsActive { get; set; }

    }
}
