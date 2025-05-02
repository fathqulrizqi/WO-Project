using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NGKBusi.Models;

namespace NGKBusi.Areas.Kaizen.Controllers
{
    public class ReportController : Controller
    {
        DefaultConnection db = new DefaultConnection();
        // GET: Kaizen/Report
        public ActionResult Index()
        {

            Int32 period = (Request["iPeriod"] != null ? Int32.Parse(Request["iPeriod"]) : (DateTime.Now.Month < 4 ? DateTime.Now.Year - 1 : DateTime.Now.Year));
            ViewBag.Period = period;
            var rangeStart = new DateTime(period, 4, 1);
            var rangeEnd = new DateTime(period + 1, 4, 1);
            ViewBag.kaizenList = db.Kaizen_Data.Where(w => w.issuedDate >= rangeStart && w.issuedDate < rangeEnd).OrderByDescending(x => x.ID).ToList();
            ViewBag.OCDList = db.Kaizen_Score_Categories.Where(x => x.groupID == 1).ToList();
            ViewBag.KOCList = db.Kaizen_Score_Categories.Where(x => x.groupID == 2).ToList();
            ViewBag.SCList = db.Kaizen_Score_Categories.Where(x => x.groupID == 3).ToList();
            ViewBag.NavHide = true;
            return View();
        }
    }
}
