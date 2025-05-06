using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NGKBusi.Models;
using Microsoft.AspNet.Identity;
using System.Globalization;
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

            var user = db.V_Users_Active.FirstOrDefault(u => u.NIK == currUserId);

            ViewBag.vendorList = db.AX_Vendor_List.Where(w => w.IsActive == true && w.VENDGROUP != "OTH").ToList();
            ViewBag.currUsr = currUserId;
            ViewBag.currDate = date;
            ViewBag.currUsrName = user?.Name ?? "unknown";

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

        public String toRomawi(string month)
        {
            var romawi = "";
            switch (month)
            {
                case "01":
                    romawi = "I";
                    break;
                case "02":
                    romawi = "II";
                    break;
                case "03":
                    romawi = "III";
                    break;
                case "04":
                    romawi = "IV";
                    break;
                case "05":
                    romawi = "V";
                    break;
                case "06":
                    romawi = "VI";
                    break;
                case "07":
                    romawi = "VII";
                    break;
                case "08":
                    romawi = "VIII";
                    break;
                case "09":
                    romawi = "IX";
                    break;
                case "10":
                    romawi = "X";
                    break;
                case "11":
                    romawi = "XI";
                    break;
                case "12":
                    romawi = "XII";
                    break;
                default:
                    break;

            }
            return romawi;
        }

        public JsonResult CreateData()
        {
            
                var dateStr = Request["Date"];
                string format = "dd-MM-yyyy";
                DateTime date;

            if (!DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return Json(new { success = false, message = "Invalid date format. Please use 'dd-MM-yyyy'." }, JsonRequestBehavior.AllowGet);
            }

                var userNik = Request["NIK"];
                var vendor = Request["selVendorList"];
                var subject = Request["Subject"];

                var year = DateTime.Now.ToString("yyyy");

                string month = date.ToString("MM");

                string romanMonth = toRomawi(month);

                var lastNumber = dbm.FA_WO
                    .Where(w => w.Number.EndsWith(year) && w.Number.Contains("/Niterra/"))
                    .OrderByDescending(o => o.Number)
                    .Select(s => s.Number)
                    .FirstOrDefault();

                string sequenceNumber;
                if (string.IsNullOrEmpty(lastNumber))
                {
                    sequenceNumber = "0001";
                }
                else
                {
                    var currentNumber = lastNumber.Split('/')[0];
                    sequenceNumber = (int.Parse(currentNumber) + 1).ToString("D4");
                }

                // Construct the new number using the Roman numeral for the month
                var newNumber = $"{sequenceNumber}/WO/Niterra/{romanMonth}/{year}";

                FA_WO data = new FA_WO
                {
                    Date = date,
                    Number = newNumber,
                    Vendor = vendor,
                    Subject = subject,
                    NIK = userNik,
                    Timestamps = DateTime.Now
                };

                dbm.FA_WO.Add(data);
                dbm.SaveChanges();

                return Json(new { success = true, message = "Working Order created successfully" }, JsonRequestBehavior.AllowGet);
           
        }

    }
}