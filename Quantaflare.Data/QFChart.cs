using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Quantaflare.Data
{
    public class QFChart
    {
        public int chartID {  get; set; }
        public string chartTitle {  get; set; }=string.Empty;
        /// <summary>
        public MudBlazor.ChartType chartType { get; set; }

        public string chartDataStreamJson { get; set; } = string.Empty;
        /// </summary>
        public List<ChartDataStream> chartDataStreamList { get; set; } = new List<ChartDataStream>();

        public void DeserializeChartDataStreamJson()
        {
            if (!string.IsNullOrEmpty(chartDataStreamJson))
            {
                JsonSerializerOptions options = new() { TypeInfoResolver = new DefaultJsonTypeInfoResolver(), WriteIndented = true};
                var deserializedList = JsonSerializer.Deserialize<List<ChartDataStream>>(chartDataStreamJson, options);

                // If deserialization returns null, keep the existing list
                chartDataStreamList = deserializedList ?? new List<ChartDataStream>();
            }
        }
    }
}
