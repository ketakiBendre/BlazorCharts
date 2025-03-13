using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCharts.Data
{
    public class BarChartData: ChartDataStream
    {
        public string Aggregator { get; set; } = string.Empty;
        public string GroupByField { get; set; } = string.Empty;

    }
}
