using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Web.Routing;

namespace NGKBusi.Helpers
{
    public class CheckExternalDomainAuth : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string DomainName = HttpContext.Current.Request.Url.Host;
            if (!HttpContext.Current.Request.Url.AbsoluteUri.ToLower().Contains("login"))
            {
                if ((DomainName == "202.152.35.148" || DomainName.ToLower() == "portal.ngkbusi.com") && (!HttpContext.Current.User.Identity.IsAuthenticated))
                {
                    filterContext.Result = new RedirectResult((DomainName == "portal.ngkbusi.com" ? "http://portal.ngkbusi.com/NGKBusi/User/Login": "http://202.152.35.148/NGKBusi/User/Login"));
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}