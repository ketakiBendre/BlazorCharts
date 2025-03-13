using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using BlazorCharts.Data;
using System.Data;
using System.Data.Common;
using static MudBlazor.CategoryTypes;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;


namespace BlazorCharts.API.Controllers
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
        public async Task<IEnumerable<Dashboard>> getDashboard(int clusterId)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();
                var dashQuery = @"SELECT * FROM dashboard WHERE clusterid = @clusterId ORDER BY dashname ASC";
                var dashList = await db.QueryAsync<Dashboard>(dashQuery, new { clusterId });

                return dashList;
            }
        }


        [HttpGet]
        [Route("GetDashboardName")]
        public async Task<string> GetDashboardName(string newDashName, int clusterId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            bool isNewDashboard = newDashName.StartsWith("New Dashboard", StringComparison.OrdinalIgnoreCase);

            if (isNewDashboard)
            {
                // Find the latest "New Dashboard %" for the given cluster
                var maxExistingDashboardQuery = @"
                     SELECT max(cast(('0' || regexp_replace(dashname, '\D', '', 'g')) as integer)) FROM dashboard 
                    WHERE clusterid = @clusterId AND dashname LIKE 'New Dashboard %'";

                var LatestCount = await connection.ExecuteScalarAsync<int?>(maxExistingDashboardQuery, new { ClusterId = clusterId });

                if (LatestCount.HasValue && LatestCount.Value > 0)
                    return $"New Dashboard {LatestCount.Value + 1}";

                return "New Dashboard 1";  // Default first name if no existing dashboards
            }
            else
            {
                // Check if the custom name already exists within the same cluster
                var existsQuery = @"SELECT COUNT(*) FROM dashboard WHERE clusterid = @clusterId AND dashname = @newDashName";
                var count = await connection.ExecuteScalarAsync<int>(existsQuery, new { ClusterId = clusterId, NewDashName = newDashName });

                return count > 0 ? "DUPLICATE_NAME" : newDashName;
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
            string jsonData;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // Serialize QFChartList to JSON
                if (ds.QFChartList != null && ds.QFChartList.Count > 0)
                {
                    jsonData = JsonSerializer.Serialize(ds.QFChartList);
                }
                else
                {
                   jsonData = "[]"; // Empty JSON array
                }

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

        [HttpDelete]
        [Route("DeleteChart/{clusterId}/{dashName}/{chartId}")]
        public async Task<IActionResult> DeleteChart(int clusterId, string dashName, int chartId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        

                        // Step 1: Delete the chart from QFChart
                        string deleteChartQuery = "DELETE FROM QFChart WHERE chartid = @chartId;";
                        int rowsAffected = await connection.ExecuteAsync(deleteChartQuery, new { chartId }, transaction);

                        if (rowsAffected == 0)
                        {
                            await transaction.RollbackAsync();
                            return NotFound(new { message = "Chart not found in QFChart" });
                        }

                        // Step 2: Remove the chart ID from chartinfo JSON in Dashboard
                        string updateDashboardQuery = @"
                                                        UPDATE Dashboard 
                                        SET chartinfo = (
                                            SELECT json_agg(elem) -- Convert JSONB array back to JSON format
                                            FROM jsonb_array_elements(chartinfo::jsonb) elem
                                            WHERE NOT (elem ? @chartId::text) -- Remove the object containing chartId
                                        )::json -- Ensure the result is cast back to JSON
                                        WHERE clusterid = @clusterId 
                                        AND dashname = @dashName 
                                        AND chartinfo::text LIKE '%' || @chartId || '%';";


                        int updatedRows = await connection.ExecuteAsync(updateDashboardQuery,
                            new { chartId, clusterId, dashName });

                        if (updatedRows == 0)
                        {
                            await transaction.RollbackAsync();
                            return NotFound(new { message = "Chart ID not found in Dashboard's chartinfo" });
                        }

                        await transaction.CommitAsync();
                        return Ok(new { message = "Chart deleted successfully" });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error deleting chart", error = ex.Message });
                }
            }
        }

    }
}
