using System.Collections.Generic;
using System.Web.Mvc;

namespace CRM_Buddies_Task.Models
{
    public class AccessManagementViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int CurrentRoleId { get; set; }
        public string CurrentRoleName { get; set; }
        public int NewRoleId { get; set; }
        public List<SelectListItem> AvailableRoles { get; set; }
        public string MenuIds { get; set; }
        public List<Menu> AvailableMenus { get; set; }
        public List<int> SelectedMenuIds { get; set; }
    }

    public class UserAccessInfo
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string MenuIds { get; set; }
        public string MenuNames { get; set; }

    }

    public class Menu
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string MenuURL { get; set; }
        public bool IsSelected { get; set; }
    }


}