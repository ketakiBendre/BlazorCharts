using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quantaflare.Data;
using Dapper;
using Npgsql;

namespace Quantaflare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly string _connectionString;


        public DeviceController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                 ?? throw new ArgumentNullException(nameof(_connectionString), "Connection string 'DefaultConnection' not found.");
        }

        [HttpOptions]
        public IActionResult Options()
        {
            
            return Ok();
        }

        [HttpPost("RegisterDevice")]
        //[Route("RegisterDevice")]
        public IActionResult RegisterDevice([FromBody] string connectId)
        {
            if (string.IsNullOrEmpty(connectId))
            {
                return BadRequest("Connect ID is required.");
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
               // connection.Open();
                var energyConnect = connection.Query<UserConnect>("SELECT * FROM energyconnect");
                Console.WriteLine(energyConnect.ToString());

                foreach (var user in energyConnect)
                {
                    if(user.ConnectId == connectId)
                    {
                        // Logic to generate or retrieve the UnitId and UnitName based on the deviceId
                        var unitId = Guid.NewGuid().ToString(); // Unique identifier for the unit
                        var unitName = $"Device-{connectId}";

                        var unitInfo = new UnitInfo
                        {
                            UnitId = unitId,
                            UnitName = unitName
                        };
                        return Ok(unitInfo);
                    }
                }
            }
            return BadRequest("Connect ID is not registered.");

        }
    }
}
