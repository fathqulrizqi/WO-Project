using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using NGKBusi.Areas.HC.Models;
using NGKBusi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace NGKBusi.Areas.HC.Controllers
{
    [Authorize]
    public class CompetencyController : Controller
    {
        CompetencyMapConnection dbCM = new CompetencyMapConnection();
        DefaultConnection db = new DefaultConnection();
        // GET: HC/Competency
        public ActionResult Map()
        {
            var currUser = ((ClaimsIdentity)User.Identity);
            var currUserID = currUser.GetUserId();
            var currUserData = db.V_Users_Active.Where(w => w.NIK == currUserID).FirstOrDefault();
            var currDivName = currUserData?.DivisionName;
            var currDeptName = currUserData?.DeptName;
            var currSectName = currUserData?.SectionName;
            var currCostName = currUserData?.CostName;

            var coll = (from usr in db.Users.DefaultIfEmpty()
                        from rol in usr.Users_Menus_Roles.DefaultIfEmpty()
                        where usr.NIK == currUserID && rol.menuID == 107
                        select new { usr, rol })
                                .AsEnumerable().Select(s => s.usr);
            if (coll.FirstOrDefault() == null)
            {
                return View("UnAuthorized");
            }
            //all Competency map access to Agnes, Kiswoyo & Yogi
            if (currUserID == "834.10.19" || currUserID == "P070624" || currUserID == "592.02.10" || currUserID == "853.07.21")
            {
                ViewBag.DeptList = db.V_Users_Active.Where(w => w.TitleName != null && w.TitleName != "" && w.NIK.Substring(0, 3) != "NGK" && w.NIK.Substring(0, 3) != "EXP" && w.NIK.Substring(0, 3) != "BOD" && w.NIK.Substring(0, 3) != "PKL" && w.NIK != "SCReward").AsEnumerable().GroupBy(g => new { g.DivisionName, g.DeptName, g.SectionName, g.CostName, g.TitleName, g.PositionName }).Select(s => new V_Users_Active { DivisionName = s.Key.DivisionName, DeptName = s.Key.DeptName, SectionName = s.Key.SectionName, CostName = s.Key.CostName, TitleName = s.Key.TitleName, PositionName = s.Key.PositionName }).ToList();
            }
            else
            {
                ViewBag.DeptList = db.V_Users_Active.Where(w => w.DeptName.ToLower() == currDeptName.ToLower() && w.TitleName != null && w.TitleName != "" && w.NIK.Substring(0, 3) != "NGK" && w.NIK.Substring(0, 3) != "EXP" && w.NIK.Substring(0, 3) != "BOD" && w.NIK.Substring(0, 3) != "PKL" && w.NIK != "SCReward").AsEnumerable().GroupBy(g => new { g.DivisionName, g.DeptName, g.SectionName, g.CostName, g.TitleName, g.PositionName }).Select(s => new V_Users_Active { DivisionName = s.Key.DivisionName, DeptName = s.Key.DeptName, SectionName = s.Key.SectionName, CostName = s.Key.CostName, TitleName = s.Key.TitleName, PositionName = s.Key.PositionName }).ToList();
            }

            ViewBag.NavHide = true;

            return View();
        }
        public ActionResult Result()
        {
            var currUser = ((ClaimsIdentity)User.Identity);
            var currUserID = currUser.GetUserId();
            var currUserData = db.V_Users_Active.Where(w => w.NIK == currUserID).FirstOrDefault();
            var currDivName = currUserData?.DivisionName;
            var currDeptName = currUserData?.DeptName;
            var currSectName = currUserData?.SectionName;
            var periodFY = "FY1" + (!String.IsNullOrEmpty(Request["iPAPeriod"]) ? Request["iPAPeriod"].ToString().Substring(2, 2) : (DateTime.Now.Month < 10 ? (DateTime.Now.Year).ToString().Substring(2, 2) : (DateTime.Now.Year + 1).ToString().Substring(2, 2)));
            ViewBag.Period = periodFY;


            var coll = (from usr in db.Users.DefaultIfEmpty()
                        from rol in usr.Users_Menus_Roles.DefaultIfEmpty()
                        where usr.NIK == currUserID && rol.menuID == 107
                        select new { usr, rol })
                                .AsEnumerable().Select(s => s.usr);
            if (coll.FirstOrDefault() == null)
            {
                return View("UnAuthorized");
            }

            ViewBag.DeptList = db.V_Users_Active.Where(w => w.DeptName.ToLower() == currDeptName.ToLower() && w.NIK.Substring(0, 3) != "NGK" && w.NIK.Substring(0, 3) != "EXP" && w.NIK.Substring(0, 3) != "BOD" && w.NIK != "SCReward").AsEnumerable().Select(s => new Users { DeptName = s.DeptName, SectionName = s.SectionName, PositionName = s.PositionName }).Distinct().ToList();

            var subOrdinate = db.V_Users_Position.Where(w => w.Position_Ranking < currUserData.Users_Position.Position_Ranking).Select(s => s.Position_Name).ToArray();
            if (currUserID == "822.01.19")
            {
                subOrdinate = db.V_Users_Position.Where(w => w.Position_Ranking <= currUserData.Users_Position.Position_Ranking).Select(s => s.Position_Name).ToArray();
            }

            var DataList = dbCM.HC_Competency_Result_Header
                .Where(w => (subOrdinate.Contains(w.PostName) || w.NIK == currUserID) && w.NIK.Substring(0, 1) != "P" && w.NIK.Substring(0, 3) != "NGK" && w.NIK.Substring(0, 3) != "EXP" && w.NIK.Substring(0, 3) != "BOD" && w.NIK.Substring(0, 1) != "K" && w.NIK.Substring(0, 1) != "M" && w.NIK != "SCReward");
            var MemberList = db.V_Users_Active.Where(w => (subOrdinate.Contains(w.PositionName) || w.NIK == currUserID) && w.NIK.Substring(0, 1) != "P" && w.NIK.Substring(0, 3) != "NGK" && w.NIK.Substring(0, 3) != "EXP" && w.NIK.Substring(0, 3) != "BOD" && w.NIK.Substring(0, 1) != "K" && w.NIK.Substring(0, 1) != "M" && w.NIK != "SCReward");
            string[] position0 = { "FOREMAN" };
            string[] position1 = { "SUPERVISOR" };
            string[] position2 = { "ASSISTANT MANAGER", "ASSISTANT MANAGER (ACTING)", "MANAGER", "MANAGER (ACTING)" };
            string[] position3 = { "SENIOR MANAGER", "SENIOR MANAGER (ACTING)", "DEPUTY GENERAL MANAGER", "DEPUTY GENERAL MANAGER ACTING", "GENERAL MANAGER" };
            string[] position4 = { "BOD" };
            if (position0.Contains(currUserData.Users_Position.Position_Name))
            {
                if (currUserID == "763.11.16" && periodFY != "FY123")
                {
                    DataList = DataList.Where(w => w.SectionName == currSectName);
                    MemberList = MemberList.Where(w => w.SectionName == currSectName);
                }
                else
                {
                    DataList = DataList.Where(w => w.NIK == currUserID);
                    MemberList = MemberList.Where(w => w.NIK == currUserID);
                }
            }
            else if (position1.Contains(currUserData.Users_Position.Position_Name))
            {
                DataList = DataList.Where(w => w.SectionName == currSectName);
                MemberList = MemberList.Where(w => w.SectionName == currSectName);
            }
            else if (position2.Contains(currUserData.Users_Position.Position_Name))
            {
                if (currUserID == "685.05.14")
                {

                    DataList = DataList.Where(w => w.DeptName == "SP PD - METAL SHELL & INSULATOR" || (w.DeptName == currDeptName && w.NIK == "685.05.14"));
                    MemberList = MemberList.Where(w => w.DeptName == "SP PD - METAL SHELL & INSULATOR" || (w.DeptName == currDeptName && w.NIK == "685.05.14"));
                }
                else if (currUserID == "692.08.14")
                {
                    DataList = DataList.Where(w => w.DeptName == "SP PD - SPARK PLUG" || (w.DeptName == currDeptName && w.NIK == "692.08.14"));
                    MemberList = MemberList.Where(w => w.DeptName == "SP PD - SPARK PLUG" || (w.DeptName == currDeptName && w.NIK == "692.08.14"));
                }
                else if (currUserID == "664.08.13")
                {
                    DataList = DataList.Where(w => w.DeptName == "PC PD - PLUG CAP" || (w.DeptName == currDeptName && w.NIK == "664.08.13"));
                    MemberList = MemberList.Where(w => w.DeptName == "PC PD - PLUG CAP" || (w.DeptName == currDeptName && w.NIK == "664.08.13"));
                }
                else if (currUserID == "793.03.18")
                {
                    DataList = DataList.Where(w => w.DeptName == "QUALITY" || w.DeptName == currDeptName);
                    MemberList = MemberList.Where(w => w.DeptName == "QUALITY" || w.DeptName == currDeptName);
                }
                else if (currUserID == "822.01.19")
                {
                    DataList = DataList.Where(w => w.SectionName != "SALES ADMIN" && w.DeptName == currDeptName);
                    MemberList = MemberList.Where(w => w.SectionName != "SALES ADMIN" && w.DeptName == currDeptName);
                }
                else if (currUserID == "589.10.09")
                {
                    DataList = DataList.Where(w => w.SectionName == "SALES ADMIN" || w.DeptName == currDeptName);
                    MemberList = MemberList.Where(w => w.SectionName == "SALES ADMIN" || w.DeptName == currDeptName);
                }
                else if (currUserID == "814.01.19")
                {
                    DataList = DataList.Where(w => w.DeptName == currDeptName || w.NIK == "822.01.19");
                    MemberList = MemberList.Where(w => w.DeptName == currDeptName || w.NIK == "822.01.19");
                }
                else
                {
                    DataList = DataList.Where(w => w.DeptName == currDeptName);
                    MemberList = MemberList.Where(w => w.DeptName == currDeptName);
                }
            }
            else if (position3.Contains(currUserData.Users_Position.Position_Name))
            {
                if (currUserID == "851.02.21")
                {
                    DataList = DataList.Where(w => (w.DivName == currDivName && w.DeptName != "SALES - OEM & OES") || (w.NIK == "663.01.14" || w.NIK == "665.11.13"));
                    MemberList = MemberList.Where(w => (w.DivisionName == currDivName && w.DeptName != "SALES - OEM & OES") || (w.NIK == "663.01.14" || w.NIK == "665.11.13"));
                }
                else if (currUserID == "546.08.05")
                {
                    DataList = DataList.Where(w => w.DivName == currDivName || w.DivName == "SUPPLY CHAIN MANAGEMENT");
                    MemberList = MemberList.Where(w => w.DivisionName == currDivName || w.DivisionName == "SUPPLY CHAIN MANAGEMENT");
                }
                else if (currUserID == "618.04.12")
                {
                    DataList = DataList.Where(w => w.DivName == "PROD. ENGINEERING & MAINTENANCE" || w.DivName == "PRODUCTION");
                    MemberList = MemberList.Where(w => w.DivisionName == "PROD. ENGINEERING & MAINTENANCE" || w.DivisionName == "PRODUCTION");
                }
                else if (currUserID == "EXP.014")
                {
                    DataList = DataList.Where(w => w.DivName == "PROD. ENGINEERING & MAINTENANCE" || w.DivName == "PRODUCTION" || w.DivName == "QUALITY MANAGEMENT");
                    MemberList = MemberList.Where(w => w.DivisionName == "PROD. ENGINEERING & MAINTENANCE" || w.DivisionName == "PRODUCTION" || w.DivisionName == "QUALITY MANAGEMENT");
                }
                else if (currUserID == "EXP.015")
                {
                    DataList = DataList.Where(w => w.DivName == "SALES & MARKETING");
                    MemberList = MemberList.Where(w => w.DivisionName == "SALES & MARKETING");
                }
                else if (currUserID == "EXP.011")
                {
                    DataList = DataList.Where(w => w.DivName == "ACCOUNTING & IT");
                    MemberList = MemberList.Where(w => w.DivisionName == "ACCOUNTING & IT");
                }
                else
                {
                    DataList = DataList.Where(w => w.DivName == currDivName);
                    MemberList = MemberList.Where(w => w.DivisionName == currDivName);
                }
            }
            else if (position4.Contains(currUserData.Users_Position.Position_Name))
            {
                if (currUserID == "EXP.014")
                {
                    DataList = DataList.Where(w => w.DivName == "PROD. ENGINEERING & MAINTENANCE" || w.DivName == "PRODUCTION" || w.DivName == "QUALITY MANAGEMENT");
                    MemberList = MemberList.Where(w => w.DivisionName == "PROD. ENGINEERING & MAINTENANCE" || w.DivisionName == "PRODUCTION" || w.DivisionName == "QUALITY MANAGEMENT");
                }
                else if (currUserID == "EXP.015")
                {
                    DataList = DataList.Where(w => w.DivName == "SALES & MARKETING");
                    MemberList = MemberList.Where(w => w.DivisionName == "SALES & MARKETING");
                }
                else if (currUserID == "EXP.011")
                {
                    DataList = DataList.Where(w => w.DivName == "ACCOUNTING & IT");
                    MemberList = MemberList.Where(w => w.DivisionName == "ACCOUNTING & IT");
                }
                else if (currUserID == "EXP.013")
                {
                    //AllDiv No need to place where clause
                }
                else
                {
                    DataList = DataList.Where(w => w.DivName == currDivName);
                    MemberList = MemberList.Where(w => w.DivisionName == currDivName);
                }
            }
            else
            {
                DataList = DataList.Where(w => w.NIK == currUserID);
                MemberList = MemberList.Where(w => w.NIK == currUserID);
            }
            var DataNIK = DataList.Select(s => s.NIK).ToArray();
            MemberList = MemberList.Where(w => !DataNIK.Contains(w.NIK));
            ViewBag.DataList = DataList.OrderByDescending(o => o.DivName).ThenByDescending(o => o.DeptName).OrderByDescending(o => o.SectionName).ToList();
            ViewBag.MemberList = MemberList.OrderByDescending(o => o.DivisionName).ThenByDescending(o => o.DeptName).OrderByDescending(o => o.SectionName).ToList();



            ViewBag.NavHide = true;

            return View();
        }
        public class competencyMapData
        {
            public List<object[]> MapData { get; set; }
            public String MapMergeCells { get; set; }
        }

        public JsonResult getMapData()
        {
            var _Division = Request["iDivision"];
            var _Department = Request["iDepartment"];
            var _Section = Request["iSection"];
            var _CostName = Request["iCostName"];
            var _Title = Request["iTitleName"];
            var _Position = Request["iPosition"];
            competencyMapData data = new competencyMapData();
            var _mapData = new List<object[]>();
            _mapData.Add(new object[] { "A. Pengetahuan Teknis / Technical Knowledge", null, null, null, null, null, null, null, null, null, null });
            _mapData.Add(new object[] { 1, null, null, null, null, null, null, null, null, null, null });
            _mapData.Add(new object[] { "B. Kemampuan Praktik / Practical Skill", null, null, null, null, null, null, null, null, null, null });
            _mapData.Add(new object[] { 1, null, null, null, null, null, null, null, null, null, null });
            _mapData.Add(new object[] { "C. Perilaku / Behaviour", null, null, null, null, null, null, null, null, null, null });
            _mapData.Add(new object[] { 1, null, null, null, null, null, null, null, null, null, null });
            data.MapData = _mapData;
            data.MapMergeCells = "[{\"row\":0,\"col\":0,\"rowspan\":1,\"colspan\":11},{\"row\":2,\"col\":0,\"rowspan\":1,\"colspan\":11},{\"row\":4,\"col\":0,\"rowspan\":1,\"colspan\":11}]";

            var checkData = dbCM.HC_Competency_Map_Header.Where(w => w.Division == _Division && w.Department == _Department && w.Section == _Section && w.CostName == _CostName && w.TitleName == _Title && w.Job_Position == _Position).FirstOrDefault();
            if (checkData != null)
            {
                var _data = dbCM.HC_Competency_Map_Line.Where(w => w.Header_ID == checkData.ID).OrderBy(o => o.Idx).Select(s => new { s.No, s.Requirement, s.Score, s.Module, s.Internal, s.Internal_Duration, s.External, s.External_Duration, s.Trainer, s.Evaluation_Method, s.Remark }).ToList();
                if (_data.Count() > 0)
                {
                    var newdata = new List<object[]>();
                    for (var i = 0; i < _data.Count(); i++)
                    {
                        newdata.Add(new object[] { _data[i].No, _data[i].Requirement, _data[i].Score, _data[i].Module, _data[i].Internal, _data[i].Internal_Duration, _data[i].External, _data[i].External_Duration, _data[i].Trainer, _data[i].Evaluation_Method, _data[i].Remark });
                    }
                    data.MapData = newdata;
                    data.MapMergeCells = checkData.MergeCells;

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult setMapData(string iDivision, string iDepartment, string iSection, string iCostName, string iTitleName, string iPosition, string[][] iData, string iMergeCells)
        {
            var _Division = iDivision;
            var _Department = iDepartment;
            var _Section = iSection;
            var _CostName = iCostName;
            var _Title = iTitleName;
            var _Position = iPosition;
            var _data = iData;
            var _mergecells = iMergeCells;
            var headerID = 0;

            var checkHeaderData = dbCM.HC_Competency_Map_Header.Where(w => w.Division == _Division && w.Department == _Department && w.Section == _Section && w.CostName == _CostName && w.Job_Position == _Position && w.TitleName == _Title).FirstOrDefault();
            if (checkHeaderData == null)
            {
                var newHeaderData = new HC_Competency_Map_Header();
                newHeaderData.GUID = Guid.NewGuid().ToString();
                newHeaderData.Division = _Division;
                newHeaderData.Department = _Department;
                newHeaderData.Section = _Section;
                newHeaderData.CostName = _CostName;
                newHeaderData.TitleName = _Title;
                newHeaderData.Job_Position = _Position;
                newHeaderData.MergeCells = _mergecells;
                dbCM.HC_Competency_Map_Header.Add(newHeaderData);
                dbCM.SaveChanges();
                headerID = newHeaderData.ID;
            }
            else
            {
                checkHeaderData.MergeCells = _mergecells;
                headerID = checkHeaderData.ID;
            }
            List<int> _hashList = new List<int>();
            var __idx = 1;
            foreach (var data in _data)
            {
                var _hash = (__idx + data[0] + data[1] + data[2] + data[3] + data[4] + data[5] + data[6] + data[7] + data[8] + data[9] + data[10]).GetHashCode();

                __idx++;
                _hashList.Add(_hash);
            }

            var checkLineList = dbCM.HC_Competency_Map_Line.Where(w => w.Header_ID == headerID && !_hashList.Contains(w.HashCode)).ToList();
            dbCM.HC_Competency_Map_Line.RemoveRange(checkLineList);

            var _idx = 1;
            foreach (var data in _data)
            {
                var _hash = (_idx + data[0] + data[1] + data[2] + data[3] + data[4] + data[5] + data[6] + data[7] + data[8] + data[9] + data[10]).GetHashCode();
                var checkLineData = dbCM.HC_Competency_Map_Line.Where(w => w.Header_ID == headerID && w.HashCode == _hash).FirstOrDefault();
                if (checkLineData != null)
                {
                    checkLineData.No = data[0] == "" ? null : data[0];
                    checkLineData.Requirement = data[1] == "" ? null : data[1];
                    checkLineData.Score = data[2] == "" ? (int?)null : int.Parse(data[2]);
                    checkLineData.Module = data[3] == "" ? null : data[3];
                    checkLineData.Internal = data[4] == "" ? null : data[4];
                    checkLineData.Internal_Duration = data[5] == "" ? (int?)null : int.Parse(data[5]);
                    checkLineData.External = data[6] == "" ? null : data[6];
                    checkLineData.External_Duration = data[7] == "" ? (int?)null : int.Parse(data[7]);
                    checkLineData.Trainer = data[8] == "" ? null : data[8];
                    checkLineData.Evaluation_Method = data[9] == "" ? null : data[9];
                    checkLineData.Remark = data[10] == "" ? null : data[10];
                    checkLineData.Idx = _idx;
                    checkLineData.HashCode = _hash;
                }
                else
                {
                    var newLineData = new HC_Competency_Map_Line();
                    newLineData.Header_ID = headerID;
                    newLineData.No = data[0] == "" ? null : data[0];
                    newLineData.Requirement = data[1] == "" ? null : data[1];
                    newLineData.Score = data[2] == "" ? (int?)null : int.Parse(data[2]);
                    newLineData.Module = data[3] == "" ? null : data[3];
                    newLineData.Internal = data[4] == "" ? null : data[4];
                    newLineData.Internal_Duration = data[5] == "" ? (int?)null : int.Parse(data[5]);
                    newLineData.External = data[6] == "" ? null : data[6];
                    newLineData.External_Duration = data[7] == "" ? (int?)null : int.Parse(data[7]);
                    newLineData.Trainer = data[8] == "" ? null : data[8];
                    newLineData.Evaluation_Method = data[9] == "" ? null : data[9];
                    newLineData.Remark = data[10] == "" ? null : data[10];
                    newLineData.Idx = _idx;
                    newLineData.HashCode = _hash;
                    dbCM.HC_Competency_Map_Line.Add(newLineData);
                }
                _idx++;

            }
            dbCM.SaveChanges();
            return Json(_data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getResultData()
        {
            var _NIK = Request["iNIK"];
            var _Name = Request["iName"];
            var _Division = Request["iDivision"];
            var _Department = Request["iDepartment"];
            var _Section = Request["iSection"];
            var _CostName = Request["iCostName"];
            var _Position = Request["iPosition"];
            var _TitleName = Request["iTitleName"];
            var _Competency = Request["iCompetency"];

            List<object[]> _ResultData = new List<object[]>();
            switch (_Competency)
            {
                case "B":
                    _ResultData.Add(new object[] { "B. Kemampuan Praktik / Practical Skill", null, null, null, null, null, null, null, null, null, null });
                    _ResultData.Add(new object[] { 1, null, null, null, null, null, null, null, null, null, null });
                    break;
                case "C":
                    _ResultData.Add(new object[] { "C. Perilaku / Behaviour", null, null, null, null, null, null, null, null, null, null });
                    _ResultData.Add(new object[] { 1, null, null, null, null, null, null, null, null, null, null });
                    break;
                default:
                    _ResultData.Add(new object[] { "A. Pengetahuan Teknis / Technical Knowledge", null, null, null, null, null, null, null, null, null, null });
                    _ResultData.Add(new object[] { 1, null, null, null, null, null, null, null, null, null, null });
                    break;
            }


            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());
            con.Open();

            SqlCommand cmd = new SqlCommand("sp_CompetencyResult", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@divName", _Division);
            cmd.Parameters.AddWithValue("@deptName", _Department);
            cmd.Parameters.AddWithValue("@sectName", _Section);
            cmd.Parameters.AddWithValue("@costName", _CostName);
            cmd.Parameters.AddWithValue("@positionName", _Position.Replace(" (ACTING)", ""));
            cmd.Parameters.AddWithValue("@titleName", _TitleName);
            cmd.Parameters.AddWithValue("@tableType", _Competency);
            cmd.CommandTimeout = 30000;
            DataTable dt = new DataTable();
            List<string> resultData = new List<string>();
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }
            if(dt.Rows.Count > 0)
            {
                _ResultData.Clear();
                var idx = 1;
                foreach (DataRow row in dt.Rows)
                {
                    _ResultData.Add(new object[] { row["No"].ToString(), row["Requirement"].ToString(), row["Std_Score"].ToString(), checkResultScore(row["Score5"].ToString()), checkResultScore(row["Score4"].ToString()), checkResultScore(row["Score3"].ToString()), checkResultScore(row["Score2"].ToString()), checkResultScore(row["Score1"].ToString()), row["Score"]?.ToString(), "=IF(OR(I" + @idx + " < C" + @idx+ ",I" + @idx + "=\"\"),\"UNFIT\",\"FIT\")", row["Note"].ToString() });
                    idx++;
                }
            }


            //var result = JsonConvert.SerializeObject(dt,new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Ignore});

            return Json(_ResultData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult setResultData(string iDivision, string iDepartment, string iSection, string iCostName, string iTitleName, string iPosition, string[][][] iData)
        {
            var _Division = iDivision;
            var _Department = iDepartment;
            var _Section = iSection;
            var _CostName = iCostName;
            var _Title = iTitleName;
            var _Position = iPosition;
            var _data = iData;
            //var headerID = 0;

            //var checkHeaderData = dbCM.HC_Competency_Map_Header.Where(w => w.Division == _Division && w.Department == _Department && w.Section == _Section && w.CostName == _CostName && w.Job_Position == _Position && w.TitleName == _Title).FirstOrDefault();
            //if (checkHeaderData == null)
            //{
            //    var newHeaderData = new HC_Competency_Map_Header();
            //    newHeaderData.GUID = Guid.NewGuid().ToString();
            //    newHeaderData.Division = _Division;
            //    newHeaderData.Department = _Department;
            //    newHeaderData.Section = _Section;
            //    newHeaderData.CostName = _CostName;
            //    newHeaderData.TitleName = _Title;
            //    newHeaderData.Job_Position = _Position;
            //    dbCM.HC_Competency_Map_Header.Add(newHeaderData);
            //    dbCM.SaveChanges();
            //    headerID = newHeaderData.ID;
            //}
            //else
            //{
            //    headerID = checkHeaderData.ID;
            //}
            //List<int> _hashList = new List<int>();
            //var __idx = 1;
            //foreach (var data in _data)
            //{
            //    var _hash = (__idx + data[0] + data[1] + data[2] + data[3] + data[4] + data[5] + data[6] + data[7] + data[8] + data[9] + data[10]).GetHashCode();

            //    __idx++;
            //    _hashList.Add(_hash);
            //}

            //var checkLineList = dbCM.HC_Competency_Map_Line.Where(w => w.Header_ID == headerID && !_hashList.Contains(w.HashCode)).ToList();
            //dbCM.HC_Competency_Map_Line.RemoveRange(checkLineList);

            //var _idx = 1;
            //foreach (var data in _data)
            //{
            //    var _hash = (_idx + data[0] + data[1] + data[2] + data[3] + data[4] + data[5] + data[6] + data[7] + data[8] + data[9] + data[10]).GetHashCode();
            //    var checkLineData = dbCM.HC_Competency_Map_Line.Where(w => w.Header_ID == headerID && w.HashCode == _hash).FirstOrDefault();
            //    if (checkLineData != null)
            //    {
            //        checkLineData.No = data[0] == "" ? null : data[0];
            //        checkLineData.Requirement = data[1] == "" ? null : data[1];
            //        checkLineData.Score = data[2] == "" ? (int?)null : int.Parse(data[2]);
            //        checkLineData.Module = data[3] == "" ? null : data[3];
            //        checkLineData.Internal = data[4] == "" ? null : data[4];
            //        checkLineData.Internal_Duration = data[5] == "" ? (int?)null : int.Parse(data[5]);
            //        checkLineData.External = data[6] == "" ? null : data[6];
            //        checkLineData.External_Duration = data[7] == "" ? (int?)null : int.Parse(data[7]);
            //        checkLineData.Trainer = data[8] == "" ? null : data[8];
            //        checkLineData.Evaluation_Method = data[9] == "" ? null : data[9];
            //        checkLineData.Remark = data[10] == "" ? null : data[10];
            //        checkLineData.Idx = _idx;
            //        checkLineData.HashCode = _hash;
            //    }
            //    else
            //    {
            //        var newLineData = new HC_Competency_Map_Line();
            //        newLineData.Header_ID = headerID;
            //        newLineData.No = data[0] == "" ? null : data[0];
            //        newLineData.Requirement = data[1] == "" ? null : data[1];
            //        newLineData.Score = data[2] == "" ? (int?)null : int.Parse(data[2]);
            //        newLineData.Module = data[3] == "" ? null : data[3];
            //        newLineData.Internal = data[4] == "" ? null : data[4];
            //        newLineData.Internal_Duration = data[5] == "" ? (int?)null : int.Parse(data[5]);
            //        newLineData.External = data[6] == "" ? null : data[6];
            //        newLineData.External_Duration = data[7] == "" ? (int?)null : int.Parse(data[7]);
            //        newLineData.Trainer = data[8] == "" ? null : data[8];
            //        newLineData.Evaluation_Method = data[9] == "" ? null : data[9];
            //        newLineData.Remark = data[10] == "" ? null : data[10];
            //        newLineData.Idx = _idx;
            //        newLineData.HashCode = _hash;
            //        dbCM.HC_Competency_Map_Line.Add(newLineData);
            //    }
            //    _idx++;

            //}
            //dbCM.SaveChanges();
            return Json(_data, JsonRequestBehavior.AllowGet);
        }

        public bool checkResultScore(string Score)
        {
            if(int.Parse(Score) == 1)
            {
                return true;
            }

            return false;
        }
    }
}