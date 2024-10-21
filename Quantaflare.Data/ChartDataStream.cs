using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Quantaflare.Data
{
    [JsonPolymorphic]
    [JsonDerivedType(typeof(ChartDataStream), typeDiscriminator: "base")]
    [JsonDerivedType(typeof(LineChartData), typeDiscriminator: "withLine")]
    [JsonDerivedType(typeof(BarChartData), typeDiscriminator: "withBar")]
    public class ChartDataStream
    {
        public string stream { get; set; } = string.Empty;
        public string field { get; set; } = string.Empty;

    }
}
