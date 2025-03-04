using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantaflare.Data
{
    public class LocationDetails
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int ZoomLevel { get; set; } = 10;
    }
}
