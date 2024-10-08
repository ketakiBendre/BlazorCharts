﻿using Quantaflare.Data;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Net.Http.Json;

namespace QFWASM.UI.Services
{
    public class MyChartService
    {
        private readonly HttpClient Http;
        private string[] xAxisLabels = new string[0];
        private List<ChartDataResult> chartDataList;
        private List<MudBlazor.ChartSeries> series = new List<MudBlazor.ChartSeries>();
        public MyChartService(HttpClient httpClient)
        {
            Http = httpClient;
        }
        public async Task<(string[], List<MudBlazor.ChartSeries> series)> GetChartInfo(List<EnergyStream> eStreamList)
        {

            var response = await Http.PostAsJsonAsync("api/Stream/getChartInfo", eStreamList);
            if (response.IsSuccessStatusCode)
            {
                var resultList = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

                chartDataList = resultList.Select(dict => new ChartDataResult
                {
                    recordtime = DateTimeOffset.Parse(dict["recordtime"].ToString()),
                    result = JsonSerializer.Deserialize<Dictionary<string, double>>(dict["result"].ToString())
                }).ToList();
                xAxisLabels = chartDataList.Select(data => data.recordtime.ToString()).ToArray();

                // Prepare Series with Data Points
                var seriesKeys = chartDataList.SelectMany(data => data.result.Select(x => x.Key)).Distinct().ToList();
                foreach (var key in seriesKeys)
                {
                    var seriesData = new MudBlazor.ChartSeries
                    {
                        Name = key,

                        Data = chartDataList.SelectMany(data => data.result.Select(x => x.Value)).ToArray()

                    };
                    if (!series.Any(s => s.Name == seriesData.Name))
                    {
                        series.Add(seriesData);
                    }
                }
            }
            return (xAxisLabels, series);
        }
    }
}
