using BlazorCharts.Data;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Net.Http.Json;
using System;
using MudBlazor.Components.Chart.Models;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using Microsoft.AspNetCore.Components;
using MudBlazor;


namespace BlazorCharts.UI.Services
{
    public class MyChartService
    {
        private readonly HttpClient Http;
        private string[] xAxisLabels = new string[0];
        private List<ChartDataResult> chartDataList;
        private List<MudBlazor.ChartSeries> series = new List<MudBlazor.ChartSeries>();
        private List<TimeSeriesChartSeries> tSeries = new List<TimeSeriesChartSeries>();
        private ChartSeriesData chartSeriesData = new ChartSeriesData();
        private readonly NavigationManager _navigationManager;
        private readonly ISnackbar Snackbar;
        public MyChartService(HttpClient httpClient, NavigationManager navigationManager, ISnackbar snackbar)
        {
            Http = httpClient;
            _navigationManager = navigationManager;
            Snackbar = snackbar;
        }

        public async Task<ChartSeriesData> GetLineChartInfo(List<LineChartData> lineChartList)
        {
            chartSeriesData = new ChartSeriesData();
            var response = await Http.PostAsJsonAsync("api/Stream/getLineChartInfo", lineChartList);
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

        public ChartSeriesData RemoveLineChartInfo(ChartDataStream chartStream)
        {
            string removeKey = chartStream.field;
            if (chartStream is LineChartData lineChartData)
            {
                removeKey = (removeKey + "_" + lineChartData.agr).ToLower();
            }

            var seriesToRemove = chartSeriesData.series.FirstOrDefault(x => x.Name == removeKey);

            // If found, remove it from the list
            if (seriesToRemove != null)
            {
                chartSeriesData.series.Remove(seriesToRemove);
            }
            return chartSeriesData;
        }

        public async Task<ChartSeriesData> GetTimeChartInfo(List<ChartDataStream> timeChartList, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            chartSeriesData = new ChartSeriesData();
            var queryString = $"?startTime={startTime:O}&endTime={endTime:O}"; // 'O' formats DateTime to ISO 8601

            // Make the POST request with the query string
            var response = await Http.PostAsJsonAsync($"api/Stream/getTimeChartInfo{queryString}", timeChartList);

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
                tSeries = new List<TimeSeriesChartSeries>();
                var seriesKeys = chartDataList.SelectMany(data => data.result.Select(x => x.Key)).Distinct().ToList();
                foreach (var key in seriesKeys)
                {
                    var seriesData = new TimeSeriesChartSeries
                    {
                        Name = key,
                        Data = chartDataList
                   .Where(data => data.result.ContainsKey(key))
                   .Select(data => new TimeSeriesChartSeries.TimeValue(data.recordtime.UtcDateTime, data.result[key]))
                   .ToList(),
                        IsVisible = true,
                        Type = TimeSeriesDiplayType.Area
                    };

                    tSeries.Add(seriesData);
                }

                chartSeriesData.tSeries = tSeries;

            }
            return chartSeriesData;
        }

        public async Task<ChartSeriesData> GetMapDataInfo(List<ChartDataStream> mapChartList, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var mapSeriesData = new ChartSeriesData();
            var queryString = $"?startTime={startTime.UtcDateTime:O}&endTime={endTime.UtcDateTime:O}";
            // 'O' formats DateTime to ISO 8601

            // Make the POST request with the query string
            var response = await Http.PostAsJsonAsync($"api/Stream/getMapInfo{queryString}", mapChartList);

            if (response.IsSuccessStatusCode)
            {
                var locationDetailsList = await response.Content.ReadFromJsonAsync<List<LocationDetails>>();
                mapSeriesData.locationDetailsList = locationDetailsList;
                return mapSeriesData;

            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return new ChartSeriesData(); // Return empty list on failure
            }
        }
        public async Task<ChartSeriesData> GetRawDataInfo(List<ChartDataStream> timeChartList, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            chartSeriesData = new ChartSeriesData();
            var rawDataList = new List<RawData>();
            var queryString = $"?startTime={startTime:O}&endTime={endTime:O}"; // 'O' formats DateTime to ISO 8601

            // Make the POST request with the query string
            var response = await Http.PostAsJsonAsync($"api/Stream/getTimeChartInfo{queryString}", timeChartList);

            if (response.IsSuccessStatusCode)
            {
                var resultList = await response.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();

                rawDataList = resultList.Select(dict => new RawData
                {
                    Timestamp = DateTime.Parse(dict["recordtime"].ToString()),
                    Fields = JsonSerializer.Deserialize<Dictionary<string, object>>(dict["result"].ToString())
                }).ToList();
                chartSeriesData.rawDataList = rawDataList;


            }
            return chartSeriesData;
        }

        
        public List<RawData> RemoveRawData(ChartDataStream chartStream, List<RawData> rawDataList)
        {
            string removeKey = chartStream.field;


            foreach (var rawData in rawDataList)
            {
                // Check if the field exists in the dictionary, and remove it
                if (rawData.Fields.ContainsKey(removeKey.ToLower()))
                {
                    rawData.Fields.Remove(removeKey.ToLower());
                }
            }
            return rawDataList;
        }

        public void ShowAlert(string message, string position, Severity sev)
        {
            Snackbar.Clear();
            Snackbar.Configuration.PositionClass = position;
            Snackbar.Add(message, sev, c => c.SnackbarVariant = Variant.Outlined);
        }
    }
}
