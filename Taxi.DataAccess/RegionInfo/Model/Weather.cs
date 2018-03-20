using System;
using System.Collections.Generic;

namespace Taxi.DataAccess.RegionInfo.Model
{
    public class Weather
    {
        public int RegionId { get; set; }

        public string RegionName { get; set; }

        public string CountryName { get; set; }

        public DateTime Date { get; set; }

        public WeaterDetail Detail { get; set; }

        public IList<DayPart> DayParts { get; set; }

        public bool HasError { get; internal set; } = false;

        public string ResultMessage { get; internal set; }
    }
}
