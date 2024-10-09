using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Quantaflare.Data;
using System.Data;
using System.Data.Common;

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

        /*[HttpGet]
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
        */
        [HttpGet]
        [Route("getDashboard")]

        public async Task<IActionResult> getDashboard(int inputId)
        {
            var streams = await getDashboardId(inputId);
            return Ok(streams);
        }

        private async Task<IEnumerable<Dashboard>> getDashboardId(int inputId)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();
                var result = await db.QueryAsync<Dashboard>("SELECT * FROM public.getdashboard(@inputId)", new { inputId });
                return result; // Ensure you return the result here
            }
        }


        [HttpPost]
        [Route("PostDashboard")]
        public IActionResult PostDashboard(Dashboard ds)
         {
             using (var connection = new NpgsqlConnection(_connectionString))
             {
                 connection.Open();
                var sql = "INSERT INTO dashboard (clusterid, createdon, dashname, dashtype) values(@clusterId, @createdOn, @dashName, @dashType)";
                int rowsAffected = connection.Execute(sql, ds);
                return Ok(rowsAffected);
            }
         }

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
