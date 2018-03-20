using Taxi.DataAccess.RegionInfo.Model;

namespace Taxi.DataAccess.RegionInfo
{
    public class RegionInfoManager
    {
        RegionInfoRepository _repo;

        public RegionInfoManager(string pathToFakeData = null)
        {
            _repo = new RegionInfoRepository(pathToFakeData);
        }

        public Traffic GetTraffic(int regionCode = 0) =>
            _repo.GetTraffic(regionCode);

        public Weather GetWeather(int regionCode = 0) =>
            _repo.GetWeather(regionCode);

        public Info GetRegionInfo(int regionCode = 0) =>
            _repo.GetRegionInfo(regionCode);
    }
}
