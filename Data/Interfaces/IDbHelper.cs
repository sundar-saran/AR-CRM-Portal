using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static CRM_Buddies_Task.Models.DbHelper;

namespace CRM_Buddies_Task.Models.Interfaces
{
    public interface IDbHelper
    {
        // User Management
        string HashPassword(string password);
        int RegisterUser(UserModel user);
        bool ValidateUser(string email, string password, out int userId, out int roleId);

        // User Profile & Details
        List<MenuItem> GetMenusByUser(int userId);
        string GetUserFullName(int userId);
        UserModel GetUserProfile(int userId);
        void UpdateUserProfile(EditProfileViewModel model);
        UserModel GetUserById(int userId);
        List<UserDetails> GetAllUsers();
        int UpdateUserFromViewModel(UserDetailsViewModel model);
        bool AddNewUser(UserDetails user);

        // Role & Access Management
        List<AccessEntry> GetAllAccessEntries();
        List<SelectListItem> GetRoles();
        void UpdateUserRole(int userId, int newRoleId);

        // Project Management
        List<ProjectModel> GetAllProjects();
        List<ProjectModel> GetAppliedProjects(int userId);
        void ApplyToProject(int userId, int projectId);
        List<UserProjectViewModel> GetAllUserProjectApplications();
        void UpdateUserProjectStatus(int userProjectId, string newStatus);
        ProjectModel GetProjectById(int projectId);
        List<ProjectModel> GetPendingProjects(int userId);
        List<ProjectModel> GetApprovedProjects(int userId);
        bool AddNewProject(ProjectModel project);
        List<ProjectModel> ShowAllProjects();
        ProjectModel ShowProjectById(int id);
        bool UpdateProject(ProjectModel model);

        int AddMenu(MenuModel menu);
        List<MenuModel> GetAllMenus();
        bool UpdateMenu(MenuModel menu);
        List<UserAccessInfo> GetAllUserAccessDetails();

        (bool success, string message) UpdateUserAccess(int userId, int newRoleId, string menuIds, int updaterId);

        List<LeadColumnModel> GetLeadColumns();
        (bool success, string message) AddLeadColumn(string columnName, string dataType);
        (bool success, string message) DeleteLeadColumn(string columnName);
        (bool success, string message) InsertLead(int userId, string leadJson);
        List<Dictionary<string, object>> GetLeads(int? userId = null);
        List<LeadApplicationViewModel> GetAllLeadApplicationsWithDetails();

        string GetUserEmail(int userId);
        //string GetUserFullName(int userId);
        string GetProjectName(int projectId);
        int GetReportingManagerId(int userId);
        UserProjectEmailModel GetUserProjectApplicationById(int userProjectId);

    }

}