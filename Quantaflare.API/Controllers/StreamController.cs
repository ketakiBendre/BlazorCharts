using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Quantaflare.Data;

namespace Quantaflare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private readonly string _connectionString;


        public StreamController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                 ?? throw new ArgumentNullException(nameof(_connectionString), "Connection string 'DefaultConnection' not found.");
        }

        [HttpGet]
        [Route("getEnergyStream")]
        public IActionResult getEnergyStream()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var energyStream = connection.Query<EnergyStream>("SELECT distinct(streams) FROM energy");
                return Ok(energyStream);
            }



        }

        [HttpGet]
        [Route("getEnergyField")]
        public IActionResult getEnergyField()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var energyField = connection.Query<EnergyStream>("SELECT * FROM energy WHERE streams=\'Battery\'");
                return Ok(energyField);
            }



        }
    }
}
