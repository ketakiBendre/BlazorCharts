using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantaflare.Data
{
    public class EnergyData
    {
        public string eTime { get; set; } = string.Empty;
        public double home { get; set; }
        public double powerwall { get; set; }
        public double solar { get; set; }
    }
}
