using NGKBusi.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace NGKBusi.Areas.QCC.Controllers
{
    public class DataController : Controller
    {
        DefaultConnection db = new DefaultConnection();
        // GET: QCC/Data
        public ActionResult List()
        {
            var currPeriod = !String.IsNullOrEmpty(Request["iPeriod"]) ? Int32.Parse(Request["iPeriod"]) : 0;
            var currGroup = !String.IsNullOrEmpty(Request["iGroup"]) ? Request["iGroup"] : "";
            ViewBag.Users = db.V_Users_Active.ToList();
            var QCCData = db.QCC_List.ToList();
            if (!String.IsNullOrEmpty(Request["iPeriod"]) && !String.IsNullOrEmpty(Request["iGroup"]))
            {
                QCCData = QCCData.Where(w => w.Period == currPeriod && w.Group == currGroup).ToList();
            }

            ViewBag.DataList = QCCData;
            return View();
        }
        // GET: QCC/Data
        public ActionResult Progress()
        {
            var currPeriod = !String.IsNullOrEmpty(Request["iPeriod"]) ? Int32.Parse(Request["iPeriod"]) : 0;
            var currGroup = !String.IsNullOrEmpty(Request["iGroup"]) ? Request["iGroup"] : "";
            var QCCData = db.QCC_List.Select(s => new QCCDataList
            {
                ID = s.ID,
                Period = s.Period,
                Group = s.Group,
                Type = s.Type,
                Theme = s.Theme,
                Step1 = db.QCC_Progress.Where(w => w.List_ID == s.ID && w.Step == 1).FirstOrDefault(),
                Step2 = db.QCC_Progress.Where(w => w.List_ID == s.ID && w.Step == 2).FirstOrDefault(),
                Step3 = db.QCC_Progress.Where(w => w.List_ID == s.ID && w.Step == 3).FirstOrDefault(),
                Step4 = db.QCC_Progress.Where(w => w.List_ID == s.ID && w.Step == 4).FirstOrDefault(),
                Step5 = db.QCC_Progress.Where(w => w.List_ID == s.ID && w.Step == 5).FirstOrDefault(),
                Step6 = db.QCC_Progress.Where(w => w.List_ID == s.ID && w.Step == 6).FirstOrDefault(),
                Step7 = db.QCC_Progress.Where(w => w.List_ID == s.ID && w.Step == 7).FirstOrDefault(),
                Step8 = db.QCC_Progress.Where(w => w.List_ID == s.ID && w.Step == 8).FirstOrDefault()
            }).ToList();
            if(!String.IsNullOrEmpty(Request["iPeriod"]) && !String.IsNullOrEmpty(Request["iGroup"]))
            {
                QCCData = QCCData.Where(w => w.Period == currPeriod && w.Group == currGroup).ToList();
            }
            ViewBag.QCCList = QCCData;
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult insertList()
        {
            var arrayAnggota = Request["iAnggota[]"].Split(',');
            var currUser = (ClaimsIdentity)User.Identity;
            var newData = new QCC_List();
            newData.Period = Int32.Parse(Request["iPeriod"]);
            newData.Group = Request["iGroupName"];
            newData.Type = Request["iType"];
            newData.Theme = Request["iTema"];
            newData.Fasilitator = Request["iFasilitator"];
            newData.Leader = Request["iLeader"];
            db.QCC_List.Add(newData);
            db.SaveChanges();
            var path = Path.Combine(Server.MapPath("~/Files/QCC/Progress/"), newData.ID.ToString());
            Directory.CreateDirectory(path);
            for (var i = 1; i <= 8; i++)
            {
                Directory.CreateDirectory(Path.Combine(path, i.ToString()));
            }

            //var deleteDataMember = db.QCC_List_Member.Where(w => w.List_ID == newData.ID).ToList();
            //db.QCC_List_Member.RemoveRange(deleteDataMember);
            foreach (var anggota in arrayAnggota)
            {
                var section = db.Users.Where(w => w.NIK == anggota).FirstOrDefault();
                db.QCC_List_Member.Add(new QCC_List_Member
                {
                    List_ID = newData.ID,
                    Member = anggota,
                    Section = (section.SubSectionName != "" ? section.SubSectionName : (section.SectionName != "" ? section.SectionName : (section.DeptName != "" ? section.DeptName : section.DivisionName)))
                });
            }
            db.SaveChanges();
            return RedirectToAction("List", "Data", new { area = "QCC" });
        }
        [HttpPost]
        [Authorize]
        public ActionResult editList()
        {
            var arrayAnggota = Request["iAnggota[]"].Split(',');
            var currID = Int32.Parse(Request["iID"]);
            var currUser = (ClaimsIdentity)User.Identity;
            var editData = db.QCC_List.Where(w => w.ID == currID).FirstOrDefault();
            editData.Period = Int32.Parse(Request["iPeriod"]);
            editData.Group = Request["iGroupName"];
            editData.Type = Request["iType"];
            editData.Theme = Request["iTema"];
            editData.Fasilitator = Request["iFasilitator"];
            editData.Leader = Request["iLeader"];
            foreach (var anggota in arrayAnggota)
            {
                var section = db.Users.Where(w => w.NIK == anggota).FirstOrDefault();
                var checkMember = db.QCC_List_Member.Where(w => w.List_ID == currID && w.Member == anggota).FirstOrDefault();
                if (checkMember == null)
                {
                    db.QCC_List_Member.Add(new QCC_List_Member
                    {
                        List_ID = editData.ID,
                        Member = anggota,
                        Section = (section.SubSectionName != "" ? section.SubSectionName : (section.SectionName != "" ? section.SectionName : (section.DeptName != "" ? section.DeptName : section.DivisionName)))
                    });
                }
            }
            var deleteNotExistMember = db.QCC_List_Member.Where(w => w.List_ID == currID && !arrayAnggota.Contains(w.Member)).ToList();
            db.QCC_List_Member.RemoveRange(deleteNotExistMember);
            db.SaveChanges();
            return RedirectToAction("List", "Data", new { area = "QCC" });
        }
        [HttpPost]
        [Authorize]
        public ActionResult insertStep()
        {
            var step = int.Parse(Request["iStep"]);
            var listID = int.Parse(Request["iListID"]);
            var note = Request["iNote"];
            var checkProgress = db.QCC_Progress.Where(w => w.List_ID == listID && w.Step == step).FirstOrDefault();

            if (checkProgress != null)
            {
                checkProgress.Note = note;
            }
            else
            {
                db.QCC_Progress.Add(new QCC_Progress
                {
                    List_ID = listID,
                    Step = step,
                    Note = note
                });
            }
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase iFile = Request.Files[i];
                // extract only the filename
                if (iFile.ContentLength > 0)
                {
                    var fileName = iFile.FileName;
                    string extension = Path.GetExtension(fileName);
                    // store the file inside ~/App_Data/uploads folder
                    var path = Path.Combine(Server.MapPath("~/Files/QCC/Progress/" + listID + "/" + step), fileName);
                    iFile.SaveAs(path);
                    var checkFile = db.QCC_Progress_Files.Where(w => w.List_ID == listID && w.Step == step && w.Filename == fileName).FirstOrDefault();
                    if (checkFile == null)
                    {
                        db.QCC_Progress_Files.Add(new QCC_Progress_Files()
                        {
                            List_ID = listID,
                            Step = step,
                            Filename = fileName,
                            Ext = extension

                        });
                    }
                }
            }

            db.SaveChanges();

            return RedirectToAction("Progress", "Data", new { area = "QCC" });
        }
        [HttpPost]
        public ActionResult getStep()
        {
            var step = int.Parse(Request["iStep"]);
            var listID = int.Parse(Request["iListID"]);
            var checkProgress = db.QCC_Progress.Where(w => w.List_ID == listID && w.Step == step).FirstOrDefault();
            var notes = "";
            var stats = false;
            var getFiles = db.QCC_Progress_Files.Where(w => w.List_ID == listID && w.Step == step).Select(s => new { filename = s.Filename, ext = s.Ext, id = s.ID });
            if (checkProgress != null)
            {
                notes = checkProgress.Note;
                stats = true;
            }
            return Json(new { stat = stats, note = notes, files = getFiles }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        public ActionResult deleteList()
        {
            var currUser = (ClaimsIdentity)User.Identity;
            var currID = Int32.Parse(Request["iID"]);
            var currDataList = db.QCC_List.Where(x => x.ID == currID).FirstOrDefault();
            var currDataMember = db.QCC_List_Member.Where(x => x.List_ID == currID).ToList();
            var currDataProgress = db.QCC_Progress.Where(x => x.List_ID == currID).ToList();
            var currDataProgressFiles = db.QCC_Progress_Files.Where(x => x.List_ID == currID).ToList();
            db.QCC_List_Member.RemoveRange(currDataMember);
            db.QCC_List.Remove(currDataList);
            db.QCC_Progress.RemoveRange(currDataProgress);
            db.QCC_Progress_Files.RemoveRange(currDataProgressFiles);

            var path = Server.MapPath("~/Files/QCC/Progress/" + currID);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            db.SaveChanges();

            return Content(Boolean.TrueString);
        }

        [HttpPost]
        public ActionResult deleteFile()
        {
            var currID = Int32.Parse(Request["iID"]);
            var del = db.QCC_Progress_Files.Where(w => w.ID == currID).FirstOrDefault();

            var path = Server.MapPath("~/Files/QCC/Progress/" + del.List_ID + "/" + del.Step + "/" + del.Filename);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            db.QCC_Progress_Files.Remove(del);
            db.SaveChanges();

            return Content(Boolean.TrueString);
        }
        [HttpPost]
        public ActionResult cancelStep()
        {
            var currID = Int32.Parse(Request["iID"]);
            var currStep = Int32.Parse(Request["iStep"]);
            var currDataProgress = db.QCC_Progress.Where(x => x.List_ID == currID && x.Step == currStep).ToList();
            var currDataProgressFiles = db.QCC_Progress_Files.Where(x => x.List_ID == currID && x.Step == currStep).ToList();
            db.QCC_Progress.RemoveRange(currDataProgress);
            db.QCC_Progress_Files.RemoveRange(currDataProgressFiles);

            var path = Server.MapPath("~/Files/QCC/Progress/" + currID + "/" + currStep);
            System.IO.DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            db.SaveChanges();

            return Content(Boolean.TrueString);
        }

    }

    public class QCCDataList
    {
        public int ID { get; set; }
        public int Period { get; set; }
        public string Group { get; set; }
        public string Type { get; set; }
        public string Theme { get; set; }
        public QCC_Progress Step1 { get; set; }
        public QCC_Progress Step2 { get; set; }
        public QCC_Progress Step3 { get; set; }
        public QCC_Progress Step4 { get; set; }
        public QCC_Progress Step5 { get; set; }
        public QCC_Progress Step6 { get; set; }
        public QCC_Progress Step7 { get; set; }
        public QCC_Progress Step8 { get; set; }
    }
}