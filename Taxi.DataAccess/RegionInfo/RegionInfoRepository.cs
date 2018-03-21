using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Taxi.DataAccess.Interfaces;
using Taxi.DataAccess.RegionInfo.Model;

namespace Taxi.DataAccess.RegionInfo
{
    internal class RegionInfoRepository
    {
        #region private fields

        private const string regionInfoEndpointWithParametr = "https://export.yandex.com/bar/reginfo.xml?region={0}";
        private const string regionInfoEndpointWithDefault = "https://export.yandex.com/bar/reginfo.xml?region=143";
        private const string citiesEndpoint = "https://pogoda.yandex.ru/static/cities.xml";

        private string _pathToFakeData;

        private XDocument _document;

        private bool _isRegionConvertedToCity = false;

        #endregion

        public RegionInfoRepository(string pathToFakeData = null)
        {
            this._pathToFakeData = pathToFakeData;
        }

        #region country and city list

        public IEnumerable<Country> GetCountryList()
        {
            try
            {
                return Policy.Handle<System.Xml.XmlException>()
                        .Or<System.Net.WebException>()
                        .Retry(3)
                        .Execute(() => XDocument.Load(citiesEndpoint)
                                        .Element("cities")
                                        .Elements("country")
                                        .Select(country => new Country
                                        {
                                            Name = country.Attribute("name").Value,
                                            Cities = country.Elements("city")?.Select(x => new City
                                            {
                                                RegionId = int.Parse(x.Attribute("region").Value),
                                                Name = x.Value
                                            }).ToList()
                                        })
                        );
            }
            catch (Exception ex) when (
                ex is System.Xml.XmlException ||
                ex is System.Net.WebException
            )
            {
                if (_pathToFakeData == null)
                    throw ex;

                return new List<Country>
                {
                    new Country
                    {
                        Name = "Украина",
                        Cities = new List<City>
                        {
                            new City
                            {
                                Name = "Киев",
                                RegionId = 143
                            }
                        }
                    }
                };
            }
        }

        public IEnumerable<City> GetCityList(string countryName) =>
            GetCountryList()
                .Single(x => x.Name == countryName)
                .Cities;

        #endregion

        #region region information

        public Info GetRegionInfo()
            => new Info
            {
                Weater = GetWeather(),
                Traffic = GetTraffic()
            };

        public Info GetRegionInfo(int regionCode)
            => new Info
            {
                Traffic = GetTraffic(regionCode),
                Weater = GetWeather(regionCode)
            };

        public Traffic GetTraffic(int regionCode = 0)
        {
            var result = new Traffic();

            if (_document == null)
            {
                DownloadDocument(regionCode, result);
            }

            var info = _document.Element("info");
            var regionInfo = info.Element("region");

            if (regionInfo == null)
            {
                result.ResultMessage = $"We can't find information {(regionCode != 0 ? "for region code: " + regionCode : "default")}.";
                result.HasError = true;
                return result;
            }

            result.RegionId = int.Parse(regionInfo.Attribute("id").Value);
            result.RegionName = regionInfo.Element("title").Value;

            var regionTraffic = info.Element("traffic")?.Element("region");
            if (regionTraffic == null)
            {
                if (_isRegionConvertedToCity)
                {
                    result.ResultMessage = $"Traffic information for {result.RegionName} isn't available.";
                    result.HasError = true;
                    return result;
                }
                else
                {
                    regionCode = int.Parse(info.Element("weather").Attribute("region").Value);
                    DownloadDocument(regionCode, result);
                    _isRegionConvertedToCity = true;
                    return GetTraffic(regionCode);
                }
            }

            result.Time = regionTraffic.Element("time").Value;
            result.TrafficLevel = int.Parse(regionTraffic.Element("level").Value);
            result.TrafficColor = regionTraffic.Element("icon").Value;
            result.Comment = regionTraffic.Elements("hint").Single(x => x.Attribute("lang").Value == "ru").Value;

            result.ResultMessage = result.ResultMessage ?? "Success.";

            return result;
        }

        public Weather GetWeather(int regionCode = 0)
        {
            var result = new Weather();

            if (_document == null)
            {
                DownloadDocument(regionCode, result);
            }

            var info = _document.Element("info");
            var regionInfo = info.Element("region");

            if (regionInfo == null)
            {
                result.ResultMessage = $"We can't find information {(regionCode != 0 ? "for region code: " + regionCode : "default")}.";
                result.HasError = true;
                return result;
            }

            result.RegionId = int.Parse(regionInfo.Attribute("id").Value);
            result.RegionName = regionInfo.Element("title").Value;

            var weather = info.Element("weather")?.Element("day");
            if (weather == null)
            {
                result.ResultMessage = $"Weather information for {result.RegionName} isn't available.";
                result.HasError = true;
                return result;
            }

            result.CountryName = weather.Element("country").Value;
            result.Date = DateTime.Parse(weather.Element("date").Attribute("date").Value);

            result.DayParts = new List<DayPart>();
            foreach (var item in weather.Elements("day_part"))
            {
                if (item.Element("weather_type")?.Value != null)
                {
                    result.Detail = new WeaterDetail
                    {
                        Description = item.Element("weather_type").Value,
                        Temperature = int.Parse(item.Element("temperature").Value),
                        ImageUrl = item.Element("image-v3").Value
                    };
                    continue;
                }

                result.DayParts.Add(new DayPart
                {
                    Title = item.Attribute("type").Value,
                    ImageUrl = item.Element("image-v3").Value,
                    TemperatureMin = item.Element("temperature_from")?.Value,
                    TemperatureMax = item.Element("temperature_to")?.Value
                });
            }

            return result;
        }

        #endregion

        #region download document
        private void DownloadDocument(int regionCode, IHaveErrorInfo item)
        {
            if (regionCode == 0)
            {
                DownloadDocument(item);
                return;
            }

            DownloadDocument(string.Format(regionInfoEndpointWithParametr, regionCode), item);
        }

        private void DownloadDocument(IHaveErrorInfo item)
            => DownloadDocument(regionInfoEndpointWithDefault, item);

        private void DownloadDocument(string path, IHaveErrorInfo item)
        {
            try
            {
                Policy.Handle<System.Xml.XmlException>()
                        .Or<System.Net.WebException>()
                        .Retry(3)
                        .Execute(() => _document = XDocument.Load(path));
            }
            catch (Exception ex) when (
                ex is System.Xml.XmlException ||
                ex is System.Net.WebException
            )
            {
                if (_pathToFakeData != null)
                {
                    item.HasError = true;
                    item.ResultMessage = ex.Message;
                    _document = XDocument.Load(_pathToFakeData);
                }
                else
                {
                    throw ex;
                }
            }
        }

        #endregion
    }
}
