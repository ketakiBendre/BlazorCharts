using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Npgsql;
using Quantaflare.Data;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static MudBlazor.CategoryTypes;



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
                //connection.Open();
                var energyStream = connection.Query<EnergyStream>("SELECT * FROM streams");
                return Ok(energyStream);
            }
        }

        [HttpGet]
        [Route("getStreams")]

        public async Task<IActionResult> getStreams(int inputId)
        {
            var streams = await GetStreamsId(inputId); 
            return Ok(streams);
        }

        private async Task<IEnumerable<Streams>> GetStreamsId(int inputId)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                 db.Open();
                var result = await db.QueryAsync<Streams>("SELECT * FROM public.get_streams(@inputId)", new { inputId });
                return result; // Ensure you return the result here
            }
        }

        [HttpGet]
        [Route("getStreamAndField")]

        public async Task<IActionResult> getStreamAndField(int inputId)
        {
            var streamAndField = await GetStreamsbyId(inputId);
            return Ok(streamAndField);
        }

        private async Task<IEnumerable<EnergyStream>> GetStreamsbyId(int inputId)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();
                var result = await db.QueryAsync<EnergyStream>("SELECT * FROM public.getStreamAndField(@inputId)", new { inputId });
                return result; // Ensure you return the result here
            }
        }

        [HttpGet]
        [Route("getEnergyField")]
        public IActionResult getEnergyField()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                //connection.Open();
                var energyField = connection.Query<EnergyStream>("SELECT * FROM energy");

                return Ok(energyField);
            }
        }

        [HttpPost]
        [Route("getChartInfo")]

        public async Task<IActionResult> getChartInfo([FromBody] List<LineChartData> lineChartList)
        {
            var data = await GetChartInfoFunc(lineChartList);
            return Ok(data);
        }

        private async Task<List<Dictionary<string, object>>> GetChartInfoFunc([FromBody] List<LineChartData> lineChartList)
        {
            List<string> fields = new  List<string>();
            List <string> agr=new List<string>();
            string streamname = string.Empty;

            foreach(LineChartData lineChart in lineChartList)
            {
                fields.Add(lineChart.field.ToLower());
                agr.Add(lineChart.agr.ToLower());
                streamname = lineChart.stream;
            }

            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();
                var parameters = new
                {
                    fields = fields,
                    agr = agr,
                    stream = streamname
                };
                var result = await db.QueryAsync("SELECT * FROM public.getchartdata(@fields, @agr, @stream)",parameters);
                
                var dynamicResults = new List<Dictionary<string, object>>();

                foreach (var row in result)
                {
                    var dictionary = new Dictionary<string, object>();

                    // Iterate through each property of the dynamic object
                    foreach (var property in (IDictionary<string, object>)row)
                    {
                        dictionary[property.Key] = property.Value;
                    }

                    dynamicResults.Add(dictionary);
                }

                return dynamicResults;
            }
        }

        [HttpPost]
        [Route("PostQFChart")]
        public async Task<IActionResult> PostQFChart([FromBody] QFChart qfChart)
        {
            if (qfChart == null)
            {
                return BadRequest("Invalid chart data.");
            }
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
            var jsonData = JsonSerializer.Serialize<List<ChartDataStream>>(qfChart.chartDataStreamList, options);
           // var jsonDeString = JsonSerializer.Deserialize<List<ChartDataStream>>(jsonData);
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var sql = "INSERT INTO qfchart (charttitle, charttype, data) VALUES (@charttitle, @charttype, @jsonData::json) RETURNING chartid;";

                var parameters = new
                {
                    charttitle = qfChart.chartTitle,
                    charttype = qfChart.chartType.ToString(),
                    jsonData
                };

                // Execute the query and retrieve the generated chartid
                try
                {
                    // Execute the query and retrieve the generated chartid
                    var chartId = await connection.ExecuteScalarAsync<int>(sql, parameters);
                    return Ok(chartId); 
                }
                catch (Exception ex)
                {
                    // Log the exception (optional) and return an error response
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }

        [HttpGet]
        [Route("GetQFChart")]
        public async Task<IActionResult> GetQFChart([FromQuery] string chartId)
        {
            if (string.IsNullOrWhiteSpace(chartId))
            {
                return BadRequest("No chart IDs provided.");
            }

            // Split the string into an array of integers and handle potential parsing errors
            int[] chartIdArray;
            try
            {
                chartIdArray = chartId.Split(',').Select(int.Parse).ToArray();
            }
            catch (FormatException ex)
            {
                return BadRequest("Invalid chart ID format: " + ex.Message);
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                try
                {
                    var qfChart = await connection.QueryAsync<QFChart>(
                        "SELECT chartid, charttitle, charttype, data AS chartDataStreamJson FROM qfchart WHERE chartid = ANY(@chartId);",
                        new { chartId = chartIdArray }
                    );

                    // Convert to list
                    var qfChartList = qfChart.ToList();

                    // Deserialize JSON if charts were found
                    foreach (var chart in qfChartList)
                    {
                        chart.DeserializeChartDataStreamJson();
                    }

                    return Ok(qfChartList); // Return the list of charts
                }
                catch (Exception ex)
                {
                    // Log the exception (you could use a logging framework)
                    return StatusCode(500, "Internal server error: " + ex.Message);
                }
            }
        }

        private async Task<List<Dictionary<string, object>>> getChartInfoAsync([FromBody] List<EnergyStream> eStreamList)
        {
            string GroupByFields = "skv.timestamp";
            var aggregateQuery = new StringBuilder();
            var sql_query = new StringBuilder();
            var parameters = new DynamicParameters();

            foreach (var eStream in eStreamList)
            {
                if (!string.IsNullOrEmpty(eStream.Fields) && !string.IsNullOrEmpty(eStream.agr))
                {
                    aggregateQuery.Append($", {eStream.agr.ToLower()}(skv.keyvalue) AS {eStream.agr}{eStream.Fields}");
                }
                if (!string.IsNullOrEmpty(eStream.Streams))
                {
                    sql_query.Append($" AND s.streamname = \'{eStream.Streams}\'");
                    parameters.Add("@streams", eStream.Streams);
                }

                if (!string.IsNullOrEmpty(eStream.Fields))
                {
                    sql_query.Append($" AND sk.key = \'{eStream.Fields.ToLower()}\'");
                    parameters.Add("@key", eStream.Fields.ToLower());
                }
            }
            var sql = new StringBuilder($"SELECT {GroupByFields} AS time {aggregateQuery} FROM streams s,streamkeys sk,streamkeyvalues skv WHERE sk.streamkeyid =skv.streamkeyid AND s.streamid = sk.streamid ");

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                sql_query.Append($" GROUP BY {GroupByFields} ORDER BY {GroupByFields}");
                sql.Append(sql_query);
                Console.WriteLine($"Received Cluster ID: {sql}");
                var result = await connection.QueryAsync(sql.ToString());
                var dynamicResults = new List<Dictionary<string, object>>();

                foreach (var row in result)
                {
                    var dictionary = new Dictionary<string, object>();

                    // Iterate through each property of the dynamic object
                    foreach (var property in (IDictionary<string, object>)row)
                    {
                        dictionary[property.Key] = property.Value;
                    }

                    dynamicResults.Add(dictionary);
                }

                return dynamicResults;
            }
        }

        [HttpPost]
        [Route("getChartInfo_working")]
        public async Task<IActionResult> getChartInfo_working([FromBody] List<EnergyStream> eStreamList)
        {
            var chartInfo = await getChartInfoByAgr(eStreamList);
            return Ok(chartInfo);
        }
        private async Task<IEnumerable<ChartDataResult>> getChartInfoByAgr(List<EnergyStream> eStreamList)
        {
            string GroupByFields = "skv.timestamp";
            var aggregateQuery = new StringBuilder();
            var sql_query = new StringBuilder();
            var parameters = new DynamicParameters();

            foreach (var eStream in eStreamList)
            {
                if (!string.IsNullOrEmpty(eStream.Fields) && !string.IsNullOrEmpty(eStream.agr))
                {
                    aggregateQuery.Append($", {eStream.agr.ToLower()}(skv.keyvalue) AS {eStream.agr}{eStream.Fields}");
                }
                if (!string.IsNullOrEmpty(eStream.Streams))
                {
                    sql_query.Append($" AND s.streamname = \'{eStream.Streams}\'");
                    parameters.Add("@streams", eStream.Streams);
                }

                if (!string.IsNullOrEmpty(eStream.Fields))
                {
                    sql_query.Append($" AND sk.key = \'{eStream.Fields.ToLower()}\'");
                    parameters.Add("@key", eStream.Fields.ToLower());
                }
            }
            var sql = new StringBuilder($"SELECT {GroupByFields} AS time {aggregateQuery} FROM streams s,streamkeys sk,streamkeyvalues skv WHERE sk.streamkeyid =skv.streamkeyid AND s.streamid = sk.streamid ");
         
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                sql_query.Append($" GROUP BY {GroupByFields} ORDER BY {GroupByFields}");
                sql.Append(sql_query);
                Console.WriteLine($"Received Cluster ID: {sql}");
                var chartInfo = await connection.QueryAsync<ChartDataResult>(sql.ToString());
                return chartInfo;
            }
        }

    }
}
