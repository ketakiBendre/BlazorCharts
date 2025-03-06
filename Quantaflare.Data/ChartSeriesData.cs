using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudBlazor;
using MudBlazor.Components.Chart.Models;

namespace Quantaflare.Data
{
    public class ChartSeriesData
    {
        public int chartId {  get; set; }
        public string chartTitle { get; set; } = string.Empty;
        public QChartType chartType { get; set; }
        public MudBlazor.ChartType MudChartType => ConvertToMudChartType(chartType); 
        public MudBlazor.ChartOptions options { get; set; } = new MudBlazor.ChartOptions();

        public int Index { get; set; } = -1;
        public string[] xAxisLabels = Array.Empty<string>();

        public List<MudBlazor.ChartSeries> series = new List<MudBlazor.ChartSeries>();

        public List<TimeSeriesChartSeries> tSeries= new List<TimeSeriesChartSeries>();

        public List<RawData> rawDataList = new List<RawData>();
        public List<LocationDetails> locationDetailsList = new List<LocationDetails>();

        private static MudBlazor.ChartType ConvertToMudChartType(QChartType chartType)
        {
            return chartType switch
            {
                QChartType.Line => MudBlazor.ChartType.Line,
                QChartType.Bar => MudBlazor.ChartType.Bar,
                QChartType.TimeSeries => MudBlazor.ChartType.Timeseries,
                QChartType.Pie => MudBlazor.ChartType.Pie,
                _ => throw new NotSupportedException($"Chart type {chartType} is not supported in MudBlazor.")
            };
        }
    }

    public enum QChartType
    {
        Line,
        Bar,
        Pie,
        TimeSeries,
        Map,
        RawData
    }
}
