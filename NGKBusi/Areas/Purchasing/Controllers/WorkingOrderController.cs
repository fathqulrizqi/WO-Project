using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NGKBusi.Models;

namespace NGKBusi.Areas.Purchasing.Controllers
{
    public class WorkingOrderController : Controller
    {
        DefaultConnection db = new DefaultConnection();

        public ActionResult Index()
        {
            return View();
        }
    }
}