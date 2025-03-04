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
                var dashboardName = connection.QuerySingleOrDefault<string>("SELECT dashname FROM dashboard WHERE clusterid = @clusterId AND dashname like \'New Dashboard%\' ORDER BY dashname DESC LIMIT 1", new { clusterId });
                if (dashboardName == null)
                {
                    return Ok(string.Empty); 
                }
                return Ok(dashboardName.Trim());
            }
        }

        [HttpGet]
        [Route("GetDashboardDetails")]
        public IActionResult GetDashboardDetails(int clusterId, string dashname)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var parameters = new { clusterId, dashname };

                var sql = @"
            SELECT dashid, 
                   clusterid, 
                   createdon, 
                   dashname, 
                   dashtype, 
                   chartinfo 
            FROM dashboard 
            WHERE clusterid = @clusterId AND dashname = @dashname";

                // Query the raw data
                var rawDashboard = connection.QuerySingleOrDefault(sql, parameters);

                if (rawDashboard == null)
                    return NotFound();

                // Map the raw data to a Dashboard object
                var dashboard = new Dashboard
                {
                    dashid = rawDashboard.dashid,
                    clusterId = rawDashboard.clusterid,
                    createdOn = rawDashboard.createdon,
                    dashName = rawDashboard.dashname,
                    dashType = rawDashboard.dashtype,
                };

                // Deserialize the chartinfo (JSON) into QFChartList
                if (!string.IsNullOrEmpty(rawDashboard.chartinfo))
                {
                    dashboard.QFChartList = JsonSerializer.Deserialize<List<Dictionary<int, int>>>(rawDashboard.chartinfo);
                }

                return Ok(dashboard);
            }
        }


        [HttpPost]
        [Route("PostDashboard")]
        public IActionResult PostDashboard(Dashboard ds)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // Serialize QFChartList to JSON
                var jsonData = JsonSerializer.Serialize(ds.QFChartList);

                // Define SQL query with parameterized JSON
                var sql = @"
            INSERT INTO dashboard (clusterid, createdon, dashname, dashtype, chartinfo)
            VALUES (@ClusterId, @CreatedOn, @DashName, @DashType, @ChartInfo::jsonb)
            RETURNING dashid;";

                var parameters = new
                {
                    ClusterId = ds.clusterId,
                    CreatedOn = DateTime.UtcNow,
                    DashName = ds.dashName,
                    DashType = ds.dashType,
                    ChartInfo = jsonData // Serialized JSON string
                };

                try
                {
                    // Execute query and get the inserted dashboard ID
                    var dashboardId = connection.ExecuteScalar<Guid>(sql, parameters);
                    return Ok(dashboardId);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }



        [HttpPost]
        [Route("UpdateDashboard")]
        public IActionResult UpdateDashboard(Dashboard request)
        {
            var updates = new List<string>();
            var parameters = new DynamicParameters();

            // Update `dashName` if provided
            if (!string.IsNullOrEmpty(request.dashName))
            {
                updates.Add("dashname = @DashName");
                parameters.Add("DashName", request.dashName);
            }

            // Update `dashType` if provided
            if (!string.IsNullOrEmpty(request.dashType))
            {
                updates.Add("dashtype = @DashType");
                parameters.Add("DashType", request.dashType);
            }

            // Serialize `QFChartList` and update it
            if (request.QFChartList != null && request.QFChartList.Count > 0)
            {
                var chartListJson = JsonSerializer.Serialize(request.QFChartList);
                updates.Add("chartinfo = @ChartInfo::jsonb"); // Ensure it's saved as JSONB in PostgreSQL
                parameters.Add("ChartInfo", chartListJson, DbType.String);
            }

            // Build the SQL query
            var sql = $"UPDATE dashboard SET {string.Join(", ", updates)} WHERE clusterid = @ClusterId AND dashid = @DashId";
            parameters.Add("ClusterId", request.clusterId);
            parameters.Add("DashId", request.dashid);

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
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
