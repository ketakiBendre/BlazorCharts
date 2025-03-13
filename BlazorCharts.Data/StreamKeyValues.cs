using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCharts.Data
{
    public class StreamKeyValues
    {
        public int streamkvid {  get; set; }
        public TimeOnly timestamp { get; set; }

        public int streamid {  get; set; }

        public int streamkeyid {  get; set; }
        public double keyvalue { get; set; }
    }
}
