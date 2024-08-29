using Quantaflare.Data;

namespace Quantaflare.UI.Services
{
    public interface IUConnectServices
    {
        Task<IEnumerable<UserConnect>> getConn();
    }
}
