using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Npgsql;
using BlazorCharts.Data;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Transactions;
using static MudBlazor.CategoryTypes;



namespace BlazorCharts.API.Controllers
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
        [Route("GetChartName")]
        public async Task<bool> GetChartName(string title)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"
        SELECT COUNT(*)
        FROM QFChart
        WHERE chartTitle = @Title;";

            var count = await connection.ExecuteScalarAsync<int>(query, new { Title = title });

            return count > 0; // Returns true if the title exists
        }


            [HttpPost]
        [Route("getLineChartInfo")]

        public async Task<IActionResult> getLineChartInfo([FromBody] List<LineChartData> lineChartList)
        {
            var data = await GetLineChartInfoFunc(lineChartList);
            return Ok(data);
        }

        private async Task<List<Dictionary<string, object>>> GetLineChartInfoFunc([FromBody] List<LineChartData> lineChartList)
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
        [Route("getTimeChartInfo")]
        public async Task<IActionResult> getTimeChartInfo([FromBody] List<ChartDataStream> timeChartList,
                                                          [FromQuery] DateTimeOffset startTime,
                                                          [FromQuery] DateTimeOffset endTime)
        {
            var data = await GetTimeChartInfoFunc(timeChartList, startTime, endTime);
            return Ok(data);
        }

        private async Task<List<Dictionary<string, object>>> GetTimeChartInfoFunc(
            List<ChartDataStream> timeChartList,
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            List<string> fields = new List<string>();
            string streamname = string.Empty;

            // Prepare fields and stream name
            foreach (ChartDataStream timeChart in timeChartList)
            {
                fields.Add(timeChart.field.ToLower());
                streamname = timeChart.stream;
            }

            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                db.Open();
                var parameters = new
                {
                    fields = fields.ToArray(), // Use array for compatibility with the stored procedure
                    stream = streamname,
                    start_time = startTime.ToUniversalTime(), // Format to match PostgreSQL
                    end_time = endTime.ToUniversalTime()
                };

                // Execute the stored procedure
                var result = await db.QueryAsync(
                    "SELECT * FROM public.gettimechartdata(@fields, @stream, @start_time, @end_time)", parameters);

                var dynamicResults = new List<Dictionary<string, object>>();

                foreach (var row in result)
                {
                    var dictionary = new Dictionary<string, object>();

                    // Convert dynamic result into a dictionary
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
        [Route("getMapInfo")]
        public async Task<IActionResult> getMapInfo([FromBody] List<ChartDataStream> timeChartList,
                                                    [FromQuery] DateTimeOffset startTime,
                                                    [FromQuery] DateTimeOffset endTime)
        {
            try
            {
                if (timeChartList == null || !timeChartList.Any())
                {
                    Console.WriteLine("Error: No stream names provided.");
                    return BadRequest("Stream list cannot be empty.");
                }

                var data = await getMapInfoFunc(timeChartList, startTime, endTime);

                if (!data.Any())
                {
                    Console.WriteLine("No location data found.");
                }

                Console.WriteLine($"Returning {data.Count()} records.");
                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in getMapInfo: {ex.Message}");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        private async Task<List<LocationDetails>> getMapInfoFunc(
            List<ChartDataStream> mapDataList,
            DateTimeOffset startTime,
            DateTimeOffset endTime)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var streamNames = mapDataList.Select(m => m.stream).Distinct().ToArray();

            var query = @"
SELECT latitude, longitude
FROM get_map_info(@StreamNames, @StartTime, @EndTime);";

            var parameters = new
            {
                StreamNames = streamNames,
                StartTime = startTime.UtcDateTime,
                EndTime = endTime.UtcDateTime
            };

            try
            {
                Console.WriteLine($"Executing Query with StartTime: {parameters.StartTime}, EndTime: {parameters.EndTime}");
                var result = await connection.QueryAsync<LocationDetails>(query, parameters);

                Console.WriteLine($"Fetched {result.Count()} records from DB");
                foreach (var loc in result)
                {
                    Console.WriteLine($"Lat: {loc.Latitude}, Long: {loc.Longitude}");
                }

                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Query Error: {ex.Message}");
                return new List<LocationDetails>(); // Return empty list instead of null to avoid NullReferenceException
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

        [HttpPost]
        [Route("PostLocDetails")]
        public async Task<IActionResult> PostLocDetails([FromBody] List<LocationDetails> locDetails)
        {
            if (locDetails == null || !locDetails.Any())
            {
                return BadRequest("Invalid or empty location data.");
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            // Serialize for logging or other purposes if needed
            var jsonData = JsonSerializer.Serialize(locDetails, options);

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // 1️⃣ Get streamId for "Location"
                        string streamIdQuery = "SELECT streamId FROM streams WHERE streamname = @StreamName";
                        var streamId = await connection.ExecuteScalarAsync<int>(streamIdQuery, new { StreamName = "Location" });

                        if (streamId == 0)
                            throw new Exception("StreamId not found for Location");

                        // 2️⃣ Get streamkeyid for "Lat" and "Long"
                        string streamKeyQuery = "SELECT streamkeyid, key FROM streamkeys WHERE streamid = @StreamId AND key IN ('Lat', 'Long')";
                        var streamKeys = await connection.QueryAsync<(int StreamKeyId, string Key)>(streamKeyQuery, new { StreamId = streamId });

                        var latKeyId = streamKeys.FirstOrDefault(k => k.Key == "Lat").StreamKeyId;
                        var longKeyId = streamKeys.FirstOrDefault(k => k.Key == "Long").StreamKeyId;

                        if (latKeyId == 0 || longKeyId == 0)
                            throw new Exception("StreamKeyId not found for Lat/Long");

                        // 3️⃣ Insert into streamkeyvalues for each location in the list
                        string insertQuery = @"
                    INSERT INTO streamkeyvalues (timestamp, streamid, streamkeyid, keyvalue) 
                    VALUES (@Timestamp, @StreamId, @StreamKeyId, @KeyValue)";


                        var timestamp = DateTime.UtcNow;

                        foreach (var location in locDetails)
                        {
                            // Insert Latitude for each location
                            await connection.ExecuteAsync(insertQuery, new
                            {
                                Timestamp = timestamp,
                                StreamId = streamId,
                                StreamKeyId = latKeyId,
                                KeyValue = location.Latitude
                            }, transaction);

                            // Insert Longitude for each location
                            await connection.ExecuteAsync(insertQuery, new
                            {
                                Timestamp = timestamp,
                                StreamId = streamId,
                                StreamKeyId = longKeyId,
                                KeyValue = location.Longitude
                            }, transaction);
                        }

                        // Commit the transaction
                        await transaction.CommitAsync();

                    }
                    catch (Exception ex)
                    {
                        // Rollback in case of error
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error: {ex.Message}");
                        return StatusCode(500, $"Internal server error: {ex.Message}");
                    }
                }
            }

            return Ok("Location details successfully inserted.");
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

    }
}
