using Quantaflare.Data;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Net.Http.Json;
using System;
using MudBlazor.Components.Chart.Models;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

namespace QFWASM.UI.Services
{
    public class MyChartService
    {
        private readonly HttpClient Http;
        private string[] xAxisLabels = new string[0];
        private List<ChartDataResult> chartDataList;
        private List<MudBlazor.ChartSeries> series = new List<MudBlazor.ChartSeries>();
        private List<TimeSeriesChartSeries> tSeries = new List<TimeSeriesChartSeries>();
        private ChartSeriesData chartSeriesData = new ChartSeriesData();
        public MyChartService(HttpClient httpClient)
        {
            Http = httpClient;
        }
        public async Task<ChartSeriesData> GetChartInfo(List<LineChartData> lineChartList)
        {
            chartSeriesData = new ChartSeriesData();
            var response = await Http.PostAsJsonAsync("api/Stream/getChartInfo", lineChartList);
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

        public ChartSeriesData RemoveChartInfo(ChartDataStream chartStream)
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

        public async Task<List<RawData>> GetRawDataInfo(List<ChartDataStream> timeChartList, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            // Initialize the result list
            var rawDataList = new List<RawData>();

            // Build query string with ISO 8601 format for DateTimeOffset
            var queryString = $"?startTime={startTime:O}&endTime={endTime:O}";

            try
            {
                // Send the POST request with the query string and serialized body
                var response = await Http.PostAsJsonAsync($"api/Stream/get_raw_data{queryString}", timeChartList);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // Deserialize the response directly into a List<RawData>
                    rawDataList = await response.Content.ReadFromJsonAsync<List<RawData>>()
                                  ?? new List<RawData>();
                }
                else
                {
                    // Log or handle unsuccessful responses
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {error}");
                }
            }
            catch (Exception ex)
            {
                // Log or handle exceptions
                Console.WriteLine($"Error fetching raw data: {ex.Message}");
                throw; // Rethrow the exception if necessary
            }

            return rawDataList;
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
    }
}
