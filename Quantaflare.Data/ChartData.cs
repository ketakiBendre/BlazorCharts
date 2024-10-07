using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantaflare.Data
{
    public class ChartData
    {
        public string charttitle {  get; set; }=string.Empty;

        public List<EnergyStream> EnergyStream { get; set; } = new List<EnergyStream>();

    }
}
