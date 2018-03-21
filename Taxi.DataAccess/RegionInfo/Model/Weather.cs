using System;
using System.Collections.Generic;
using Taxi.DataAccess.Interfaces;

namespace Taxi.DataAccess.RegionInfo.Model
{
    public class Weather : IHaveErrorInfo
    {
        public int RegionId { get; set; }

        public string RegionName { get; set; }

        public string CountryName { get; set; }

        public DateTime Date { get; set; }

        public WeaterDetail Detail { get; set; }

        public IList<DayPart> DayParts { get; set; }

        public bool HasError { get; set; } = false;

        public string ResultMessage { get; set; }
    }
}
