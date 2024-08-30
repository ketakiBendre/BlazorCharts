using Microsoft.AspNetCore.Components;
using Quantaflare.Data;

namespace Quantaflare.UI.Services
{
    public class EnergyStreamClass : IEnergyStreamClass
    {
        private readonly HttpClient _httpClient;
        public EnergyStreamClass(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<EnergyStream>> getEnergyStream()
        {

            var result = await _httpClient.GetJsonAsync<EnergyStream[]>("api/Stream/getEnergyStream");

            return result;
        }


        public async Task<IEnumerable<EnergyStream>> getEnergyField()
        {

            var result = await _httpClient.GetJsonAsync<EnergyStream[]>("api/Stream/getEnergyField");

            return result;
        }
    }
}
