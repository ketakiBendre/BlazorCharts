using Quantaflare.Data;
using static System.Net.WebRequestMethods;
using System.Text.Json;
using System.Net.Http.Json;
using System;
using MudBlazor.Components.Chart.Models;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using Microsoft.AspNetCore.Components;

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
        private readonly NavigationManager _navigationManager;
        public MyChartService(HttpClient httpClient, NavigationManager navigationManager)
        {
            Http = httpClient;
            _navigationManager = navigationManager;
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

       /* public async Task<ChartSeriesData> GetMapDataInfo()
        {
            var mapSeriesData = new ChartSeriesData();
            NavigationManager navigationManager;
            var queryParams = new
            {
                OrgId = 2,
                clusterId = 3,
                fromTimeStamp = 0,
                toTimeStamp = 0,
                StreamId = 9,
                pageIndex = 0,
                pageSize = 0,
                format = "string"
            };

            var url = $"https://api.quantaflare.com/v1/Data/stream-get/5b2e2b5d-4293-429c-be80-16f25fd39549";

            using var httpClient = new HttpClient();
            var configUrl = _navigationManager.BaseUri + "config.json";
            var config = await Http.GetFromJsonAsync<Dictionary<string, string>>(configUrl);


            var apiKey = config["ApiKey"];

            httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var jsonBody = JsonSerializer.Serialize(queryParams);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    var rootElement = doc.RootElement;

                    // Check if 'data' exists in the root element
                    if (rootElement.TryGetProperty("data", out var dataElement))
                    {
                        // Deserialize the "data" field into the UnitDataQueryResult object
                        var apiResponse = JsonSerializer.Deserialize<UnitDataQueryResult>(dataElement.ToString(), options);



                        foreach (var dataPoint in apiResponse.DataPoints)
                        {
                            // Initialize latitude and longitude to null initially
                            double? latitude = null;
                            double? longitude = null;

                            // Find the StreamKey for Lat and Long dynamically by checking StreamKeyId
                            var latStreamKey = apiResponse.StreamKeys.FirstOrDefault(s => s.Key == "Lat");
                            var longStreamKey = apiResponse.StreamKeys.FirstOrDefault(s => s.Key == "Long");

                            // Iterate over StreamKeyValues for each data point to find the respective values
                            foreach (var streamKeyValue in dataPoint.StreamKeyValues)
                            {
                                // If the StreamKeyId matches the one for Latitude, assign the value
                                if (latStreamKey != null && streamKeyValue.Key == latStreamKey.StreamKeyId.ToString())
                                {
                                    latitude = Convert.ToDouble(streamKeyValue.Value);

                                }
                                // If the StreamKeyId matches the one for Longitude, assign the value
                                if (longStreamKey != null && streamKeyValue.Key == longStreamKey.StreamKeyId.ToString())
                                {
                                    longitude = Convert.ToDouble(streamKeyValue.Value);

                                }
                            }

                            // If both latitude and longitude are available, add a LocationDetails object to the list
                            if (latitude.HasValue && longitude.HasValue)
                            {
                                mapSeriesData.locationDetailsList.Add(new LocationDetails
                                {
                                    Latitude = latitude.Value,
                                    Longitude = longitude.Value
                                });
                            }
                        }
                        return mapSeriesData;
                    }
                    else
                    {
                        Console.WriteLine("'data' field not found in the JSON response.");
                    }
                }

            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }


            return mapSeriesData;
        }*/
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
    }
}
