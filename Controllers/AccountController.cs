using CRM_Buddies_Task.Data;
using CRM_Buddies_Task.Filters;
using CRM_Buddies_Task.Models;
using CRM_Buddies_Task.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CRM_Buddies_Task.Utilities;

namespace CRM_Buddies_Task.Controllers
{
    /// <summary>
    /// Account controller handling user authentication, profile management, project operations,
    /// lead management, and administrative functions for the CRM Buddies application.
    /// Implements JWT-based authentication and role-based authorization.
    /// </summary>
    public class AccountController : BaseController
    {
        private readonly IDbHelper _dbHelper;

        /// <summary>
        /// Default constructor initializing database concrete implementations.
        /// </summary>
        public AccountController() : this(new DbHelper()) { }


        /// <summary>
        /// Parameterized constructor for dependency injection and testing.
        /// </summary>
        public AccountController(IDbHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }



        // ====================================================================
        // AUTHENTICATION & USER REGISTRATION
        // ====================================================================

        /// <summary>
        /// Displays user registration form.
        /// </summary>
        public ActionResult Register()
        {
            return View();
        }


        /// <summary>
        /// Processes user registration form submission.
        /// Creates new user with default 'User' role (Role_Id = 1).
        /// </summary>
        [HttpPost]
        public ActionResult Register(UserModel user)
        {
            if (!ModelState.IsValid)
                return View(user);

            try
            {
                user.Role_Id = 1;
                int userId = _dbHelper.RegisterUser(user);
                Session["UserId"] = userId;
                Session["RoleId"] = 1;

                TempData["SuccessMessage"] = "Registration successful! Redirecting to login...";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Registration failed: " + ex.Message;
                return View(user);
            }
        }


        /// <summary>
        /// Displays login form and clears existing session/cookies.
        /// </summary
        public ActionResult Login()
        {
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies[".AspNet.ApplicationCookie"] != null)
            {
                var cookie = new HttpCookie(".AspNet.ApplicationCookie");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
            return View();
        }


        /// <summary>
        /// Authenticates user credentials and creates JWT token upon successful login.
        /// Sets session variables and generates JWT token.
        /// </summary>
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            if (!ModelState.IsValid)
                return View();
            if (_dbHelper.ValidateUser(email, password, out int userId, out int roleId))
            {
                Session["UserId"] = userId;
                Session["RoleId"] = roleId;

                string roleName;
                if (roleId == 2)
                {
                    roleName = "Admin";
                }
                else
                {
                    roleName = "User";
                }

                string token = JwtManager.GenerateToken(email, userId, roleName);

                Session["JwtToken"] = token;



                TempData["SuccessMessage"] = "Login successful! Redirecting...";
                return RedirectToAction("Welcome");
            }

            TempData["ErrorMessage"] = "Invalid email or password.";
            return View();
        }



        /// <summary>
        /// Logout user by clearing all session data and authentication cookies.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies[".AspNet.ApplicationCookie"] != null)
            {
                var cookie = new HttpCookie(".AspNet.ApplicationCookie");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("Login");
        }



        // ====================================================================
        // DASHBOARD & PROFILE MANAGEMENT
        // ====================================================================

        /// <summary>
        /// Displays welcome dashboard for authenticated users.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult Welcome()
        {
            if (Session["UserId"] == null || Session["RoleId"] == null)
                return RedirectToAction("Login");

            return View();
        }


        /// <summary>
        /// Displays admin dashboard with system
        /// Access restricted to users with Admin role (RoleId = 2).
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult AdminDashboard()
        {
            if (Session["UserId"] == null || Session["RoleId"] == null || (int)Session["RoleId"] != 2)
                return RedirectToAction("Login", "Account");

            int totalUsers = 0;
            string connStr = ConfigurationManager.ConnectionStrings["SUNDAR"].ConnectionString;

            using (var con = new SqlConnection(connStr))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Sundar_tbl_UserDetails WHERE is_Delete = 0", con))
                {
                    totalUsers = (int)cmd.ExecuteScalar();
                }
            }

            ViewBag.TotalUsers = totalUsers;
            return View();
        }



        /// <summary>
        /// Displays regular user dashboard with personalized information.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult UserDashboard()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserId"];
            var user = _dbHelper.GetUserProfile(userId);

            var appliedProjects = _dbHelper.GetAppliedProjects(userId);
            var approvedProjects = appliedProjects.Where(p => p.Status == "Approved").ToList();
            var pendingProjects = appliedProjects.Where(p => p.Status == "Pending").ToList();

            var dashboardModel = new UserDashboardViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                City = user.City,
                CreatedDate = user.CreatedDate,
            };

            return View(dashboardModel);
        }


        /// <summary>
        /// Displays current user's profile information.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult Profile()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserId"];
            var user = _dbHelper.GetUserProfile(userId);
            return View(user);
        }



        /// <summary>
        /// Displays profile editing form with current user data.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpGet]
        public ActionResult EditProfile()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserId"];
            var user = _dbHelper.GetUserProfile(userId);

            LoadReportingManagers();

            var model = new EditProfileViewModel
            {
                UserId = userId,
                FullName = user.FullName,
                City = user.City,
                Mobile = user.Mobile,
                ReportingTo = user.ReportingTo
            };


            return View(model);
        }


        /// <summary>
        /// Processes profile update form submission.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                _dbHelper.UpdateUserProfile(model);
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating profile: " + ex.Message;
            }

            return RedirectToAction("EditProfile");
        }



        // ====================================================================
        // USER MANAGEMENT
        // ====================================================================

        /// <summary>
        /// Displays list of all users
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult UserList()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            List<UserDetails> users = _dbHelper.GetAllUsers();
            return View(users);
        }



        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        public ActionResult UserList(UserDetailsViewModel model)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            List<UserDetails> users = _dbHelper.GetAllUsers();

            int rows = _dbHelper.UpdateUserFromViewModel(model);
            if (rows > 0)
                TempData["SuccessMessage"] = "User updated successfully!";
            else
                TempData["ErrorMessage"] = "No changes were made.";

            return View(users);
        }


        /// <summary>
        /// Displays user editing form for specific user.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpGet]
        public ActionResult EditUser(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            var user = _dbHelper.GetUserById(id);
            if (user == null)
                return HttpNotFound();

            LoadReportingManagers();

            var viewModel = new UserDetailsViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Mobile = user.Mobile,
                Email = user.Email,
                City = user.City,
                IsActive = user.is_Active,
                ReportingTo = user.ReportingTo   
            };


            return View(viewModel);
        }


        /// <summary>
        /// Processes user update form submission.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(UserDetailsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int rows = _dbHelper.UpdateUserFromViewModel(model);
            if (rows > 0)
                TempData["SuccessMessage"] = "User updated successfully!";
            else
                TempData["ErrorMessage"] = "No changes were made.";

            return RedirectToAction("UserList");
        }

        /// <summary>
        /// Displays form to add new user to system.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpGet]
        public ActionResult AddUser()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            LoadReportingManagers();
            return View(new UserDetails());
        }



        /// <summary>
        /// Processes new user creation form submission.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(UserDetails model)
        {
            if (!ModelState.IsValid)
            {
                LoadReportingManagers();
                return View(model);
            }

            try
            {
                bool result = _dbHelper.AddNewUser(model);

                if (result)
                {
                    TempData["SuccessMessage"] = "User added successfully!";
                    return RedirectToAction("UserList");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add user. User may already exist.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                return View(model);
            }
        }

        /// <summary>
        /// Updates user role.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserRole(int userId, int newRoleId)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            _dbHelper.UpdateUserRole(userId, newRoleId);
            return RedirectToAction("ManageRoles");
        }


        /// <summary>
        /// Displays role management interface.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult ManageRoles()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            var accessEntries = _dbHelper.GetAllAccessEntries();
            return View(accessEntries);
        }



        // ====================================================================
        // PROJECT MANAGEMENT
        // ====================================================================

        /// <summary>
        /// Displays project application interface with available projects.
        /// Shows approved projects.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult ProjectApply()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserId"];
            var allProjects = _dbHelper.GetAllProjects();
            var appliedProjects = _dbHelper.GetAppliedProjects(userId);

            var approved = appliedProjects
                            .Where(p => p.Status == "Approved")
                            .Select(p => p.ProjectName)
                            .ToList();

            if (approved.Any())
            {
                TempData["ApprovedNotice"] = $"Congratulations! Your project {(approved.Count == 1 ? "application" : "applications")} for: {string.Join(", ", approved)} has been approved.";
            }

            var viewModel = new ApplyProjectViewModel
            {
                AllProjects = allProjects,
                MyAppliedProjects = _dbHelper.GetPendingProjects(userId)
            };

            return View(viewModel);
        }


        /// <summary>
        /// Processes project application submission.
        /// Prevents duplicate applications and applications for already approved projects.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProjectApply(int projectId)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserId"];

            try
            {
                var appliedProjects = _dbHelper.GetAppliedProjects(userId);

                bool alreadyApplied = appliedProjects.Any(p => p.ProjectId == projectId);
                bool alreadyApproved = appliedProjects.Any(p => p.ProjectId == projectId && p.Status == "Assigned");

                if (alreadyApproved)
                {
                    TempData["ErrorMessage"] = "This project is already approved by Admin.";
                    return RedirectToAction("ProjectApply");
                }

                if (alreadyApplied)
                {
                    TempData["ErrorMessage"] = "You have already applied for this project.";
                    return RedirectToAction("ProjectApply");
                }

                _dbHelper.ApplyToProject(userId, projectId);
                // -------- SEND EMAIL TO REPORTING MANAGER --------
                int managerId = _dbHelper.GetReportingManagerId(userId);

                string managerEmail = _dbHelper.GetUserEmail(managerId);
                string managerName = _dbHelper.GetUserFullName(managerId);
                string employeeName = _dbHelper.GetUserFullName(userId);
                string projectName = _dbHelper.GetProjectName(projectId);

                string body = EmailTemplates.ProjectApplied(
                    managerName,
                    employeeName,
                    projectName
                );

                EmailUtility.SendEmail(
                    managerEmail,
                    "New Project Application",
                    body
                );
                TempData["SuccessMessage"] = "Applied successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Project Not Applied! " + ex.Message;
            }

            return RedirectToAction("ProjectApply");
        }


        /// <summary>
        /// Displays detailed information about a specific project.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult ProjectDetails(int projectId)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            var project = _dbHelper.GetProjectById(projectId);
            if (project == null)
                return HttpNotFound("Project not found.");

            return View(project);
        }


        /// <summary>
        /// Displays projects approved for the current user.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult YourProjects()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserId"];
            var approvedProjects = _dbHelper.GetApprovedProjects(userId);
            return View(approvedProjects);
        }


        /// <summary>
        /// Displays all project applications for admin review and management.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult ManageProjectApplications()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            var apps = _dbHelper.GetAllUserProjectApplications();
            return View(apps);
        }


        /// <summary>
        /// Updates project application status.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateApplicationStatus(int userProjectId, string status)
        {
            if (Session["UserId"] == null || (int?)Session["RoleId"] != 2)
                return RedirectToAction("Login");

            _dbHelper.UpdateUserProjectStatus(userProjectId, status);
            // -------- SEND EMAIL TO USER --------
            var details = _dbHelper.GetUserProjectApplicationById(userProjectId);

            string userEmail = details.UserEmail;
            string userName = details.UserName;
            string projectName = details.ProjectName;

            string body = status == "Assigned"
                ? EmailTemplates.ProjectApproved(userName, projectName)
                : EmailTemplates.ProjectRejected(userName, projectName);

            EmailUtility.SendEmail(
                userEmail,
                "Project Application Status Update",
                body
            );
            TempData["Message"] = status == "Assigned" ? "Project assigned successfully!" : "Project unassigned successfully!";
            return RedirectToAction("ManageProjectApplications");
        }


        /// <summary>
        /// Displays form to add new project.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpGet]
        public ActionResult AddProject()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            return View();
        }


        /// <summary>
        /// Processes new project creation form submission.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProject(ProjectModel model)
        {
            if (ModelState.IsValid)
            {
                bool result = _dbHelper.AddNewProject(model);

                if (result)
                {
                    TempData["SuccessMessage"] = "Project added successfully!";
                    return RedirectToAction("AddProject");
                }
                else
                {
                    TempData["ErrorMessage"] = "Something went wrong while adding the project.";
                }
            }

            return View(model);
        }



        /// <summary>
        /// Displays all projects in the system.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpGet]
        public ActionResult AllProjects()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            var allProjects = _dbHelper.ShowAllProjects();
            return View(allProjects);
        }


        /// <summary>
        /// Displays project editing form.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpGet]
        public ActionResult EditProject(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            var project = _dbHelper.ShowProjectById(id);
            if (project == null)
                return HttpNotFound();

            return View(project);
        }


        /// <summary>
        /// Processes project update form submission.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProject(ProjectModel model)
        {
            if (ModelState.IsValid)
            {
                bool updated = _dbHelper.UpdateProject(model);
                if (updated)
                {
                    TempData["SuccessMessage"] = "Project updated successfully!";
                    return RedirectToAction("EditProject");
                }
                TempData["ErrorMessage"] = "Failed to update project.";
            }
            return View(model);
        }


        /// <summary>
        /// Displays lead column management interface.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult ManageLead()
        {
            var columns = _dbHelper.GetLeadColumns();
            return View(columns);
        }


        /// <summary>
        /// Adds new column to lead management system.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        public ActionResult AddColumn(string columnName, string dataType)
        {
            var (success, message) = _dbHelper.AddLeadColumn(columnName, dataType);

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction("ManageLead");
        }


        /// <summary>
        /// Deletes column from lead management system.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        public ActionResult DeleteColumn(string columnName)
        {
            var (success, message) = _dbHelper.DeleteLeadColumn(columnName);

            if (success)
            {
                TempData["SuccessMessage"] = "Successfully Deleted Column!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error when you Perform Delete oparetions";
            }

            return RedirectToAction("ManageLead");
        }


        /// <summary>
        /// Displays lead application form with dynamic columns.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult ApplyForLead()
        {
            var columns = _dbHelper.GetLeadColumns();
            ViewBag.Columns = columns;
            return View();
        }



        /// <summary>
        /// Processes lead application form submission.
        /// Converts form data to JSON and stores in database.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        public ActionResult ApplyForLead(FormCollection form)
        {
            var dict = new Dictionary<string, string>();
            foreach (var key in form.AllKeys)
            {
                if (key == "__RequestVerificationToken") continue;
                dict[key] = form[key];
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
            int userId = Convert.ToInt32(Session["UserId"]);

            var (success, message) = _dbHelper.InsertLead(userId, json);

            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction("ApplyForLead");
        }


        /// <summary>
        /// Displays all leads in the system.
        /// Shows user-specific leads for regular users, all leads for admins.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult ViewLeads()
        {
            int? userId = null;
            if (Session["RoleId"] != null && Convert.ToInt32(Session["RoleId"]) == 1)
            {
            }
            else
            {
                userId = Convert.ToInt32(Session["UserId"]);
            }
            var leads = _dbHelper.GetLeads(userId);
            return View(leads);
        }



        /// <summary>
        /// Displays leads applied by the current user.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult MyAppliedLeads()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = Convert.ToInt32(Session["UserId"]);
            var userLeads = _dbHelper.GetLeads(userId);
            ViewBag.UserLeads = userLeads;

            return View();
        }



        /// <summary>
        /// Displays all lead applications for admin review and management.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult ManageLeadApplications()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            var applications = _dbHelper.GetAllLeadApplicationsWithDetails();
            return View(applications);
        }



        // ====================================================================
        // MENU MANAGEMENT
        // ====================================================================

        /// <summary>
        /// Displays menu management interface.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpGet]
        public ActionResult ManageMenus()
        {
            var menus = _dbHelper.GetAllMenus();
            return View(menus);
        }


        /// <summary>
        /// Displays form to add new menu.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpGet]
        public ActionResult AddMenu()
        {

            return View(new MenuModel { IsActive = true });
        }


        /// <summary>
        /// Processes new menu creation form submission.
        /// Invalidates menu cache to refresh navigation.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMenu(MenuModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                int newMenuId = _dbHelper.AddMenu(model);

                    TempData["SuccessMessage"] = "Menu added successfully!";
                    InvalidateMenuCache();
                return RedirectToAction("AddMenu"); 
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return View(model);
        }


        /// <summary>
        /// Displays menu editing form.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpGet]
        public ActionResult EditMenu(int id)
        {
            var menu = _dbHelper.GetAllMenus().FirstOrDefault(m => m.MenuId == id);
            if (menu == null)
                return HttpNotFound();

            return View("AddMenu", menu);
        }


        /// <summary>
        /// Processes menu update form submission.
        /// Invalidates menu cache to refresh navigation.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMenu(MenuModel model)
        {
            if (!ModelState.IsValid)
                return View("AddMenu", model);

            bool updated = _dbHelper.UpdateMenu(model);
            
                TempData["SuccessMessage"] = "Menu updated successfully!";
                InvalidateMenuCache();
                TempData["ErrorMessage"] = "Failed to update menu.";

                return RedirectToAction("EditMenu");
        }

        /// <summary>
        /// Invalidates menu cache to force refresh of navigation data.
        /// </summary>
        private void InvalidateMenuCache()
        {
            Session["LayoutDataCached"] = null;
        }


        // ====================================================================
        // ACCESS MANAGEMENT
        // ====================================================================

        /// <summary>
        /// Displays user access management interface.
        /// Shows all users with their current roles and menu access.
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        public ActionResult ManageAccess()
        {
            var accessList = _dbHelper.GetAllUserAccessDetails();
            return View(accessList);
        }

        // ====================================================================
        // API Management
        // ====================================================================

        /// <summary>
        /// To Get leads 
        /// </summary>
        /// [JwtAuthorize]
        [SetLayoutData]
        [HttpGet]
        [Route("Account/api/lead/get")]
        public JsonResult GetLeadColumns()
        {
            try
            {
                var columns = _dbHelper.GetLeadColumns();
                return Json(new
                {
                    success = true,
                    data = columns
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// To Post leads in the existing lead manage
        /// </summary>
        [JwtAuthorize]
        [SetLayoutData]
        [HttpPost]
        [Route("Account/api/lead/add")]
        public JsonResult AddLeadColumn(string columnName, string dataType)
        {
            try
            {
                var (success, message) = _dbHelper.AddLeadColumn(columnName, dataType);
                return Json(new
                {
                    success,
                    message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        //[Route("Account/api/jwtToken")]
        [HttpGet]
        public ActionResult GetJwtToken()
        {
            var token = Session["JwtToken"] as string;

            if (string.IsNullOrEmpty(token))
            {
                return Json(new { error = "Token not found. Please log in first." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { token }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Load the reporting manager name and id for the use better experiance
        /// </summary>
        private void LoadReportingManagers()
        {
            ViewBag.ReportingManagers = _dbHelper.GetAllUsers()
                .Where(u => u.Role_Id == 2 && u.is_Active)
                .Select(u => new SelectListItem
                {
                    Value = u.User_ID.ToString(),
                    Text = u.FullName
                })
                .ToList();
        }
    }
}