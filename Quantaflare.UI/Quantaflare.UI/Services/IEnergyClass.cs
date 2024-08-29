using Quantaflare.Data;

namespace Quantaflare.UI.Services
{
    public interface IEnergyClass
    {
        Task<IEnumerable<EnergyData>> getEnergyData();
    }
}
