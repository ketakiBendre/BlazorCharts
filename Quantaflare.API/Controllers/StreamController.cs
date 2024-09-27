using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Quantaflare.Data;
using System.Data;

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

        [HttpGet]
        [Route("getChartInfo")]
        public IActionResult getChartInfo()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                //connection.Open();
                var sql = @"SELECT time, avg(voltage) as voltage FROM chartinfo group by time order by time asc";
                var chartInfo = connection.Query<ChartInfo>(sql);
                return Ok(chartInfo);
            }
        }

    }
}
