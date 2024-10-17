using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantaflare.Data
{
    public class Dashboard
    {
        public int? clusterId { get; set; }
        public DateTime createdOn { get; set; }
        public int? dashbaordId { get; private set; }
        public string dashName { get; set; } = string.Empty;

        public string dashType { get; set; }= string.Empty;
        public ChartPosition chartPosition { get; set; } = new ChartPosition();
        public List<QFChart> chartList { get; set; } = new List<QFChart>();


    }
}
