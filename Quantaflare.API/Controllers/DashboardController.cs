using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Quantaflare.Data;
using System.Data;
using System.Data.Common;
using static MudBlazor.CategoryTypes;
using System.Text.Json;

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
                return result;
            }
        }

        [HttpGet]
        [Route("GetDashboardName")]
        public IActionResult GetDashboardName(int clusterId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var dashboardName = connection.QuerySingleOrDefault<string>("SELECT dashname FROM dashboard WHERE clusterid = @clusterId AND dashname like \'New Dashboard%\' ORDER BY dashid DESC LIMIT 1", new { clusterId });
                if (dashboardName == null)
                {
                    return Ok(string.Empty); 
                }
                return Ok(dashboardName.Trim());
            }
        }

        [HttpPost]
        [Route("PostDashboard")]
        public IActionResult PostDashboard(Dashboard ds)
         {
             using (var connection = new NpgsqlConnection(_connectionString))
             {
                connection.Open();
                var sql = @"INSERT INTO dashboard (clusterid, createdon, dashname, dashtype) 
                    VALUES (@clusterId, @createdOn, @dashName, @dashType) 
                    RETURNING dashid;"; 

                // Execute the query and capture the returned ID
                var dashboardId = connection.ExecuteScalar<int>(sql, ds);

                // Return the dashboard ID
                return Ok(dashboardId);
            }
         }

        [HttpPost]
        [Route("UpdateDashboard")]
        public IActionResult UpdateDashboard(Dashboard ds)
        {
            var jsonData = JsonSerializer.Serialize(ds.QFChartList);

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var sql = "UPDATE dashboard SET chartinfo=@jsonData::jsonb WHERE clusterid =@clusterId and dashname= @dashName and dashid =@dashid";
                var parameters = new
                {
                    jsonData,
                    clusterId = ds.clusterId,
                    dashName = ds.dashName,
                    dashid = ds.dashid
                };
                int rowsAffected = connection.Execute(sql, parameters);
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
