using System.Collections.Generic;

namespace Taxi.DataAccess.RegionInfo.Model
{
    public class Country
    {
        public string Name { get; set; }

        public List<City> Cities { get; set; }
    }
}
