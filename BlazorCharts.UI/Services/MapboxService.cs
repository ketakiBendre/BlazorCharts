using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorCharts.UI.Services
{


    public class MapboxService
    {
        private readonly HttpClient _httpClient;
        private readonly string _mapboxAccessToken;

        public MapboxService(HttpClient httpClient, string mapboxAccessToken)
        {
            _httpClient = httpClient;
            _mapboxAccessToken = mapboxAccessToken;
        }

        public async Task<string> GetGeocodingDataAsync(string query, string countryCode = null)
        {
            // Construct the request URL
            var url = $"https://api.mapbox.com/geocoding/v5/mapbox.places/{query}.json?access_token={_mapboxAccessToken}";

            if (!string.IsNullOrEmpty(countryCode))
            {
                url += $"&country={countryCode}";
            }

            // Make the API request
            var response = await _httpClient.GetAsync(url);

            // Ensure the request was successful
            response.EnsureSuccessStatusCode();

            // Return the JSON string or parse it as needed
            return await response.Content.ReadAsStringAsync();
        }
    }

}
