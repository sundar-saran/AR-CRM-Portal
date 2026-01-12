using CRM_Buddies_Task.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace CRM_Buddies_Task.Filters
{
    public class SetLayoutDataAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            if (controller == null) return;

            string currentController = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string currentAction = filterContext.ActionDescriptor.ActionName;

            if (currentController == "Account" &&
                (currentAction == "Login" || currentAction == "Logout" || currentAction == "Register"))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var session = controller.HttpContext.Session;

            // Check if user is logged in
            if (session == null || session["UserId"] == null || session["RoleId"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" }
                    });
                return;
            }

            // If layout data not cached, load it
            if (session["LayoutDataCached"] == null || session["LayoutDataCached"].ToString() != "true")
            {
                int userId;
                if (int.TryParse(session["UserId"].ToString(), out userId))
                {
                    var dbHelper = new DbHelper();

                    string fullName = dbHelper.GetUserFullName(userId);
                    session["UserFullName"] = fullName ?? "User";

                    var menus = dbHelper.GetMenusByUser(userId);
                    session["UserMenus"] = menus;

                    var roles = dbHelper.GetRoles();
                    session["RoleList"] = roles;

                    session["LayoutDataCached"] = "true";
                }
            }

            // Always bind values from session to ViewBag
            controller.ViewBag.FullName = session["UserFullName"] ?? "User";
            controller.ViewBag.Menus = session["UserMenus"] as List<MenuItem> ?? new List<MenuItem>();
            controller.ViewBag.RoleList = session["RoleList"] as List<SelectListItem> ?? new List<SelectListItem>();
            controller.ViewBag.RoleId = session["RoleId"];

            base.OnActionExecuting(filterContext);
        }
    }
}
