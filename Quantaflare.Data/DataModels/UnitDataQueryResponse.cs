
namespace QFDomain.Entities.Data.Read
{
    public class UnitDataQuery
    {

        //need to identify the bucket
        public int OrgId { get; set; }     //clients will query and cache the following 2 - we will also make them available in the config
        //need to identify the measurement
        public int ClusterId { get; set; }

        //need for the range 
        private static long defaultFromTimeStamp = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();
        private static long defaultToTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public long FromTimeStamp { get; set; } = defaultFromTimeStamp; //what timestamp data is needed for
        public long ToTimeStamp { get; set; } = defaultToTimeStamp;  //What is the end timestamp

        //what stream
        public int StreamId { get; set; } // one streamID at a time - later we will do the multiple stream
                                          //public  List <int> StreamIds { get; set; } = new List<int>(); //which stream the data is needed for

        //what keys - currently returning all the keys for the stream
        //public List <Int64> ? StreamKeyIds { get; set; } = new List<Int64>(); //what keys to be included for the stream 

        public int? PageIndex { get; set; } = 1; //the index of the page to directly jump to 

        public int? PageSize { get; set; } = 10; //the size of the page (default is 10) 

        public string Format { get; set; } = "JSON"; //csv or JSON

    }

    /// <summary>
    /// This object represents a time series data API response for the object UnitDataQuery time series data container 
    /// </summary>
    public class UnitDataQueryResult
    {
        //what is the unit id
        //what stream
        //List of TSDataPoints 
        public string UnitKey { get; set; } //unit API key 
        public int StreamId { get; set; } //what is the streamID 

        public int PointCount { get; set; } = 0; //how many points are available in this result
        public List<StreamKey> StreamKeys { get; set; } = new List<StreamKey>();  //this tells what all keys are available in this result (used for headers)

        public List<TSDataPoint> DataPoints { get; set; } = new List<TSDataPoint>(); //this tells the value for the value of the streamkey(s) at each timestamp
    }

}
