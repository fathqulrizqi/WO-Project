using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NGKBusi.Models;
using Microsoft.AspNet.Identity;
using NGKBusi.Areas.Purchasing.Models;

namespace NGKBusi.Areas.Purchasing.Controllers
{
    public class WorkingOrderController : Controller
    {
        DefaultConnection db = new DefaultConnection();
        WOConnection dbm = new WOConnection();

        public ActionResult Index()
        {
            var currUserId = User.Identity.GetUserId();
            var now = DateTime.Now.Date;
            string date = now.ToString("dd-MM-yyyy");


            ViewBag.currUsr = currUserId;
            ViewBag.currDate = date;

            return View();
        }

        public JsonResult GetDatas()
        {
            var datas = dbm.FA_WO
                .Select(data => new
                {
                    data.ID,
                    data.Date,
                    data.Number,
                    data.Vendor,
                    data.Subject,
                    data.NIK
                }).ToList();

            return Json(new { datas = datas }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVendorData()
        {
            var vendorData = db.AX_Vendor_List
                .Select(vd => new
                {
                    vd.Name
                }).ToList();

            return Json(new { datas = vendorData }, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult CreateData(int ID)
        //{
        //    var date = Request["Date"];
        //    var usernik = Request["NIK"];


        //    return Json(new { datas = datas }, JsonRequestBehavior.AllowGet);

        //}
    }
}