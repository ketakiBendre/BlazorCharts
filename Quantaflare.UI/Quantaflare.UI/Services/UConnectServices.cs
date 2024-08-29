using Microsoft.AspNetCore.Components;
using Quantaflare.Data;

namespace Quantaflare.UI.Services
{
    public class UConnectServices : IUConnectServices
    {
        private readonly HttpClient _httpClient;
        public UConnectServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<UserConnect>> getConn()
        {

            var result = await _httpClient.GetJsonAsync<UserConnect[]>("api/Connect/getConn");
            if (result == null)
            {
                UserConnect[] _connList = { new UserConnect { ConnectId = "A6" } };

                result = _connList;
            }
            return result;
        }
    }
}
