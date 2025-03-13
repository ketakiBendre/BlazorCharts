using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCharts.Data
{
    public class ChartDataResult
    {
        public DateTimeOffset recordtime{ get; set; }
        public Dictionary<string, double> result { get; set; }
    }
}
