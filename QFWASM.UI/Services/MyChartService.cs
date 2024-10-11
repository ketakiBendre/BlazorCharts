using Quantaflare.Data;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Net.Http.Json;
using System;

namespace QFWASM.UI.Services
{
    public class MyChartService
    {
        private readonly HttpClient Http;
        private string[] xAxisLabels = new string[0];
        private List<ChartDataResult> chartDataList;
        private List<MudBlazor.ChartSeries> series = new List<MudBlazor.ChartSeries>();
        private ChartSeriesData chartSeriesData = new ChartSeriesData();
        public MyChartService(HttpClient httpClient)
        {
            Http = httpClient;
        }
        public async Task<ChartSeriesData> GetChartInfo(List<EnergyStream> eStreamList)
        {
            chartSeriesData = new ChartSeriesData();
            var response = await Http.PostAsJsonAsync("api/Stream/getChartInfo", eStreamList);
            if (response.IsSuccessStatusCode)
            {
                var resultList = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

                chartDataList = resultList.Select(dict => new ChartDataResult
                {
                    recordtime = DateTimeOffset.Parse(dict["recordtime"].ToString()),
                    result = JsonSerializer.Deserialize<Dictionary<string, double>>(dict["result"].ToString())
                }).ToList();
                chartSeriesData.xAxisLabels = chartDataList.Select(data => data.recordtime.ToString("hh:mm tt")).ToArray();

                // Prepare Series with Data Points
                series = new List<MudBlazor.ChartSeries>();
                var seriesKeys = chartDataList.SelectMany(data => data.result.Select(x => x.Key)).Distinct().ToList();
                foreach (var key in seriesKeys)
                {
                    var seriesData = new MudBlazor.ChartSeries
                    {
                        Name = key,

                        Data = chartDataList.Select(data => data.result[key]).ToArray()

                    };
                    if (!series.Any(s => s.Name == seriesData.Name))
                    {
                        series.Add(seriesData);
                    }
                }
                chartSeriesData.series = series;
            }
            return chartSeriesData;
        }

        public ChartSeriesData RemoveChartInfo(EnergyStream eStream)
        {
            string removeKey = (eStream.Fields + "_" + eStream.agr).ToLower();
            var seriesToRemove = chartSeriesData.series.FirstOrDefault(x => x.Name == removeKey);

            // If found, remove it from the list
            if (seriesToRemove != null)
            {
                chartSeriesData.series.Remove(seriesToRemove);
            }
            return chartSeriesData;
        }
    }
}
