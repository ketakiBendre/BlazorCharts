using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCharts.Data
{
    public class AppConfig
    {
        public MapboxConfig Mapbox { get; set; }
    }

    public class MapboxConfig
    {
        public string AccessToken { get; set; }
    }

}
