using NGKBusi.Models;
using NGKBusi.Areas.Other.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.IO;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Globalization;
using NGKBusi.Areas.IT.Controllers;


namespace NGKBusi.Areas.Other.Controllers
{
    public class MRRController : Controller
    {

        DefaultConnection db = new DefaultConnection();
        MRRConnection dbm = new MRRConnection();

        public ActionResult Index()
        {
            ViewBag.NavHide = true;
            var currUserId = User.Identity.GetUserId();
            var currUserName = User.Identity.GetUserName();

            var user = db.V_Users_Active.FirstOrDefault(u => u.NIK == currUserId);

            var now = DateTime.Now.Date;
            string currentTime = now.ToString("yyyy-MM-ddTHH:mm:ss");
            string timeX = now.ToString("HH:mm");
            string dateX = now.ToString("yyyy-MM-dd");

            ViewBag.now = now;
            ViewBag.timeX = timeX;
            ViewBag.dateX = dateX;
            ViewBag.allowedUpdate = false;
            ViewBag.currUsr = currUserId;
            ViewBag.currUsrNIK = user;
            ViewBag.currUsrName = user?.Name ?? "unknown";
            return View();
        }

        [HttpPost]
        public JsonResult GetRoomDetails(int ID)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var currUserName = ((ClaimsIdentity)User.Identity).GetUserName();
            var CurrUser = db.V_Users_Active.FirstOrDefault(w => w.NIK == currUser);

            var rooms = dbm.OTH_MRR_Master_Rooms
                .Where(w => w.IDRoomCat == ID)
                .Select(room => new
                {
                    room.ID,
                    room.RoomTitle,
                    room.Image,
                    room.IDRoomCat,
                    room.ExtensionNumber,
                    prop = dbm.OTH_MRR_Rooms_Properties
                        .Where(p => p.RoomID == room.ID)
                        .Select(prop => new
                        {
                            prop.ID,
                            prop.RoomID,
                            prop.PropsName,
                            prop.Quantity
                        })
                        .ToList()
                })
                .ToList();

            return Json(rooms, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSchedule(int roomId)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();

            List<Tbl_Events> scheduleList = new List<Tbl_Events>();

            var bookings = dbm.OTH_MRR_Bookings.Where(b => b.Status == 1 && b.RoomID == roomId).ToList();

            foreach (var booking in bookings)
            {
                var color = booking.UserNIK == currUser ? "#DA6F22" : "#007582";

                scheduleList.Add(new Tbl_Events
                {
                    id = Convert.ToInt32(booking.ID),
                    title = booking.Subject,
                    start = booking.Day.ToString("yyyy-MM-dd") + "T" + booking.StartTime,
                    end = booking.Day.ToString("yyyy-MM-dd") + "T" + booking.EndTime,
                    color = color,
                    roomid = booking.RoomID,
                    attendance = Convert.ToInt32(booking.Attendance),
                    link = booking.Link,
                    linkmeet = booking.LinkMeet
                });
            }

            return Json(scheduleList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetScheduleByEventId(int? idBook = null)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).FirstOrDefault();
            var currUserName = ((ClaimsIdentity)User.Identity).GetUserName();

            List<Tbl_Events> scheduleList = new List<Tbl_Events>();

            var bookings = dbm.OTH_MRR_Bookings.Where(b => b.Status == 1 && b.ID == idBook).ToList();


            foreach (var booking in bookings)
            {
                var color = booking.UserNIK == currUser ? "#DA6F22" : "#007582";
                var room = dbm.OTH_MRR_Master_Rooms.FirstOrDefault(m => m.ID == booking.RoomID);
                var roomTitle = room != null ? room.RoomTitle : "Unknown Room";
                var userData = db.V_Users_Active.FirstOrDefault(w => w.NIK == booking.UserNIK);
                var name = userData != null ? userData.Name : "Unknown Name";

                scheduleList.Add(new Tbl_Events
                {
                    id = Convert.ToInt32(booking.ID),
                    subject = booking.Subject,
                    start = booking.Day.ToString("yyyy-MM-dd") + "T" + booking.StartTime,
                    end = booking.Day.ToString("yyyy-MM-dd") + "T" + booking.EndTime,
                    linkmeet = booking.LinkMeet,
                    roomid = booking.RoomID,
                    user = booking.UserNIK,
                    attendance = Convert.ToInt32(booking.Attendance),
                    username = name,
                    roomtitle = roomTitle,
                    timestamps = booking.Timestamps?.ToString("yyyy-MM-dd HH:mm:ss"),
                    color = color,
                    link = booking.Link
                });
            }


            return Json(scheduleList, JsonRequestBehavior.AllowGet);
        }

        private bool InRange(DateTime newStart, DateTime newEnd, DateTime existingStart, DateTime existingEnd)
        {
            return newStart < existingEnd && newEnd > existingStart;
        }

        [HttpPost]
        public JsonResult AddSchedule(/*int? idBook = null*/)
        {

            var now = DateTime.Now;
            var currentTime = DateTime.Now.AddDays(-1);
            string timeX = now.ToString("HH:mm");
            string dateX = now.ToString("yyyy-MM-dd");


            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            var usernik = Request["UserNIK"];
            var title = Request["RoomTitle"];
            var day = DateTime.Parse(Request["Day"]);
            var startTime = Request["StartTime"];
            var endTime = Request["EndTime"];
            var roomId = int.Parse(Request["RoomId"]);
            var subject = Request["Subject"];
            var linkMeet = Request["LinkMeet"];
            var link = Request["Link"];
            var attendance = int.Parse(Request["Attendance"]);

            linkMeet = string.IsNullOrWhiteSpace(linkMeet) ? null : linkMeet;
            link = string.IsNullOrWhiteSpace(link) ? null : link;


            int status = 0;
            string msg = "";
            int errorCode = 0;


            var timeFormat = new System.Text.RegularExpressions.Regex(@"^(0[0-9]|1[0-9]|2[0-3]|[0-9]):[0-5][0-9]$");

            if (!timeFormat.IsMatch(startTime))
            {
                msg = "Start time must be in HH:mm format (e.g., 12:00).";
                return Json(new { status = status, message = msg, errorCode = errorCode }, JsonRequestBehavior.AllowGet);
            }

            if (!timeFormat.IsMatch(endTime))
            {
                msg = "End time must be in HH:mm format (e.g., 12:00).";
                return Json(new { status = status, message = msg, errorCode = errorCode }, JsonRequestBehavior.AllowGet);
            }


            DateTime newStartTime = DateTime.Parse($"{day:yyyy-MM-dd} {startTime}");
            DateTime newEndTime = DateTime.Parse($"{day:yyyy-MM-dd} {endTime}");


            if (!DateTime.TryParse(day.ToString("yyyy-MM-dd") + " " + startTime, out newStartTime) ||
                !DateTime.TryParse(day.ToString("yyyy-MM-dd") + " " + endTime, out newEndTime))
            {
                msg = "Invalid date or time format.";
                return Json(new { status = status, message = msg, errorCode = errorCode }, JsonRequestBehavior.AllowGet);
            }

            if (newEndTime < newStartTime)
            {
                msg = "End time cannot be less than start time.";
                return Json(new { status = 0, message = msg, errorCode = 999 }, JsonRequestBehavior.AllowGet);
            }

            if (currentTime > day)
            {
                msg = "Date time cannot be less than today";
                return Json(new { status = status, message = msg, errorCode = errorCode }, JsonRequestBehavior.AllowGet);
            }

            DateTime startOfDay = day.Date.AddHours(7); 
            DateTime endOfDay = day.Date.AddHours(19);

            if (newStartTime < startOfDay || newEndTime > endOfDay)
            {
                msg = "The booking time must be between 07:00 and 19:00.";
                return Json(new { status = 0, message = msg, errorCode = errorCode }, JsonRequestBehavior.AllowGet);
            }


            var existingBookings = dbm.OTH_MRR_Bookings
                .Where(b => b.RoomID == roomId &&
                            b.Day == day &&
                            //b.ID != idBook &&
                            b.Status == 1)
                .ToList();

            var conflictingEvents = new List<OTH_MRR_Bookings>();

            foreach (var booking in existingBookings)
            {
                DateTime existingStartTime = DateTime.Parse($"{booking.Day:yyyy-MM-dd} {booking.StartTime}");
                DateTime existingEndTime = DateTime.Parse($"{booking.Day:yyyy-MM-dd} {booking.EndTime}");

                bool isOverlap = newStartTime < existingEndTime && newEndTime > existingStartTime;
                if (isOverlap)
                {
                    conflictingEvents.Add(booking);
                }
            }

            if (conflictingEvents.Count > 0)
            {
                // Menyusun pesan konflik
                var conflictMessages = new List<string>();
                foreach (var conflictingEvent in conflictingEvents)
                {
                    var room = dbm.OTH_MRR_Master_Rooms.FirstOrDefault(m => m.ID == conflictingEvent.RoomID);
                    var roomTitle = room != null ? room.RoomTitle : "Unknown Room";
                    var userData = db.V_Users_Active.FirstOrDefault(w => w.NIK == conflictingEvent.UserNIK);
                    var name = userData != null ? userData.Name : "Unknown Name";

                    conflictMessages.Add($"'{conflictingEvent.Subject}' in '{roomTitle}' ({conflictingEvent.StartTime} to {conflictingEvent.EndTime})");
                }

                msg = conflictingEvents.Count == 1
                    ? $"Schedule conflict detected with {conflictMessages.First()}. Please choose a different time."
                    : $"Multiple schedule conflicts detected:<br>{string.Join("<br>", conflictMessages)}";

                status = 0;
                errorCode = conflictingEvents.Count == 1 ? 101 : 102;

                return Json(new
                {
                    status = status,
                    message = msg,
                    errorCode = errorCode,
                    conflict = conflictingEvents.Select(ce => new
                    {
                        ce.ID,
                        ce.UserNIK,
                        RoomID = ce.RoomID,
                        Subject = ce.Subject,
                        LinkMeet = ce.LinkMeet,
                        Attendance = ce.Attendance,
                        Day = ce.Day.ToString("yyyy-MM-dd"),
                        StartTime = ce.StartTime,
                        EndTime = ce.EndTime
                    }).ToList()
                }, JsonRequestBehavior.AllowGet);
            }

            var roomR = dbm.OTH_MRR_Master_Rooms.FirstOrDefault(m => m.RoomTitle == title);
            var roomId2 = roomR != null ? roomR.ID : 0;

            int finalRoomId = (roomId > 0) ? roomId : roomId2;

            var user = db.V_Users_Active.FirstOrDefault(u => u.Name == usernik);
            var userNIK = user != null ? user.NIK : null;

            OTH_MRR_Bookings schedule = new OTH_MRR_Bookings
            {
                UserNIK = CurrUser?.NIK ?? usernik,
                RoomID = finalRoomId,
                Subject = subject,
                LinkMeet = linkMeet,
                Attendance = attendance,
                Link = link,
                Day = day,
                StartTime = startTime,
                EndTime = endTime,
                Status = 1,
                Timestamps = DateTime.Now
            };

            dbm.OTH_MRR_Bookings.Add(schedule);
            int ins = dbm.SaveChanges();

            if (ins > 0)
            {
                msg = "Workspace Reservation Added Successfully";
                status = 1;
            }
            else
            {
                msg = "Failed Save Schedule";
                status = 0;
            }

            return Json(new { status = status, message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateSchedule(int idBook)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();
            var currentTime = DateTime.Now.AddDays(-1);


            var usernik = Request["UserNIK"];
            var day = DateTime.Parse(Request["DayUpdate"]);
            var startTime = Request["StartTimeUpdate"];
            var endTime = Request["EndTimeUpdate"];
            var roomId = int.Parse(Request["RoomIdUpdate"]);
            var subject = Request["SubjectUpdate"];
            var linkmeet = Request["LinkMeetUpdate"];
            var link = Request["LinkUpdate"];
            var attendance = int.Parse(Request["AttendanceUpdate"]);

            linkmeet = string.IsNullOrWhiteSpace(linkmeet) ? null : linkmeet;
            link = string.IsNullOrWhiteSpace(link) ? null : link;

            int status = 0;
            string msg = "";
            int errorCode = 0;

            DateTime newStartTime;
            DateTime newEndTime;

            if (!DateTime.TryParse(day.ToString("yyyy-MM-dd") + " " + startTime, out newStartTime) ||
                !DateTime.TryParse(day.ToString("yyyy-MM-dd") + " " + endTime, out newEndTime))
            {
                msg = "Invalid date or time format.";
                return Json(new { status = status, message = msg, errorCode = 991 }, JsonRequestBehavior.AllowGet);
            }

            DateTime startOfDay = day.Date.AddHours(7); ;
            DateTime endOfDay = day.Date.AddHours(19);

            if (newStartTime < startOfDay || newEndTime > endOfDay)
            {
                msg = "The booking time must be between 07:00 and 19:00.";
                return Json(new { status = 0, message = msg, errorCode = 992 }, JsonRequestBehavior.AllowGet);
            }

            if (currentTime > day)
            {
                msg = "Data cannot be updated because it is outdated";
                return Json(new { status = status, message = msg, errorCode = errorCode }, JsonRequestBehavior.AllowGet);
            }

            if (newEndTime < newStartTime)
            {
                msg = "End time cannot be less than start time.";
                return Json(new { status = 0, message = msg, errorCode = 993 }, JsonRequestBehavior.AllowGet);
            }


            var schedule = dbm.OTH_MRR_Bookings.First(b => b.ID == idBook);
            var user = db.V_Users_Active.FirstOrDefault(u => u.Name == usernik);
            var userNIK = user != null ? user.NIK : null;

            schedule.UserNIK = CurrUser?.NIK ?? userNIK;
            schedule.RoomID = roomId;
            schedule.Subject = subject;
            schedule.LinkMeet = linkmeet;
            schedule.Attendance = attendance;
            schedule.Day = day;
            schedule.Link = link;
            schedule.StartTime = startTime;
            schedule.EndTime = endTime;
            schedule.Status = 1;
            schedule.Timestamps = DateTime.Now;

            var existingBookings = dbm.OTH_MRR_Bookings
                .Where(b => b.RoomID == roomId &&
                            b.Day == day &&
                            b.ID != idBook &&
                            b.Status == 1)
                .ToList();

            var conflictingEvents = new List<OTH_MRR_Bookings>();

            foreach (var booking in existingBookings)
            {
                if (!DateTime.TryParse($"{booking.Day:yyyy-MM-dd} {booking.StartTime}", out var existingStartTime) ||
                    !DateTime.TryParse($"{booking.Day:yyyy-MM-dd} {booking.EndTime}", out var existingEndTime))
                    continue;

                if (InRange(newStartTime, newEndTime, existingStartTime, existingEndTime))
                {
                    conflictingEvents.Add(booking);
                }
            }

            if (conflictingEvents.Count > 0)
            {
                status = 0;

                if (conflictingEvents.Count == 1)
                {
                    var conflictingEvent = conflictingEvents.First();
                    var room = dbm.OTH_MRR_Master_Rooms.FirstOrDefault(m => m.ID == conflictingEvent.RoomID);
                    var roomTitle = room != null ? room.RoomTitle : "Unknown Room";
                    var userData = db.V_Users_Active.FirstOrDefault(w => w.NIK == conflictingEvent.UserNIK);
                    var name = userData != null ? userData.Name : "Unknown Name";

                    msg = $"Conflict detected with the following existing reservation:<br><br>" +
                          $"<strong>'{conflictingEvent.Subject}'</strong> in <strong>{roomTitle}</strong> ({conflictingEvent.StartTime} to {conflictingEvent.EndTime})<br>";

                    errorCode = 101; // Kode untuk satu konflik

                    return Json(new
                    {
                        status = status,
                        message = msg,
                        errorCode = errorCode,
                        conflict = new
                        {
                            conflictingEvent.ID,
                            conflictingEvent.UserNIK,
                            RoomID = conflictingEvent.RoomID,
                            Subject = conflictingEvent.Subject,
                            StartTime = conflictingEvent.StartTime,
                            EndTime = conflictingEvent.EndTime,
                            Day = conflictingEvent.Day.ToString("yyyy-MM-dd")
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                else 
                {
                    msg = "Multiple conflicts detected with the following existing reservations:<br><br>";
                    errorCode = 102; 

                    foreach (var conflictingEvent in conflictingEvents)
                    {
                        var room = dbm.OTH_MRR_Master_Rooms.FirstOrDefault(m => m.ID == conflictingEvent.RoomID);
                        var roomTitle = room != null ? room.RoomTitle : "Unknown Room";
                        var userData = db.V_Users_Active.FirstOrDefault(w => w.NIK == conflictingEvent.UserNIK);
                        var name = userData != null ? userData.Name : "Unknown Name";

                        msg += $"<strong>'{conflictingEvent.Subject}'</strong> in <strong>{roomTitle}</strong> ({conflictingEvent.StartTime} to {conflictingEvent.EndTime})<br>";
                    }

                    return Json(new
                    {
                        status = status,
                        message = msg,
                        errorCode = errorCode,
                        conflict = conflictingEvents.Select(ce => new
                        {
                            ce.ID,
                            ce.UserNIK,
                            RoomID = ce.RoomID,
                            Subject = ce.Subject,
                            StartTime = ce.StartTime,
                            EndTime = ce.EndTime,
                            Day = ce.Day.ToString("yyyy-MM-dd")
                        }).ToList()
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            int updateSchedule = dbm.SaveChanges();

            if (updateSchedule > 0)
            {
                msg = "Workspace Reservation Updated Successfully";
                status = 1;
            }
            else
            {
                msg = "Failed Update Workspace";
                status = 0;
            }

            return Json(new { status = status, message = msg, roomId = roomId, errorCode = errorCode }, JsonRequestBehavior.AllowGet);
        }


        private string ExtractTime(string dateTimeString)
        {
            DateTime dateTime;
            if (DateTime.TryParse(dateTimeString, out dateTime))
            {
                return dateTime.ToString("HH:mm");
            }
            else
            {
                throw new Exception("Invalid Time Format.");
            }
        }

        [HttpPost]
        public JsonResult SubmitRequest(OTH_MRR_Requests requestModel)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            var CurrUser = db.V_Users_Active.Where(w => w.NIK == currUser).First();

            var idConflict = Int32.Parse(Request["idConflict"]);
            var NIKConflict = Request["NIKConflict"];
            var dateConflict = Request["dateConflict"];
            var startConflict = Request["startConflict"];
            var endConflict = Request["endConflict"];
            string startTime = ExtractTime(startConflict);
            string endTime = ExtractTime(endConflict);
            var roomConflict = Request["roomConflict"];
            var subjectConflict = Request["subjectConflict"];
            var attendanceConflict = Int32.Parse(Request["attendanceConflict"]);

            var NIKRequest = Request["NIKRequest"];
            var roomRequest = Request["roomRequest"];
            var dateRequest = Request["dateRequest"];
            var startRequest = Request["startRequest"];
            var endRequest = Request["endRequest"];
            var messageRequest = Request["messageRequest"];
            var attendanceRequest = Int32.Parse(Request["attendanceRequest"]);

            var existingRequests = dbm.OTH_MRR_Requests
         .Where(r => r.NIKRequest == NIKRequest && r.idConflict == idConflict && r.statusR == 2)
         .ToList();


            if (existingRequests.Any())
            {
                return Json(new { status = 0, message = "You have a pending request in this event, cannot submit another until it is resolved." }, JsonRequestBehavior.AllowGet);
            }

            var parseStart = DateTime.Parse(startRequest);
            var parseEnd = DateTime.Parse(endRequest);

            if (parseEnd < parseStart)
            {
                return Json(new { status = 0, message = "end time cannot be less than start time." }, JsonRequestBehavior.AllowGet);
            }

            var timeFormat = new System.Text.RegularExpressions.Regex(@"^(0[0-9]|1[0-9]|2[0-3]|[0-9]):[0-5][0-9]$");

            if (!timeFormat.IsMatch(startRequest))
            {
                return Json(new { status = 0, message = "Start time must be in HH:mm format (e.g., 12:00)." }, JsonRequestBehavior.AllowGet);
            }

            if (!timeFormat.IsMatch(endRequest))
            {
                return Json(new { status = 0, message = "End time must be in HH:mm format (e.g., 12:00)." }, JsonRequestBehavior.AllowGet);
            }

            int status = 0;
            string msg = "";

            OTH_MRR_Requests request = new OTH_MRR_Requests();
            request.idConflict = idConflict;
            request.NIKConflict = NIKConflict;
            request.dateConflict = dateConflict;
            request.startConflict = startTime;
            request.endConflict = endTime;
            request.roomConflict = roomConflict;
            request.attendanceConflict = attendanceConflict;
            request.subjectConflict = subjectConflict;
            request.NIKRequest = NIKRequest;
            request.roomRequest = roomRequest;
            request.dateRequest = dateRequest;
            request.startRequest = startRequest;
            request.endRequest = endRequest;
            request.messageRequest = messageRequest;
            request.attendanceRequest = attendanceRequest;
            request.timestamps = DateTime.Now;
            request.statusR = 2;
            dbm.OTH_MRR_Requests.Add(request);
            int ins = dbm.SaveChanges();

            if (ins > 0)
            {
                msg = "Your workspace request has been submitted.";
                status = 1;
                int idRequest = request.id;

                var emailConflict = db.Users.FirstOrDefault(w => w.NIK == NIKConflict);
                var emailRequest = db.Users.FirstOrDefault(w => w.NIK == NIKRequest);

                if (string.IsNullOrEmpty(emailConflict.Email))
                {
                    return Json(new { status = 0, message = "Your email is missing. Cannot send Request." }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrEmpty(emailRequest.Email))
                {
                    return Json(new { status = 0, message = "Email of recipient user is missing. Cannot send Request." }, JsonRequestBehavior.AllowGet);
                }

                try
                {
                    SendEmail(idRequest);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending email: " + ex.Message);
                }

                return Json(new { status = status, message = msg }, JsonRequestBehavior.AllowGet);
            }

            else
            {
                msg = "Failed to Make Request";
                status = 0;
            }

            return Json(new { status = status, message = msg }, JsonRequestBehavior.AllowGet);
        }

        public void SendEmail(int idRequest)
        {
            string FilePath = Path.Combine(Server.MapPath("~/Emails/Other/MRR/"), "notif.html");
            StreamReader str = new StreamReader(FilePath);
            string MailText = str.ReadToEnd();
            str.Close();

            var requests = dbm.OTH_MRR_Requests.FirstOrDefault(w => w.id == idRequest);
            
            var emailConflict = db.Users.FirstOrDefault(w => w.NIK == requests.NIKConflict);
            var emailRequest = db.V_Users_Active.FirstOrDefault(w => w.NIK == requests.NIKRequest);


            MailText = MailText.Replace("##RecipientName##", emailConflict?.Name?? "Unknown User");
            MailText = MailText.Replace("##ExistingUser##", emailConflict?.Email?? "Unknown User");
            MailText = MailText.Replace("##ExistingDate##", requests.dateConflict);
            MailText = MailText.Replace("##ExistingTimeStart##", requests.startConflict);
            MailText = MailText.Replace("##ExistingTimeEnd##", requests.endConflict);
            MailText = MailText.Replace("##ExistingRoom##", requests.roomConflict);
            MailText = MailText.Replace("##ExistingSubject##", requests.subjectConflict);
            MailText = MailText.Replace("##ExistingAttendance##", requests.attendanceConflict.ToString());

            MailText = MailText.Replace("##UserRequest##", emailRequest?.Email ?? "Unknown Requester");
            MailText = MailText.Replace("##RequestDate##", requests.dateRequest);
            MailText = MailText.Replace("##RequestTimeStart##", requests.startRequest);
            MailText = MailText.Replace("##RequestTimeEnd##", requests.endRequest);
            MailText = MailText.Replace("##RequestRoom##", requests.roomRequest);
            MailText = MailText.Replace("##RequestMessage##", requests.messageRequest);
            MailText = MailText.Replace("##RequestAttendance##", requests.attendanceRequest.ToString());

            var senderEmail = new MailAddress("ngkportal-notification@ngkbusi.com", "NWR Request");
            //var receiverEmail = new MailAddress(emailConflict.Email, "Receiver");
            var receiverEmail = new MailAddress("adisti.putri@ngkbusi.com", "Receiver");
            var password = "100%NGKbusi!";
            var sub = "NWR Conflict Request";
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
            using (var mess = new MailMessage(senderEmail, receiverEmail)
            {
                Subject = sub,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(mess);
            }
        }

        [HttpPost]
        public JsonResult GetRequest()
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();

            var requests = dbm.OTH_MRR_Requests
                .Where(r => r.statusR == 2 && r.NIKConflict == currUser)
                .Select(req => new {
                    req.id,
                    req.idConflict,
                    req.NIKConflict,
                    req.dateConflict,
                    req.startConflict,
                    req.endConflict,
                    req.roomConflict,
                    req.subjectConflict,
                    req.NIKRequest,
                    req.roomRequest,
                    req.dateRequest,
                    req.startRequest,
                    req.endRequest,
                    req.messageRequest,
                    req.statusR,
                    req.timestamps
                })
                .ToList();

            return Json(new { errorCode = 101, request = requests }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetRequestById(int id)
        {
            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();

            var requests = dbm.OTH_MRR_Requests
                .Where(r => r.statusR == 2 && r.NIKConflict == currUser && r.id == id)
                .Select(req => new {
                    req.id,
                    req.idConflict,
                    req.NIKConflict,
                    req.dateConflict,
                    req.startConflict,
                    req.endConflict,
                    req.roomConflict,
                    req.subjectConflict,
                    req.NIKRequest,
                    req.roomRequest,
                    req.dateRequest,
                    req.startRequest,
                    req.endRequest,
                    req.messageRequest,
                    req.statusR,
                    req.timestamps
                })
                .ToList();

            return Json(new { errorCode = 101, request = requests }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteSchedule(int idBook)
        {
            var booking = dbm.OTH_MRR_Bookings.FirstOrDefault(b => b.ID == idBook);

            var currentTime = DateTime.Now.AddDays(-1);
            if (currentTime > booking.Day)
            {
                return Json(new { success = false, message = "Workspace cannot be deleted because it is outdated." });
            }

            var currUser = ((ClaimsIdentity)User.Identity).GetUserId();
            if (booking.UserNIK != currUser)
            {
                return Json(new { success = false, message = "You have no right to delete this schedule."});
            }

            dbm.OTH_MRR_Bookings.Remove(booking);
            dbm.SaveChanges();

            return Json(new { success = true, message = "Workspace Deleted Successfully" });
        }

        //string FilePath = Path.Combine(Server.MapPath("~/Emails/IT/Reminder/"), "notif.html");


        //MailAddress to = new MailAddress("postmaster@eranet.id");
        //MailAddress from = new MailAddress("admin@eranet.id");
        //MailMessage message = new MailMessage(from, to);
        //message.Subject = "MRR Request";
        //    message.Body = "MRR Body";

        //    var password = "100%NGKbusi!";
        //var sub = "Portal Reminder";
        //var body = "bodyhtml";
        //var smtp = new SmtpClient
        //{
        //    Host = "mx-relay-1.eradata.id",
        //    Port = 2126,
        //    EnableSsl = true,
        //    DeliveryMethod = SmtpDeliveryMethod.Network,
        //    UseDefaultCredentials = false,
        //};

        //    try
        //    {
        //        smtp.Send(message);

        //    } catch (Exception ex)
        //    {

        //    }
    }
}
