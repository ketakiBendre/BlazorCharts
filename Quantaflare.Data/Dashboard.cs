using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Quantaflare.Data
{
    public class Dashboard
    {
        public int dashid { get; set; }
        public int? clusterId { get; set; }
        public DateTime createdOn { get; set; }
       
        public string dashName { get; set; } = string.Empty;

        public string dashType { get; set; }= string.Empty;

        public string QFChartListJson { get; set; } = string.Empty;
        public List<Dictionary<int, int>> QFChartList { get; set; } = new List<Dictionary<int, int>> ();

        public void DeserializeQFChartList()
        {
            if (!string.IsNullOrEmpty(QFChartListJson))
            {
                QFChartList = JsonSerializer.Deserialize<List<Dictionary<int, int>>>(QFChartListJson) ?? new List<Dictionary<int, int>>();
            }
        }

    }
}
