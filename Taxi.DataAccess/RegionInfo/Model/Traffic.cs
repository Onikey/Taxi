namespace Taxi.DataAccess.RegionInfo.Model
{
    public class Traffic
    {
        public int RegionId { get; set; }

        public string RegionName { get; set; }

        public int TrafficLevel { get; set; }

        public string TrafficColor { get; set; }

        public string Time { get; set; }

        public string Comment { get; set; }

        public string ResultMessage { get; set; }

        public bool HasError { get; set; } = false;
    }
}
