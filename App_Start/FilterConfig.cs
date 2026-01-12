using CRM_Buddies_Task.Filters;
using System.Web;
using System.Web.Mvc;

namespace CRM_Buddies_Task
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new SetLayoutDataAttribute());
        }
    }
}
