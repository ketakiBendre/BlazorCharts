using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudBlazor;

namespace Quantaflare.Data
{
    public class ChartSeriesData
    {
        public int chartId {  get; set; }
        public string chartTitle { get; set; } = string.Empty;
        public MudBlazor.ChartType chartType { get; set; }

        public MudBlazor.ChartOptions options { get; set; } = new MudBlazor.ChartOptions();
      

        public string[] xAxisLabels = Array.Empty<string>();

        public List<MudBlazor.ChartSeries> series = new List<MudBlazor.ChartSeries>();
    }
}
