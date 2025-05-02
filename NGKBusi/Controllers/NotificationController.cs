using NGKBusi.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NGKBusi.Controllers
{
    public class NotificationController : Controller
    {
        // GET: Notification
        public ActionResult PushNotification(string reqNo, List<string> user)
        {
            // Mengambil context hub untuk mengirimkan pesan
            var _hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            //// Mengirim notifikasi ke semua klien yang terhubung
            //hubContext.Clients.All.receiveNotification("Pesanan baru telah diterima!");

            //return Content("Notifikasi telah dikirim.");
            //int orderId = 234324;
            //var users = dbsp.SCM_Sparepart_User_Management.Where(w => w.Role == "Admin" || w.Role == "Developer").ToList();
            foreach (var usr in user)
            {
                var userId = usr;
                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                hubContext.Clients.Group(userId).receiveNotification($"{reqNo}");
            }



            return Content("Notifikasi dikirim ke user");
        }
    }
}