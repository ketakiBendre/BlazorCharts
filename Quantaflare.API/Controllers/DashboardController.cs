using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Quantaflare.Data;

namespace Quantaflare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly string _connectionString;


        public DashboardController(IConfiguration configuration)
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

        [HttpGet]
        [Route("getDashboard")]
        public IActionResult getDashboard(int clusterId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var dashboard = connection.Query<Dashboard>("SELECT * FROM dashboard").ToList();
                if (dashboard != null)
                    return Ok(dashboard);
                else
                    return BadRequest("No data found");
            }
        }

        /* public IActionResult getDashboard(int clusterId)
         {
             using (var connection = new NpgsqlConnection(_connectionString))
             {
                 connection.Open();
                 var dashboard = connection.Query<Dashboard>("SELECT * FROM dashboard").Where(e => e.clusterId == clusterId).ToList();
                 if (dashboard!= null)
                      return Ok(dashboard);
                 else
                     return BadRequest("No data found");
             }
         }*/

        [HttpGet]
        [Route("getCluster")]
        public IActionResult getCluster()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var cluster = connection.Query<Cluster>("SELECT * FROM cluster").ToList();
                 return Ok(cluster);
                
            }
        }

    }
}
