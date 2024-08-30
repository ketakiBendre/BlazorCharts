using Quantaflare.Data;

namespace Quantaflare.UI.Services
{
    public interface IEnergyDataClass
    {
        Task<IEnumerable<EnergyData>> getEnergyData();
    }
}
