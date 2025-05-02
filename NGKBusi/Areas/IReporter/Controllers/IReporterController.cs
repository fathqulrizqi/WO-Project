using Newtonsoft.Json;
using NGKBusi.Areas.IReporter.Models;
using NGKBusi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NGKBusi.Areas.IReporter.Controllers
{
    public class IReporterController : Controller
    {
        IReporterConnection dbIReporter = new IReporterConnection();

        // GET: IReporter/IReporter
        public ActionResult Index()
        {

            ViewBag.NavHide = true;

            return View();
        }

        public JsonResult getIreporterData(string iRepName, DateTime iStartDate, DateTime iEndDate, string iSheetName = "")
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());
            con.Open();

            SqlCommand cmd = new SqlCommand("sp_iReporterReport", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@reportName", iRepName);
            cmd.Parameters.AddWithValue("@sheetName", iSheetName);
            cmd.Parameters.AddWithValue("@startDate", iStartDate);
            cmd.Parameters.AddWithValue("@endDate", iEndDate);
            cmd.CommandTimeout = 30000;
            DataTable dt = new DataTable();
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }
            var result = JsonConvert.SerializeObject(dt,
                       new JsonSerializerSettings
                       {
                           ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                       });
            var fResult = Json(result, JsonRequestBehavior.AllowGet);
            fResult.MaxJsonLength = Int32.MaxValue;

            return fResult;
        }
    }
}