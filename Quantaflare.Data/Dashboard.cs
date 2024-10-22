using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantaflare.Data
{
    public class Dashboard
    {
        public int? dashid { get; set; }
        public int? clusterId { get; set; }
        public DateTime createdOn { get; set; }
       
        public string dashName { get; set; } = string.Empty;

        public string dashType { get; set; }= string.Empty;
        public List<Dictionary<int, ChartPosition>> QFChartList { get; set; } = new List<Dictionary<int, ChartPosition>> ();
      


    }
}
