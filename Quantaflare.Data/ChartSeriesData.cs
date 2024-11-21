﻿using System;
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
        public string chartTitle { get; set; }
        public string[] xAxisLabels;

        public List<MudBlazor.ChartSeries> series;
    }
}
