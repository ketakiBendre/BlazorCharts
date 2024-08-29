using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Quantaflare.Data;

namespace Quantaflare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectController : ControllerBase
    {
        private readonly string _connectionString;


        public ConnectController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                 ?? throw new ArgumentNullException(nameof(_connectionString), "Connection string 'DefaultConnection' not found.");
        }


        [HttpGet]
        [Route("getConn")]
        public IActionResult getConn()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var energyConnect = connection.Query<UserConnect>("SELECT * FROM energyconnect");
                return Ok(energyConnect);
            }



        }

    }
}
