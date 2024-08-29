using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using Quantaflare.Data;

namespace Quantaflare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnergyController : ControllerBase
    {
        private readonly string _connectionString;


        public EnergyController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                 ?? throw new ArgumentNullException(nameof(_connectionString), "Connection string 'DefaultConnection' not found.");
        }

        [HttpGet]
        [Route("getEnergyData")]

        public IActionResult getEnergyData()
        {

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var energyData = connection.Query<EnergyData>("SELECT * FROM energydata");
                return Ok(energyData);
            }

        }

    }
}
