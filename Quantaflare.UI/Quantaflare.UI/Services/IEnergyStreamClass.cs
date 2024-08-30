using Quantaflare.Data;

namespace Quantaflare.UI.Services
{
    public interface IEnergyStreamClass
    {
        Task<IEnumerable<EnergyStream>> getEnergyStream();

        Task<IEnumerable<EnergyStream>> getEnergyField();
    }
}
