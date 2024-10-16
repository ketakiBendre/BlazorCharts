using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantaflare.Data
{
    public class ChartPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 300; // Default width
        public double Height { get; set; } = 200; // Default height
    }
}
