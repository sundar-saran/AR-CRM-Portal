using CRM_Buddies_Task.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CRM_Buddies_Task.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            Response.Cache.SetValidUntilExpires(false);
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            base.OnActionExecuting(filterContext);

            if (Session["UserId"] != null)
            {
                if (Session["LayoutDataCached"] == null)
                {
                    var dbHelper = new DbHelper();
                    int userId = (int)Session["UserId"];

                    ViewBag.FullName = dbHelper.GetUserFullName(userId) ?? "Ghost";
                    ViewBag.Menus = dbHelper.GetMenusByUser(userId);
                    ViewBag.RoleList = dbHelper.GetRoles();

                    Session["LayoutDataCached"] = true;
                }
                else
                {
                    ViewBag.FullName = Session["UserFullName"];
                    ViewBag.Menus = Session["UserMenus"] as List<MenuItem>;
                    ViewBag.RoleList = Session["RoleList"] as List<SelectListItem>;
                }
            }
        }
    }
}