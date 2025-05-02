using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using NGKBusi.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System;
using System.Diagnostics;

namespace NGKBusi.SignalR
{
    [Authorize]
    public class NotificationHub : Hub
    {
        DefaultConnection db = new DefaultConnection();
        // Menyimpan connectionId untuk setiap pengguna yang login
        private static readonly Dictionary<string, string> UserConnections = new Dictionary<string, string>();

        //
        // Summary:
        //     Gets the user security information for the current HTTP request.
        //
        // Returns:
        //     The user security information for the current HTTP request.

        // Menyimpan koneksi user
        public override Task OnConnected()
        {
            var user = System.Web.HttpContext.Current.User; // Mendapatkan user dari HttpContext;

            var userId = ((ClaimsIdentity)user.Identity).GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                Groups.Add(Context.ConnectionId, userId);
            }

            //Console.WriteLine(userId);  
            //Console.WriteLine("test");
            //Debug.WriteLine("test debug");
            //Debug.WriteLine(userId);
            //Debug.WriteLine(user);

            return base.OnConnected();
        }

        // Menghapus koneksi user saat disconnect
        public override Task OnDisconnected(bool stopCalled)
        {
            var user = System.Web.HttpContext.Current.User; // Mendapatkan user dari HttpContext;
            var userId = ((ClaimsIdentity)user.Identity).GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                Groups.Remove(Context.ConnectionId, userId);
            }
            Console.WriteLine(userId);
            return base.OnDisconnected(stopCalled);
        }

        public void SendNotification(string userId, string message)
        {
            
            if (UserConnections.ContainsKey(userId))
            {
                Clients.Client(UserConnections[userId]).receiveNotification(message);
            }
        }
    }
}