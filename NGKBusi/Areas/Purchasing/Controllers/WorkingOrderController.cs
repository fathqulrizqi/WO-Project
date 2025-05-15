using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using NGKBusi.Areas.Purchasing.Models;
using NGKBusi.Areas.WebService.Models;
using NGKBusi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.PeerToPeer;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Windows.Interop;
using static System.Data.Entity.Infrastructure.Design.Executor;

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
            ViewBag.budgetList = db.V_FA_BudgetSystem_BEX_BEL.Where(w => w.Latest == 1).ToList();
            ViewBag.currUsr = currUserId;
            ViewBag.currDate = date;
            ViewBag.currUsrName = user?.Name ?? "";

            return View();
        }



        [HttpGet]
        public JsonResult GetDatas()
        {
            var datas = dbm.Purchasing_WorkingOrder_List
                .AsEnumerable()
                .Select(data => new
                {
                    data.ID,
                    Date = data.Date.ToString("yyyy-MM-dd"),
                    data.Number,
                    data.Vendor,
                    data.VendorName,
                    data.Subject,
                    data.NIK,
                    data.NIKName,
                }).ToList();

            return Json(new { datas = datas }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDataById(int id)
        {

            var datas = dbm.Purchasing_WorkingOrder_List
                .Where(w => w.ID == id)
                .AsEnumerable()
                .Select(data => new
                {
                    data.ID,
                    Date = data.Date.ToString("yyyy-MM-dd"),
                    data.Number,
                    data.Vendor,
                    data.VendorName,
                    data.Subject,
                    data.NIK,
                    data.NIKName,
                    Timestamps = data.Timestamps.ToString("yyyy-MM-dd HH:mm:ss"),
                }).FirstOrDefault();

            return Json(new { datas }, JsonRequestBehavior.AllowGet);
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

        [HttpPost]
        public JsonResult CreateData()
        {

            DateTime date;
            if (!ParseDateFromRequest("Date", out date))
            {
                return Json(new { success = false, message = "Invalid date format. Please use 'dd-MM-yyyy'." }, JsonRequestBehavior.AllowGet);
            }

            var userNik = Request["NIK"];
            var un = db.Users.FirstOrDefault(w => w.NIK == userNik);
            var username = un != null ? un.Name : "";

            var vendor = Request["selVendorList"];
            var vn = db.AX_Vendor_List.FirstOrDefault(w => w.ACCOUNTNUM == vendor);
            var vendorname = vn != null ? vn.Name : "";

            var subject = Request["Subject"];

            var year = DateTime.Now.ToString("yyyy");

            string month = date.ToString("MM");

            string romanMonth = toRomawi(month);

            var lastNumber = dbm.Purchasing_WorkingOrder_List
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

            var newNumber = $"{sequenceNumber}/WO/Niterra/{romanMonth}/{year}";

            Purchasing_WorkingOrder_List data = new Purchasing_WorkingOrder_List
            {
                Date = date,
                Number = newNumber,
                Vendor = vendor,
                VendorName = vendorname,
                Subject = subject,
                NIK = userNik,
                NIKName = username,
                Timestamps = DateTime.Now
            };

            dbm.Purchasing_WorkingOrder_List.Add(data);
            int addWO = dbm.SaveChanges();

            if (addWO > 0)
            {
                return Json(new { success = true, message = "Working Order Added Successfully" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Failed to Add Working Order" }, JsonRequestBehavior.AllowGet);
            }

        }

        private bool ParseDateFromRequest(string key, out DateTime date)
        {
            date = default(DateTime);
            string dateStr = Request[key];
            string format = "dd-MM-yyyy";

            return DateTime.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }


        [HttpPost]
        public JsonResult UpdateData(int id)
        {
            var dateStr = Request["Date"];

            var userNik = Request["NIK"];
            var un = db.Users.FirstOrDefault(w => w.NIK == userNik);
            var username = un != null ? un.Name : "";

            var vendor = Request["selVendorList"];
            var vn = db.AX_Vendor_List.FirstOrDefault(w => w.ACCOUNTNUM == vendor);
            var vendorname = vn != null ? vn.Name : "";

            var subject = Request["Subject"];
            var number = Request["Number"];
            var timestamps = Request["Timestamps"];

            DateTime date;
            if (!DateTime.TryParseExact(dateStr, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return Json(new { success = false, message = "Invalid date format. Please use 'dd-MM-yyyy'." }, JsonRequestBehavior.AllowGet);
            }
            var data = dbm.Purchasing_WorkingOrder_List.First(w => w.ID == id);

            data.Timestamps = DateTime.Now;
            data.ID = id;
            data.Date = date;
            data.Vendor = vendor;
            data.VendorName = vendorname;
            data.Subject = subject;
            data.NIK = userNik;
            data.NIKName = username;


            int updateWO = dbm.SaveChanges();

            if (updateWO > 0)
            {
                return Json(new { success = true, message = "Working Order Updated Successfully" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Failed Update Working Order" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Detail(int id)
        {
            ViewBag.budgetList = db.V_FA_BudgetSystem_BEX_BEL.Where(w => w.Latest == 1).ToList();

            var now = DateTime.Now;
            ViewBag.Now = now;

            return View();
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult SaveLetter()
        {
            var idlist = Int32.Parse(Request["IDList"]);
            string number = Request["Number"];
            var vendor = Request["Vendor"];
            var attn = Request["Attn"];
            var refs = Request["Ref"];
            var project = Request["Project"];
            var html = Request["Html"];

            DateTime date;
            if (!ParseDateFromRequest("Date", out date))
            {
                return Json(new { success = false, message = "Invalid date format. Please use 'dd-MM-yyyy'." }, JsonRequestBehavior.AllowGet);
            }

            decimal? total = null;
            var totalRequestValue = Request["Total"];
            if (!string.IsNullOrEmpty(totalRequestValue))
            {
                totalRequestValue = totalRequestValue.Replace(',', '.');
                if (!decimal.TryParse(totalRequestValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedTotal))
                {
                    return Json(new { success = false, message = "Invalid total format. Please input a valid number." }, JsonRequestBehavior.AllowGet);
                }
                total = parsedTotal; 
            }

            var paymentterm = Request["PaymentTerm"];

            DateTime? startdate = null; 
            var startDateRequestValue = Request["StartDate"];
            if (!string.IsNullOrEmpty(startDateRequestValue))
            {
                if (!ParseDateFromRequest("StartDate", out DateTime parsedStartDate))
                {
                    return Json(new { success = false, message = "Invalid date start format. Please use 'dd-MM-yyyy'." }, JsonRequestBehavior.AllowGet);
                }
                startdate = parsedStartDate; 
            }

            DateTime? enddate = null;
            var endDateRequestValue = Request["EndDate"];
            if (!string.IsNullOrEmpty(endDateRequestValue))
            {
                if (!ParseDateFromRequest("EndDate", out DateTime parsedEndDate))
                {
                    return Json(new { success = false, message = "Invalid date end format. Please use 'dd-MM-yyyy'." }, JsonRequestBehavior.AllowGet);
                }
                enddate = parsedEndDate; 
            }

            var budgetno = Request["BudgetNo"];
            var budgets = db.V_FA_BudgetSystem_BEX_BEL.FirstOrDefault(w => w.Budget_No == budgetno);
            var budgetdesc = budgets != null ? budgets.Description : "";
            var remark = Request["Remark"];

            var existing = dbm.Purchasing_WorkingOrder_Letter.FirstOrDefault(w => w.IDList == idlist);
            if (existing != null)
            {
                return Json(new
                {
                    success = false,
                    message = "Data surat sudah pernah disimpan. Silakan gunakan fitur update."
                }, JsonRequestBehavior.AllowGet);
            }

            Purchasing_WorkingOrder_Letter data = new Purchasing_WorkingOrder_Letter
            {
                IDList = idlist,
                Number = number,
                Vendor = vendor,
                Attn = attn,
                Ref = refs,
                Project = project,
                Html = html,
                Date = date,
                Total = total,
                PaymentTerm = paymentterm,
                StartDate = startdate,
                EndDate = enddate,
                BudgetNo = budgetno,
                BudgetDesc = budgetdesc,
                Remark = remark,
                Timestamps = DateTime.Now,
            };

            dbm.Purchasing_WorkingOrder_Letter.Add(data);
            int saveWO = dbm.SaveChanges();

            if (saveWO > 0)
            {
                return Json(new { success = true, message = "Working Order Saved Successfully", id = data.IDList }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Failed to Save Working Order" }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, ValidateInput(false)]
        public JsonResult UpdateLetter(int id)
        {
            string idListStr = Request["IDList"];
            int idlist;
            if (!int.TryParse(idListStr, out idlist))
            {
                return Json(new { success = false, message = "Invalid IDList format" }, JsonRequestBehavior.AllowGet);
            }
            string number = Request["Number"];
            var vendor = Request["Vendor"];
            var attn = Request["Attn"];
            var refs = Request["Ref"];
            var project = Request["Project"];
            var html = Request["Html"];

            DateTime date;
            if (!ParseDateFromRequest("Date", out date))
            {
                return Json(new { success = false, message = "Invalid date format. Please use 'dd-MM-yyyy'." }, JsonRequestBehavior.AllowGet);
            }

            decimal? total = null;
            var totalRequestValue = Request["Total"];
            if (!string.IsNullOrEmpty(totalRequestValue))
            {
                totalRequestValue = totalRequestValue.Replace(',', '.');
                if (!decimal.TryParse(totalRequestValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedTotal))
                {
                    return Json(new { success = false, message = "Invalid total format. Please input a valid number." }, JsonRequestBehavior.AllowGet);
                }
                total = parsedTotal;
            }

            var paymentterm = Request["PaymentTerm"];

            DateTime? startdate = null;
            var startDateRequestValue = Request["StartDate"];
            if (!string.IsNullOrEmpty(startDateRequestValue))
            {
                if (!ParseDateFromRequest("StartDate", out DateTime parsedStartDate))
                {
                    return Json(new { success = false, message = "Invalid date start format. Please use 'dd-MM-yyyy'." }, JsonRequestBehavior.AllowGet);
                }
                startdate = parsedStartDate;
            }

            DateTime? enddate = null;
            var endDateRequestValue = Request["EndDate"];
            if (!string.IsNullOrEmpty(endDateRequestValue))
            {
                if (!ParseDateFromRequest("EndDate", out DateTime parsedEndDate))
                {
                    return Json(new { success = false, message = "Invalid date end format. Please use 'dd-MM-yyyy'." }, JsonRequestBehavior.AllowGet);
                }
                enddate = parsedEndDate;
            }

            var budgetno = Request["BudgetNo"];
            var budgets = db.V_FA_BudgetSystem_BEX_BEL.FirstOrDefault(w => w.Budget_No == budgetno);
            var budgetdesc = budgets != null ? budgets.Description : "";
            var remark = Request["Remark"];

            var data = dbm.Purchasing_WorkingOrder_Letter.First(w => w.ID == id);
            data.IDList = idlist;
            data.Number = number;
            data.Vendor = vendor;
            data.Attn = attn;
            data.Ref = refs;
            data.Project = project;
            data.Html = html;
            data.Date = date;
            data.Total = total;
            data.PaymentTerm = paymentterm;
            data.StartDate = startdate;
            data.EndDate = enddate;
            data.BudgetNo = budgetno;
            data.BudgetDesc = budgetdesc;
            data.Remark = remark;

            int updateWO = dbm.SaveChanges();

            if (updateWO > 0)
            {
                return Json(new { success = true, message = "Working Order Updated Successfully" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Failed Update Working Order" }, JsonRequestBehavior.AllowGet);
            }
        }

        private bool ParseDecimalFromRequest(string key, out decimal value)
        {
            value = 0;
            string valStr = Request[key];

            if (!string.IsNullOrWhiteSpace(valStr))
            {
                valStr = valStr.Replace(",", "").Replace("Rp", "").Trim();
            }

            return decimal.TryParse(valStr, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        public JsonResult GetLetterById(int id)
        {
            var data = dbm.Purchasing_WorkingOrder_Letter.Where(w => w.IDList == id).FirstOrDefault();
            if (data == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            var budget = db.V_FA_BudgetSystem_BEX_BEL.FirstOrDefault(w => w.Budget_No == data.BudgetNo);

            return Json(new
            {
                success = true,
                data = new
                {
                    data.ID,
                    data.IDList,
                    data.Number,
                    data.Vendor,
                    data.Attn,
                    data.Ref,
                    data.Project,
                    data.Html,
                    data.Date,
                    data.Total,
                    data.PaymentTerm,
                    data.StartDate,
                    data.EndDate,
                    BudgetNo = data.BudgetNo,  
                    BudgetDesc = budget != null ? budget.Description : "",
                    data.Remark
                }
            }, JsonRequestBehavior.AllowGet);
        }


    }
}