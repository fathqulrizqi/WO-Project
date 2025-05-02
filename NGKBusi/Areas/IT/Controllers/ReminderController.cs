using Microsoft.AspNet.Identity;
using NGKBusi.Areas.IT.Models;
using NGKBusi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace NGKBusi.Areas.IT.Controllers
{
    public class ReminderController : Controller
    {
        DefaultConnection db = new DefaultConnection();
        ReminderConnection dbr = new ReminderConnection();
        // GET: IT/Reminder
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Home()
        {
            return View();
        }
        
        public JsonResult GetReminderDashData()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            List<Tbl_Event> events = new List<Tbl_Event>();
           
            //var spl = dbr.IT_Reminder.Where(w => w.IsActive == 1 && w.CreateBy == currUser).Select(s=>s.ID).ToList();
            //foreach (var item in spl)
            //{
            //    events.Add(new Tbl_Event
            //    {
            //        id = Convert.ToInt32(item.ID),
            //        title = item.ReminderTitle,
            //        start = item.DueDate.ToString("yyyy-MM-dd") + "T" + item.NotifTime + ":00:00",
            //        end = item.DueDate.ToString("yyyy-MM-dd") + "T" + item.NotifTime + ":59:00"
            //    });
            //}
            var spl = (from r in dbr.IT_Reminder
                       where r.IsActive == 1 && r.CreateBy == currUser
                       select new
                        {
                            ID = r.ID,
                            //ReminderTitle = r.ReminderTitle,
                            //DueDate = r.DueDate,
                            //NotifTime = r.NotifTime
                        }).ToList();
            //var spl2 = ( from ru in dbr.IT_Reminder_User
            //           join r in dbr.IT_Reminder on ru.ReminderID equals r.ID
            //           where ru.SendToUserEmail == CurrUser.Email
            //           select new
            //           {
            //               ID = r.ID,
            //               //ReminderTitle = r.ReminderTitle,
            //               //DueDate = r.DueDate,
            //               //NotifTime = r.NotifTime
            //           }).ToList();

            //var newList = spl.Union(spl2);

            foreach (var item in spl)
            {
                var dtlReminder = dbr.IT_Reminder.Where(w => w.ID == item.ID).FirstOrDefault();
                events.Add(new Tbl_Event
                {
                    id = Convert.ToInt32(dtlReminder.ID),
                    title = dtlReminder.ReminderTitle,
                    start = dtlReminder.DueDate.ToString("yyyy-MM-dd") + "T" + dtlReminder.NotifTime + ":00:00",
                    end = dtlReminder.DueDate.ToString("yyyy-MM-dd") + "T" + dtlReminder.NotifTime + ":59:00"
                });
                
            }

            return Json(events, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetReminderListUpcoming()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            
            List<Tbl_Event> upcomingEvents = new List<Tbl_Event>();
            
            var spl = (from r in dbr.IT_Reminder
                       where r.IsActive == 1 && r.CreateBy == currUser
                       select new
                       {
                           ID = r.ID,
                           
                       }).ToList();
            var spl2 = (from ru in dbr.IT_Reminder_User
                        join r in dbr.IT_Reminder on ru.ReminderID equals r.ID
                        where ru.SendToUserEmail == CurrUser.Email
                        select new
                        {
                            ID = r.ID,
                            
                        }).ToList();

            var newList = spl.Union(spl2);

            foreach (var item in newList)
            {
                var dtlReminder = dbr.IT_Reminder.Where(w => w.ID == item.ID).FirstOrDefault();
                if (dtlReminder.DueDate.Date >= DateTime.Now.Date)
                {
                    upcomingEvents.Add(new Tbl_Event
                    {
                        id = Convert.ToInt32(dtlReminder.ID),
                        title = dtlReminder.ReminderTitle,
                        start = dtlReminder.DueDate.ToString("yyyy-MM-dd") + "T" + dtlReminder.NotifTime + ":00:00",
                        end = dtlReminder.DueDate.ToString("yyyy-MM-dd") + "T" + dtlReminder.NotifTime + ":59:00",
                        module = dtlReminder.Module
                    });
                }

            }

            return Json(upcomingEvents, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetReminderDashInfo(int id)
        {
            var spl = dbr.IT_Reminder.Where(w => w.ID == id).FirstOrDefault();
            if (spl != null)
            {
                return Json(new
                {
                    title = spl.ReminderTitle,
                    start = spl.DueDate.ToString("yyyy-MM-dd") + "T" + spl.NotifTime + ":00:00",
                    end = spl.DueDate.ToString("yyyy-MM-dd") + "T" + spl.NotifTime + ":00:00" // Jika end date tidak null
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);

        }

        [Authorize]
        
        public ActionResult AddReminder()
        {
            var ListEmail = db.V_Users_Active.Where(w => w.Email != null).GroupBy(g => g.Email).Select(s => new ListEmail { Email = s.Key }).ToList();
            var ListVendor = db.V_AXVendorList.ToList();
            ViewBag.ListEmail = ListEmail;
            ViewBag.ListVendor = ListVendor;
            return View();
        }
        [HttpPost]
        public ActionResult AddReminder(IT_Reminder smodel, string[] selReminderUser, string txtDueDate)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            int status = 0;
            string msg = "";
            //DateTime DueDate = DateTime.Now;
            var DueDate = DateTime.ParseExact(txtDueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            // action insert to IT_reminder // 
            IT_Reminder reminder = new IT_Reminder();
            reminder.ReminderTitle = smodel.ReminderTitle;
            reminder.Module = smodel.Module;
            reminder.Type = smodel.Type;
            reminder.Thirdparty = smodel.Thirdparty;
            reminder.DueDate = DueDate;
            reminder.Description = smodel.Description;
            reminder.NotifStart = smodel.NotifStart * -1;
            reminder.NotifTime = smodel.NotifTime;
            reminder.IntervalRepetReminderType = smodel.IntervalRepetReminderType;
            reminder.IntervalRepeatReminderNumber = smodel.IntervalRepeatReminderNumber;
            reminder.IntervalRepeatNotifType = smodel.IntervalRepeatNotifType;
            reminder.IntervalRepeatNotifNumber = smodel.IntervalRepeatNotifNumber;
            reminder.CreateTime = DateTime.Now;
            reminder.CreateBy = CurrUser.NIK;
            // get file attachment
            string filePath = "";
            string fileName = "";
            HttpPostedFileBase uploadFile = Request.Files["FileAttachment"];
            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                fileName = uploadFile.FileName;
                string extension = Path.GetExtension(fileName);
                filePath = Server.MapPath("~/Files/IT/Reminder/");
                filePath = filePath + fileName;

                string deletedFile = filePath + reminder.Attachment;
                //remove old attachment
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                reminder.Attachment = fileName;

                
            }
            else
            {
                fileName = "not found";
                filePath = "not found";
            }
            dbr.IT_Reminder.Add(reminder);
            int ins = dbr.SaveChanges();

            if (ins > 0)
            {
                // upload file attachment
                uploadFile.SaveAs(filePath);
                // action insert to IT_reminder_user
                foreach (var user in selReminderUser)
                {
                    //var userInfo = db.V_Users_Active.Where(w => w.NIK == user).FirstOrDefault();
                    IT_Reminder_User userList = new IT_Reminder_User();
                    userList.ReminderID = reminder.ID;
                    userList.SendToUser = user;
                    userList.SendToUserEmail = user;
                    userList.IsActive = 1;

                    dbr.IT_Reminder_User.Add(userList);
                }
                int ins_user = dbr.SaveChanges();
                if (ins_user > 0)
                {
                    // action insert to IT_reminder_task
                    IT_Reminder_Task newTask = new IT_Reminder_Task();
                    newTask.ReminderID = reminder.ID;
                    newTask.ReminderDate = reminder.DueDate.AddDays(reminder.NotifStart).Date;
                    newTask.ReminderTime = Convert.ToInt32(smodel.NotifTime);
                    newTask.ReminderDueDate = reminder.DueDate;

                    dbr.IT_Reminder_Task.Add(newTask);
                    int ins_task = dbr.SaveChanges();
                    if (ins_task > 0)
                    {
                        msg = "Success Insert Reminder";
                        status = 1;
                    }
                    else
                    {
                        msg = "Failed Save Task";
                        status = 0;
                    }
                }
                else
                {
                    msg = "Failed Insert User";
                    status = 0;
                }

            }
            else
            {
                msg = "Failed Insert Reminder";
                status = 0;
            }

            return Json(new { status = status, msg = msg, reminder = reminder, DueDate = DueDate });
        }
        [HttpPost]
        public JsonResult GetReminderLIst()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            var CountRow = dbr.IT_Reminder.Where(w => w.CreateBy == CurrUser.NIK).ToList().Count();
            var rawData = dbr.IT_Reminder.Where(w => w.CreateBy == CurrUser.NIK).ToList();

            List<Tbl_IT_Reminder> data = new List<Tbl_IT_Reminder>();

            int No = 0;
            foreach (var raw in rawData)
            {
                No++;

                var UrlAction = Url.Action("FormSettingReminder", "Reminder", new { area = "IT", ID = raw.ID });

                var ActionButton = "";

                ActionButton = "<a href=\"" + UrlAction + "\" title=\"Setting\" class=\"btn btn-primary\"><i class=\"fa fa-cog\"></i></a>";

                // ambil due date terakhir
                var getReminderTask = dbr.IT_Reminder_Task.Where(w => w.ID == raw.ID).OrderByDescending(x => x.ReminderDueDate);
                int countReminderTask = getReminderTask.Count();
                var ReminderTask = getReminderTask.FirstOrDefault();

                // get nexttask
                data.Add(
                    new Tbl_IT_Reminder
                    {
                        No = No,
                        ReminderTitle = raw.ReminderTitle,
                        Module = raw.Module,
                        Type = raw.Type,
                        Thirdparty = raw.Thirdparty,
                        Description = raw.Description,
                        DueDate = countReminderTask != 0 ? ReminderTask.ReminderDueDate.ToString("dd MMM yyyy") : "Not Found",
                        NextNotif = countReminderTask != 0 ? ReminderTask.ReminderDate.ToString("dd MMM yyyy") + " " + ReminderTask.ReminderTime + ":00": "Not Found",
                        ActionButton = ActionButton,

                    });
            }

            var jsonResult = Json(new { rows = data, totalNotFiltered = CountRow, total = CountRow }, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
        [HttpGet]
        public JsonResult GetReminderTask()
        {
            DateTime today = DateTime.Now.Date;
            DateTime currentTime = DateTime.Now;

            DateTime newDate = DateTime.Now;
            DateTime newDueDate;
            int newHours;

            List<Tbl_IT_Reminder_Task> actions = new List<Tbl_IT_Reminder_Task>();
            List<string> emailSendTo = new List<string>();
            var ListEmail = new ArrayList();
            var sql = dbr.IT_Reminder_Task.Where(w => w.ReminderDate == today && w.ReminderTime == currentTime.Hour && w.IsSend == 0).ToList();
            foreach(var data in sql)
            {
                // get reminder information
                var reminder = dbr.IT_Reminder.Where(w => w.ID == data.ReminderID).FirstOrDefault();

                bool repeatReminder = reminder.IntervalRepetReminderType == "OneTime" ? false : true;
                bool repeatTask = reminder.IntervalRepeatNotifType == "OneTime" ? false : true;
                int intervalNotifTask = reminder.IntervalRepeatNotifNumber;
                int intervalReminder = reminder.IntervalRepeatReminderNumber;

                // get reminder user
                var reminderUser = dbr.IT_Reminder_User.Where(w => w.ReminderID == data.ReminderID).ToList();
                foreach (var email in reminderUser)
                {
                    //emailSendTo.Add(email.SendToUser);
                    ListEmail.Add(email.SendToUserEmail);
                }

                actions.Add(
                    new Tbl_IT_Reminder_Task
                    {
                        Description = reminder.Description,
                        ReminderTitle = reminder.ReminderTitle,
                        SendToUser = emailSendTo.ToArray()
                    });

                SendEmail(reminder.ReminderTitle, reminder.Description, ListEmail);

                // update status isSend
                var task = dbr.IT_Reminder_Task.Where(w => w.ID == data.ID).FirstOrDefault();
                task.IsSend = 1;

                dbr.SaveChanges();
                
                if (repeatTask)
                {
                    // jika ada perulangan task
                    //get next reminder date
                    if (reminder.IntervalRepeatNotifType == "day")
                    {
                        newDate = DateTime.Now.AddDays(intervalNotifTask).Date;
                        newHours = Convert.ToInt32(reminder.NotifTime);
                    }
                    else
                    {
                        newDate = DateTime.Now.AddHours(intervalNotifTask);
                        newHours = Convert.ToInt32(DateTime.Now.AddHours(intervalNotifTask).Hour);
                    }
                    if (reminder.DueDate.Date > newDate)
                    {
                        // jika newdate tidak melibihi duedate, insert new task

                        // insert new task
                        IT_Reminder_Task newtask = new IT_Reminder_Task();
                        newtask.ReminderID = reminder.ID;
                        newtask.ReminderDate = newDate;
                        newtask.ReminderTime = newHours;
                        newtask.ReminderDueDate = data.ReminderDueDate;
                        newtask.IsSend = 0;

                        dbr.IT_Reminder_Task.Add(newtask);
                        dbr.SaveChanges();
                    }
                    else
                    {
                        // jika newdate sudah melebihi due date, cek apakah status repearreminder = true
                        if (repeatReminder)
                        {
                            if (reminder.IntervalRepetReminderType == "year")
                            {
                                // jika interval type = per x tahun
                                newDueDate = reminder.DueDate.AddYears(intervalReminder).Date;
                                newDate = newDueDate.AddDays(reminder.NotifStart);
                                newHours = reminder.DueDate.AddYears(intervalReminder).Hour;
                            }
                            else
                            {
                                // jika interval type = per x bulan
                                newDueDate = reminder.DueDate.AddMonths(intervalReminder).Date;
                                newDate = newDueDate.AddDays(reminder.NotifStart);
                                newHours = reminder.DueDate.AddMonths(intervalReminder).Hour;
                            }

                            IT_Reminder_Task newtask = new IT_Reminder_Task();
                            newtask.ReminderID = reminder.ID;
                            newtask.ReminderDate = newDate;
                            newtask.ReminderTime = newHours;
                            newtask.ReminderDueDate = newDueDate;
                            newtask.IsSend = 0;

                            dbr.IT_Reminder_Task.Add(newtask);
                            dbr.SaveChanges();
                        }
                    }
                } else
                {
                    if (repeatReminder)
                    {
                        // cek apakah ada repeat reminder
                        if (reminder.IntervalRepetReminderType == "year")
                        {
                            // jika interval type = per x tahun
                            newDueDate = reminder.DueDate.AddYears(intervalReminder).Date;
                            newDate = newDueDate.AddDays(reminder.NotifStart);
                            newHours = reminder.DueDate.AddYears(intervalReminder).Hour;
                        }
                        else
                        {
                            // jika interval type = per x bulan
                            newDueDate = reminder.DueDate.AddMonths(intervalReminder).Date;
                            newDate = newDueDate.AddDays(reminder.NotifStart);
                            newHours = reminder.DueDate.AddMonths(intervalReminder).Hour;
                        }

                        IT_Reminder_Task newtask = new IT_Reminder_Task();
                        newtask.ReminderID = reminder.ID;
                        newtask.ReminderDate = newDate;
                        newtask.ReminderTime = newHours;
                        newtask.ReminderDueDate = newDueDate;
                        newtask.IsSend = 0;

                        dbr.IT_Reminder_Task.Add(newtask);
                        dbr.SaveChanges();
                    } else
                    {
                        // nonaktifkan reminder
                        reminder.IsActive = 0;

                        dbr.SaveChanges();
                    }
                }
                
            }

            string formattedDate = newDate.ToString("yyyy-MM-dd HH:mm:ss");
            return Json(new { msg = "ok", data = actions, newDate = formattedDate }, JsonRequestBehavior.AllowGet); ;
        }
        [Authorize]
        [HttpGet]
        public ActionResult FormSettingReminder(int ID)
        {
            var reminder = dbr.IT_Reminder.Where(w => w.ID == ID).FirstOrDefault();
            var reminderUser = dbr.IT_Reminder_User.Where(w => w.ReminderID == ID).ToList();
            var ListEmail = db.V_Users_Active.Where(w => w.Email != null).GroupBy(g => g.Email).Select(s => new ListEmail { Email = s.Key }).ToList();
            var ListNewEmail = dbr.IT_Reminder_User.GroupBy(g => g.SendToUser).Select(s => new ListEmail { Email = s.Key }).ToList();
            var ListVendor = db.V_AXVendorList.ToList();            

            ViewBag.reminder = reminder;
            ViewBag.reminderUser = reminderUser;
            ViewBag.ListEmail = ListEmail.Union(ListNewEmail).GroupBy(x => x.Email).Select(y => new ListEmail { Email = y.Key }).ToList();
            ViewBag.ListVendor = ListVendor;
            return PartialView();
        }
        [HttpPost]
        public ActionResult UpdateSettingReminder(IT_Reminder smodel, string[] selReminderUser, string txtDueDate)
        {
            //var DueDate = DateTime.ParseExact(txtDueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            // action update IT_reminder // 
            var reminder = dbr.IT_Reminder.Where(w => w.ID == smodel.ID).FirstOrDefault();
            //reminder.ReminderTitle = smodel.ReminderTitle;
            //reminder.Module = smodel.Module;
            //reminder.Type = smodel.Type;
            //reminder.Thirdparty = smodel.Thirdparty;
            //reminder.DueDate = DueDate;
            //reminder.Description = smodel.Description;
            //reminder.NotifStart = smodel.NotifStart;
            //reminder.NotifTime = smodel.NotifTime;
            //reminder.IntervalRepetReminderType = smodel.IntervalRepetReminderType;
            //reminder.IntervalRepeatReminderNumber = smodel.IntervalRepeatReminderNumber;
            //reminder.IntervalRepeatNotifType = smodel.IntervalRepeatNotifType;
            //reminder.IntervalRepeatNotifNumber = smodel.IntervalRepeatNotifNumber;

            //string filePath = "";
            //string fileName = "";
            //HttpPostedFileBase uploadFile = Request.Files["FileAttachment"];
            //if (uploadFile != null && uploadFile.ContentLength > 0)
            //{
            //    fileName = uploadFile.FileName;
            //    string extension = Path.GetExtension(fileName);
            //    filePath = Server.MapPath("~/Files/IT/Reminder/");                 
            //    filePath = filePath + fileName;

            //    string deletedFile = filePath + reminder.Attachment;
            //    //remove old attachment
            //    if (System.IO.File.Exists(filePath))
            //    {
            //        System.IO.File.Delete(filePath);
            //    }
            //    reminder.Attachment = fileName;

            //    uploadFile.SaveAs(filePath);
            //}
            //else
            //{
            //     fileName = "not found";
            //    filePath = "not found";
            //}

            //int s = dbr.SaveChanges();

            //// action update reminder_user //
            //var reminder_user = dbr.IT_Reminder_User.Where(w => w.ReminderID == smodel.ID).ToList();
            //dbr.IT_Reminder_User.RemoveRange(reminder_user);
            //dbr.SaveChanges();
            //foreach (var user in selReminderUser)
            //{
            //    var userInfo = db.V_Users_Active.Where(w => w.NIK == user).FirstOrDefault();
            //    IT_Reminder_User userList = new IT_Reminder_User();
            //    userList.ReminderID = smodel.ID;
            //    userList.SendToUser = user;
            //    userList.SendToUserEmail = userInfo.Email;
            //    userList.IsActive = 1;

            //    dbr.IT_Reminder_User.Add(userList);
            //}
            //int u = dbr.SaveChanges();

            //// action update reminder task
            //// cek apakah ada task yg aktif
            //var reminderTask = dbr.IT_Reminder_Task.Where(w => w.ReminderID == smodel.ID && w.IsSend == 0);
            //var task = reminderTask.FirstOrDefault();
            //var countTask = reminderTask.Count();

            //if (countTask > 0)
            //{
            //    // update existing task
            //    task.ReminderDate = reminder.DueDate.AddDays(reminder.NotifStart).Date;
            //    task.ReminderDueDate = reminder.DueDate;
            //    task.ReminderTime = Convert.ToInt32(smodel.NotifTime);
            //    dbr.SaveChanges();

            //} else
            //{
            //    // insert new task
            //    IT_Reminder_Task newTask = new IT_Reminder_Task();
            //    newTask.ReminderID = smodel.ID;
            //    newTask.ReminderDate = reminder.DueDate.AddDays(reminder.NotifStart).Date;
            //    newTask.ReminderTime = Convert.ToInt32(smodel.NotifTime);
            //    newTask.ReminderDueDate = reminder.DueDate;

            //    dbr.IT_Reminder_Task.Add(newTask);
            //    dbr.SaveChanges();
            //}

            ////dbr.SaveChanges();
            //if (s > 0 || u > 0)
            //{
            //    return Json(new { msg = "Update Successfully", status = 1 });
            //} else
            //{
            //    return Json(new { msg = "Update Failed", status = 0});
            //}
            return Json(new { data = reminder , duedate = txtDueDate, id = smodel.ID});
            
        }
        public void SendEmail(string ReminderTitle, string Description, ArrayList emailSendTo)
        {
            //var currUser = ((ClaimsIdentity)User.Identity).GetUserId();


            string FilePath = Path.Combine(Server.MapPath("~/Emails/IT/Reminder/"), "notif.html");
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            //var act = Url.Action("DetailRequest", "MatProm", new { area = "Marketing", RequestNo = requestNo }, this.Request.Url.Scheme);
            //Repalce [newusername] = signup user name      
            MailText = MailText.Replace("##ReminderTitle##", ReminderTitle);
            MailText = MailText.Replace("##Description##", Description);
            //MailText = MailText.Replace("##Status##", Status);
            //MailText = MailText.Replace("##requestByName##", requestByName);

            var senderEmail = new MailAddress("ngkportal-notification@ngkbusi.com", "Portal Reminder");
            //var receiverEmail = new MailAddress(EmailAddress, "Receiver");
            var password = "100%NGKbusi!";
            var sub = "Portal Reminder";
            var body = MailText;
            var smtp = new SmtpClient
            {
                Host = "ngkbusi.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,

                Credentials = new NetworkCredential(senderEmail.Address, password)
            };
            using (var mess = new MailMessage()
            {
                From = senderEmail,
                Subject = sub,
                Body = body,
                IsBodyHtml = true
            })
            {

                foreach (string dataEmail in emailSendTo)
                {
                    if (dataEmail.Length > 0)
                    {
                        //var CurrUser = db.V_Users_Active.Where(w => w.NIK == dataEmail).First();
                        //var EmailAddress = CurrUser.Email;

                        mess.To.Add(new MailAddress(dataEmail));

                    }

                }
                //mess.Bcc.Add(new MailAddress("ikhsan.sholihin@ngkbusi.com"));
                smtp.Send(mess);
            }


        }
        [HttpGet]
        public ActionResult ViewPDF(string fileName)
        {
            string filePath = "~/Files/IT/Reminder/" + fileName;
            Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);

            return File(filePath, "application/pdf");
        }
    }
}