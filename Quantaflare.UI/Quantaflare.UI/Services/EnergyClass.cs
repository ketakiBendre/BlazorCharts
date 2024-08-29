using Microsoft.AspNetCore.Components;
using Quantaflare.Data;

namespace Quantaflare.UI.Services
{
    public class EnergyClass:IEnergyClass
    {
        private readonly HttpClient _httpClient;
        public EnergyClass(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<EnergyData>> getEnergyData()
        {

            var result = await _httpClient.GetJsonAsync<EnergyData[]>("api/Energy/getEnergyData");

            return result;
        }
    }
}
