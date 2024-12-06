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
                var dashboardName = connection.QuerySingleOrDefault<string>("SELECT dashname FROM dashboard WHERE clusterid = @clusterId AND dashname like \'New Dashboard%\' ORDER BY dashid DESC LIMIT 1", new { clusterId });
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
                var dashboardDetail = connection.QuerySingleOrDefault<Dashboard>(
    "SELECT dashid, clusterid, createdon, dashname, dashtype, chartinfo AS QFChartListJson FROM dashboard WHERE clusterid = @clusterId AND dashname = @dashname",
    parameters
);

                if (dashboardDetail != null)
                {
                    dashboardDetail.DeserializeQFChartList(); // Call the method to deserialize the QFChartList
                }
                return Ok(dashboardDetail);
            }
        }

        [HttpPost]
        [Route("PostDashboard")]
        public IActionResult PostDashboard(Dashboard ds)
         {
            
            using (var connection = new NpgsqlConnection(_connectionString))
             {
                connection.Open();
                var jsonData = JsonSerializer.Serialize(ds.QFChartList);
                var sql = @"INSERT INTO dashboard (clusterid, createdon, dashname, dashtype,chartinfo) 
                    VALUES (@clusterId, @createdOn, @dashName, @dashType,@jsonData::json) 
                    RETURNING dashid;";

                var parameters = new
                {
                    clusterId = ds.clusterId,
                    createdOn = DateTime.UtcNow, // Assuming you want to use the current UTC time
                    dashName = ds.dashName,
                    dashType = ds.dashType,
                    jsonData = jsonData
                };

                // Execute the query and capture the returned ID
                var dashboardId = connection.ExecuteScalar<int>(sql, parameters);

                // Return the dashboard ID
                return Ok(dashboardId);
            }
         }

        [HttpPost]
        [Route("UpdateDashboard")]
        public IActionResult UpdateDashboard(Dashboard request)
        {
            var updates = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(request.dashName))
            {
                updates.Add("dashname = @DashName");
                parameters.Add("DashName", request.dashName);
            }

            if (!string.IsNullOrEmpty(request.dashType))
            {
                updates.Add("dashtype = @DashType");
                parameters.Add("DashType", request.dashType);
            }

            if (!string.IsNullOrEmpty(request.QFChartListJson))
            {
                updates.Add("chartinfo = @QFChartListJson::jsonb");
                parameters.Add("QFChartListJson", request.QFChartListJson);
            }

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
