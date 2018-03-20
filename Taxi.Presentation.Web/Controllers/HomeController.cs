using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Taxi.DataAccess.RegionInfo;

namespace Taxi.Presentation.Web.Controllers
{
    public class HomeController : Controller
    {
        private static string PathToFakeData => System.Web.HttpContext.Current.Server.MapPath("~/FakeReginfo.xml");

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            //set true as parametr to manager if don't have access to yandex
            ViewBag.RegionInfo = new RegionInfoManager().GetRegionInfo();

            return View();
        }
    }
}
