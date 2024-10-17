using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantaflare.Data
{
    public class QFChart
    {
        public int chartID {  get; private set; }
        public string chartTitle {  get; set; }=string.Empty;
        /// <summary>
        public MudBlazor.ChartType chartType { get; set; }
        
        /// </summary>
        public List<ChartDataStream> chartDataStreamList { get; set; } = new List<ChartDataStream>();
    }
}
