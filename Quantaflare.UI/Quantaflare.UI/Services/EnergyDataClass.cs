using Microsoft.AspNetCore.Components;
using Quantaflare.Data;

namespace Quantaflare.UI.Services
{
    public class EnergyDataClass:IEnergyDataClass
    {
        private readonly HttpClient _httpClient;
        public EnergyDataClass(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<EnergyData>> getEnergyData()
        {

            var result = await _httpClient.GetJsonAsync<EnergyData[]>("api/EnergyData/getEnergyData");

            return result;
        }
    }
}
