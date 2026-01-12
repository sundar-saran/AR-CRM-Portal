using CRM_Buddies_Task.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CRM_Buddies_Task.Filters
{
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var token = httpContext.Session["JwtToken"] as string;
            if (string.IsNullOrEmpty(token)) return false;

            var principal = JwtManager.ValidateToken(token);
            return principal != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/Account/Login");
        }
    }
}