using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Taxi.DataAccess.RegionInfo;

namespace Taxi.Presentation.Web.Controllers
{
    public class HomeController : Controller
    {
        private static string PathToFakeData => System.Web.HttpContext.Current.Server.MapPath("~/App_Data/FakeReginfo.xml");

        public ActionResult Index(int id = 0)
        {
            ViewBag.Title = "Home Page";

            var regionManager = new RegionInfoManager(PathToFakeData);
            var regionInfo = regionManager.GetRegionInfo(id);

            #region Countries
            var countries = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value="",
                    Selected= true,
                    Text = "Город не выбран"
                }
            };

            countries.AddRange(
                regionManager.GetCountryList()
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Name
                    })
                    .OrderBy(x => x.Text));

            ViewBag.Countries = countries; 
            #endregion

            return View(regionInfo);
        }

        public ActionResult GetRegionInfoDetail(int id) =>
            PartialView("_RegionInfoDetail", new RegionInfoManager().GetRegionInfo(id));

        [HttpGet]
        public JsonResult GetCities(string id)
        {
            var result = new RegionInfoManager()
                .GetCityList(id)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.RegionId.ToString()
                })
                .OrderBy(x => x.Text)
                .ToList();

            return Json(new SelectList(result, "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
    }
}
