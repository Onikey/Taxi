using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Taxi.DataAccess.RegionInfo.Model;

namespace Taxi.DataAccess.RegionInfo
{
    internal class RegionInfoRepository
    {
        private const string regionInfoEndpointWithParametr = "https://export.yandex.com/bar/reginfo.xml?region={0}";
        private const string regionInfoEndpointWithDefault = "https://export.yandex.com/bar/reginfo.xml?region=143";

        private XDocument _document;

        public string PathToFakeData { get; }

        public RegionInfoRepository(string pathToFakeData = null)
        {
            this.PathToFakeData = pathToFakeData;
        }

        public Info GetRegionInfo(int regionCode)
            => new Info
            {
                Weater = GetWeather(regionCode),
                Traffic = GetTraffic(regionCode)
            };

        public Info GetRegionInfo()
            => new Info
            {
                Weater = GetWeather(),
                Traffic = GetTraffic()
            };

        public Traffic GetTraffic(int regionCode = 0)
        {
            var result = new Traffic();

            if (_document == null)
            {
                try
                {
                    DownloadDocument(regionCode);
                }
                catch (Exception ex) when (
                    ex is System.Xml.XmlException ||
                    ex is System.Net.WebException
                )
                {
                    result.ResultMessage = ex.Message;
                    result.HasError = true;
                    return result;
                }
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
                result.ResultMessage = $"Traffic information for {result.RegionName} isn't available.";
                result.HasError = true;
                return result;
            }

            result.Time = regionTraffic.Element("time").Value;
            result.TrafficLevel = int.Parse(regionTraffic.Element("level").Value);
            result.TrafficColor = regionTraffic.Element("icon").Value;
            result.Comment = regionTraffic.Elements("hint").Single(x => x.Attribute("lang").Value == "ru").Value;

            result.ResultMessage = "Success.";

            return result;
        }

        public Weather GetWeather(int regionCode = 0)
        {
            var result = new Weather();

            if (_document == null)
            {
                try
                {
                    DownloadDocument(regionCode);
                }
                catch (Exception ex) when (
                    ex is System.Xml.XmlException ||
                    ex is System.Net.WebException
                )
                {
                    result.ResultMessage = ex.Message;
                    result.HasError = true;
                    return result;
                }
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

        private void DownloadDocument(int regionCode)
        {
            if (PathToFakeData != null)
            {
                _document = GenerateFakeXML();
                return;
            }

            if (regionCode == 0)
            {
                DownloadDocument();
                return;
            }

            _document = XDocument.Load(string.Format(regionInfoEndpointWithParametr, regionCode));
        }

        private void DownloadDocument()
        {
            if (PathToFakeData != null)
            {
                _document = GenerateFakeXML();
                return;
            }

            _document = XDocument.Load(regionInfoEndpointWithDefault);
        }

        private XDocument GenerateFakeXML() =>
            XDocument.Load(PathToFakeData);
    }
}
