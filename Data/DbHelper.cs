using CRM_Buddies_Task.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CRM_Buddies_Task.Models
{

    /// <summary>
    /// Data access layer implementation for CRM Buddies application.
    /// Provides database operations for user management, project management, 
    /// lead management, menu management, and access control.
    /// Implements the IDbHelper interface for dependency injection.
    /// </summary>
    public class DbHelper : IDbHelper
    {
        private static string connStr = ConfigurationManager.ConnectionStrings["SUNDAR"].ToString();


        // ====================================================================
        // SECURITY & AUTHENTICATION METHODS
        // ====================================================================

        /// <summary>
        /// Hashes a password using SHA256 algorithm for secure storage.
        /// </summary>
        public string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }


        // ====================================================================
        // USER MANAGEMENT METHODS
        // ====================================================================

        /// <summary>
        /// Registers a new user with default user role.
        /// </summary>
        public int RegisterUser(UserModel user)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "RegisterUser");
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Mobile", user.Mobile);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@City", user.City ?? "");
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Role_Id", user.Role_Id);
                    cmd.Parameters.AddWithValue("@ReportingTo", (object)user.ReportingTo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedOn", DBNull.Value);


                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.NextResult() && reader.Read())
                        {
                            if (reader.HasRows)
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    if (columnName.Equals("UserId", StringComparison.OrdinalIgnoreCase) ||
                                        columnName.Equals("User_ID", StringComparison.OrdinalIgnoreCase))
                                    {
                                        return Convert.ToInt32(reader[i]);
                                    }
                                }


                                return Convert.ToInt32(reader[0]);
                            }
                        }
                    }

                    return 0;
                }
            }
        }


        /// <summary>
        /// Validates user credentials against the database.
        /// </summary>
        public bool ValidateUser(string email, string password, out int userId, out int roleId)
        {
            userId = 0;
            roleId = 0;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "ValidateUser");
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userId = Convert.ToInt32(reader["User_ID"]);
                            roleId = Convert.ToInt32(reader["Role_ID"]);
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Retrieves complete user profile information by user ID.
        /// </summary>
        public UserModel GetUserProfile(int userId)
        {
            var user = new UserModel();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetUserData");
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && reader["ResultType"].ToString() == "SingleUser")
                        {
                            user.UserId = userId;
                            user.FullName = reader["FullName"].ToString();
                            user.Mobile = reader["Mobile"].ToString();
                            user.Email = reader["Email"].ToString();
                            user.City = reader["City"].ToString();
                            user.CreatedDate = Convert.ToDateTime(reader["Created_Date"]);
                            user.RoleName = reader["RoleName"].ToString();
                            user.ReportingTo = reader["ReportingTo"] == DBNull.Value
                                                ? null
                                                : (int?)Convert.ToInt32(reader["ReportingTo"]);

                            user.ReportingManager = reader["ReportingManager"]?.ToString();
                        }
                    }
                }
            }
            return user;
        }



        /// <summary>
        /// Updates user profile information in the database.
        /// </summary>
        public void UpdateUserProfile(EditProfileViewModel model)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "UpdateUser");
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
                    cmd.Parameters.AddWithValue("@FullName", model.FullName);
                    cmd.Parameters.AddWithValue("@Mobile", model.Mobile);
                    cmd.Parameters.AddWithValue("@City", model.City ?? "");
                    cmd.Parameters.AddWithValue("@ReportingTo", (object)model.ReportingTo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdaterId", model.UserId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// Retrieves a specific user by their ID.
        /// </summary>
        public UserModel GetUserById(int userId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetUserData");
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && reader["ResultType"].ToString() == "SingleUser")
                        {
                            return new UserModel
                            {
                                UserId = Convert.ToInt32(reader["User_ID"]),
                                FullName = reader["FullName"].ToString(),
                                Mobile = reader["Mobile"].ToString(),
                                Email = reader["Email"].ToString(),
                                City = reader["City"].ToString(),
                                is_Active = Convert.ToBoolean(reader["is_Active"]),
                                ReportingTo = reader["ReportingTo"] == DBNull.Value
                                        ? null
                                        : (int?)Convert.ToInt32(reader["ReportingTo"]),
                                ReportingManager = reader["ReportingManager"]?.ToString()
                            };

                        }
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Retrieves all users from Database.
        /// </summary>
        public List<UserDetails> GetAllUsers()
        {
            var users = new List<UserDetails>();

            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetUserData");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["ResultType"].ToString() == "AllUsers")
                            {
                                users.Add(new UserDetails
                                {
                                    User_ID = Convert.ToInt32(reader["User_ID"]),
                                    FullName = reader["FullName"].ToString(),
                                    Mobile = reader["Mobile"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    City = reader["City"].ToString(),
                                    is_Active = Convert.ToBoolean(reader["is_Active"]),
                                    Created_Date = reader["Created_Date"] as DateTime?,
                                    ReportingTo = reader["ReportingTo"] == DBNull.Value
                                        ? null
                                        : (int?)Convert.ToInt32(reader["ReportingTo"]),
                                    ReportingManager = reader["ReportingManager"]?.ToString()
                                });

                            }
                        }
                    }
                }
            }
            return users;
        }



        /// <summary>
        /// Updates user information from view model data.
        /// </summary>
        public int UpdateUserFromViewModel(UserDetailsViewModel model)
        {
            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "UpdateUser");
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
                    cmd.Parameters.AddWithValue("@FullName", model.FullName);
                    cmd.Parameters.AddWithValue("@Mobile", model.Mobile);
                    cmd.Parameters.AddWithValue("@City", model.City ?? "");
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                    cmd.Parameters.AddWithValue("@UpdaterId", HttpContext.Current.Session["UserId"] ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReportingTo", (object)model.ReportingTo ?? DBNull.Value);


                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Convert.ToInt32(reader["RowsAffected"]);
                        }
                    }
                    return 0;
                }
            }
        }


        /// <summary>
        /// Adds a new user to database.
        /// </summary>
        public bool AddNewUser(UserDetails user)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "AddNewUser");
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Mobile", user.Mobile);
                    cmd.Parameters.AddWithValue("@City", user.City);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@ReportingTo", (object)user.ReportingTo ?? DBNull.Value);

                    con.Open();

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result) == 1;
                    }

                    return false;
                }
            }
        }



        // ====================================================================
        // ROLE & ACCESS MANAGEMENT METHODS
        // ====================================================================

        /// <summary>
        /// Retrieves all access entries for role management.
        /// </summary>
        public List<AccessEntry> GetAllAccessEntries()
        {
            var list = new List<AccessEntry>();

            using (var con = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetAccessData");

                    con.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            if (rd["ResultType"].ToString() == "AllAccessData")
                            {
                                list.Add(new AccessEntry
                                {
                                    UserId = Convert.ToInt32(rd["User_ID"]),
                                    FullName = rd["FullName"].ToString(),
                                    Email = rd["Email"].ToString(),
                                    Mobile = rd["Mobile"].ToString(),
                                    RoleId = Convert.ToInt32(rd["Role_ID"]),
                                    RoleName = rd["RoleName"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            return list;
        }



        /// <summary>
        /// Retrieves all available roles.
        /// </summary>
        public List<SelectListItem> GetRoles()
        {
            var roles = new List<SelectListItem>();

            using (var con = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetRoles");

                    con.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            roles.Add(new SelectListItem
                            {
                                Value = rd["Role_ID"].ToString(),
                                Text = rd["RoleName"].ToString()
                            });
                        }
                    }
                }
            }
            return roles;
        }



        /// <summary>
        /// Updates a user's role.
        /// </summary>
        public void UpdateUserRole(int userId, int newRoleId)
        {
            int updaterId = HttpContext.Current?.Session?["UserId"] != null
                            ? Convert.ToInt32(HttpContext.Current.Session["UserId"])
                            : 0;

            using (var con = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "UpdateUserRole");
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@NewRoleId", newRoleId);
                    cmd.Parameters.AddWithValue("@UpdaterId", updaterId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// Retrieves user access details for access management interface.
        /// </summary>
        public List<UserAccessInfo> GetAllUserAccessDetails()
        {
            var accessList = new List<UserAccessInfo>();

            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetUserAccessDetails");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string menuIds = string.Empty;

                            try
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal("Menu_ID")))
                                {
                                    menuIds = reader["Menu_ID"]?.ToString() ?? string.Empty;
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                menuIds = string.Empty;
                            }

                            var menuNames = GetMenuNamesFromIds(menuIds);

                            accessList.Add(new UserAccessInfo
                            {
                                UserId = Convert.ToInt32(reader["User_ID"]),
                                FullName = reader["FullName"]?.ToString() ?? string.Empty,
                                Email = reader["Email"]?.ToString() ?? string.Empty,
                                RoleId = Convert.ToInt32(reader["Role_ID"]),
                                RoleName = reader["RoleName"]?.ToString() ?? string.Empty,
                                MenuIds = menuIds,
                                MenuNames = menuNames
                            });
                        }
                    }
                }
            }
            return accessList;
        }



        /// <summary>
        /// Updates user access with role and menu assignments.
        /// </summary>
        public (bool success, string message) UpdateUserAccess(int userId, int newRoleId, string menuIds, int updaterId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "UpdateUserAccess");
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Role_Id", newRoleId);
                    cmd.Parameters.AddWithValue("@UpdaterId", updaterId);

                    // Always pass MenuIds parameter, even if empty
                    cmd.Parameters.AddWithValue("@MenuIds", string.IsNullOrEmpty(menuIds) ? string.Empty : menuIds);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool flag = Convert.ToInt32(reader["Flag"]) == 1;
                            string msg = reader["Message"].ToString();
                            return (flag, msg);
                        }
                    }
                }
            }
            return (false, "Unexpected error");
        }




        // ====================================================================
        // PROJECT MANAGEMENT METHODS
        // ====================================================================

        /// <summary>
        /// Retrieves all available projects.
        /// </summary>
        public List<ProjectModel> GetAllProjects()
        {
            var projects = new List<ProjectModel>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetProjectData");

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["ResultType"].ToString() == "AllProjects")
                            {
                                projects.Add(new ProjectModel
                                {
                                    ProjectId = Convert.ToInt32(reader["Project_ID"]),
                                    ProjectName = reader["ProjectName"].ToString(),
                                    Description = reader["Description"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            return projects;
        }



        /// <summary>
        /// Retrieves projects applied by a specific user.
        /// </summary>
        public List<ProjectModel> GetAppliedProjects(int userId)
        {
            var applied = new List<ProjectModel>();

            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", "ManageUserProjects");
                cmd.Parameters.AddWithValue("@UserId", userId);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["ResultType"]?.ToString() == "UserProjects")
                        {
                            applied.Add(new ProjectModel
                            {
                                ProjectId = reader["Project_ID"] == DBNull.Value
                                    ? 0
                                    : Convert.ToInt32(reader["Project_ID"]),

                                ProjectName = reader["ProjectName"] == DBNull.Value
                                    ? string.Empty
                                    : reader["ProjectName"].ToString(),

                                Description = reader["Description"] == DBNull.Value
                                    ? string.Empty
                                    : reader["Description"].ToString(),

                                Status = reader["Status"] == DBNull.Value
                                    ? "Pending"
                                    : reader["Status"].ToString()
                            });
                        }
                    }
                }
            }

            return applied;
        }



        /// <summary>
        /// Submits a project application for a user.
        /// </summary>
        public void ApplyToProject(int userId, int projectId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "ManageUserProjects");
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@ProjectId", projectId);
                    cmd.Parameters.AddWithValue("@Status", "Apply");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }



        /// <summary>
        /// Retrieves all user project applications for admin review.
        /// </summary>
        public List<UserProjectViewModel> GetAllUserProjectApplications()
        {
            var list = new List<UserProjectViewModel>();
            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "ManageUserProjects");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["ResultType"].ToString() == "AllApplications")
                            {
                                list.Add(new UserProjectViewModel
                                {
                                    UserProjectId = (int)reader["UserProject_ID"],
                                    UserId = (int)reader["User_ID"],
                                    ProjectId = (int)reader["Project_ID"],
                                    ProjectName = reader["ProjectName"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Mobile = reader["Mobile"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            return list;
        }




        /// <summary>
        /// Updates the status of a user project application.
        /// </summary>
        public void UpdateUserProjectStatus(int userProjectId, string newStatus)
        {
            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "ManageUserProjects");
                    cmd.Parameters.AddWithValue("@UserProjectId", userProjectId);
                    cmd.Parameters.AddWithValue("@Status", newStatus);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }



        /// <summary>
        /// Retrieves a specific project by ID.
        /// </summary>
        public ProjectModel GetProjectById(int projectId)
        {
            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetProjectData");
                    cmd.Parameters.AddWithValue("@ProjectId", projectId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && reader["ResultType"].ToString() == "SingleProject")
                        {
                            return new ProjectModel
                            {
                                ProjectId = Convert.ToInt32(reader["Project_ID"]),
                                ProjectName = reader["ProjectName"].ToString(),
                                Description = reader["Description"].ToString(),
                            };
                        }
                    }
                }
            }
            return null;
        }



        /// <summary>
        /// Retrieves pending projects for a specific user.
        /// </summary>
        public List<ProjectModel> GetPendingProjects(int userId)
        {
            var list = new List<ProjectModel>();
            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "ManageUserProjects");
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Status", "Pending");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["ResultType"].ToString() == "UserProjectsByStatus")
                            {
                                list.Add(new ProjectModel
                                {
                                    ProjectId = Convert.ToInt32(reader["Project_ID"]),
                                    ProjectName = reader["ProjectName"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    Status = reader["Status"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            return list;
        }




        /// <summary>
        /// Retrieves approved/assigned projects for a specific user.
        /// </summary>
        public List<ProjectModel> GetApprovedProjects(int userId)
        {
            var list = new List<ProjectModel>();
            using (var conn = new SqlConnection(connStr))
            {
         

                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetApprovedProjectsByUser");
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ProjectModel
                            {
                                ProjectId = Convert.ToInt32(reader["Project_ID"].ToString()),
                                ProjectName = reader["ProjectName"].ToString(),
                                Description = reader["Description"].ToString(),
                                Status = reader["Status"].ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }



        /// <summary>
        /// Adds a new project in database.
        /// </summary>
        public bool AddNewProject(ProjectModel project)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "ManageProject");
                    cmd.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                    cmd.Parameters.AddWithValue("@Description", project.Description);

                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Convert.ToInt32(reader["RowsAffected"]) > 0;
                        }
                    }
                    return false;
                }
            }
        }



        /// <summary>
        /// Retrieves all projects with complete details.
        /// </summary>
        public List<ProjectModel> ShowAllProjects()
        {
            List<ProjectModel> projects = new List<ProjectModel>();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetProjectData");

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (dr["ResultType"].ToString() == "AllProjects")
                            {
                                projects.Add(new ProjectModel
                                {
                                    ProjectId = Convert.ToInt32(dr["Project_ID"]),
                                    ProjectName = dr["ProjectName"].ToString(),
                                    Description = dr["Description"].ToString(),
                                    is_Active = dr["is_Active"].ToString(),
                                    CreatedDate = dr["Created_Date"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(dr["Created_Date"])
                                });
                            }
                        }
                    }
                }
            }
            return projects;
        }



        /// <summary>
        /// Retrieves a specific project by ID with complete details.
        /// </summary>
        public ProjectModel ShowProjectById(int id)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetProjectData");
                    cmd.Parameters.AddWithValue("@ProjectId", id);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && reader["ResultType"].ToString() == "SingleProject")
                        {
                            return new ProjectModel
                            {
                                ProjectId = Convert.ToInt32(reader["Project_ID"]),
                                ProjectName = reader["ProjectName"].ToString(),
                                Description = reader["Description"].ToString(),
                                is_Active = reader["is_Active"].ToString(),
                                CreatedDate = Convert.ToDateTime(reader["Created_Date"])
                            };
                        }
                    }
                }
            }
            return null;
        }



        /// <summary>
        /// Updates an existing project.
        /// </summary>
        public bool UpdateProject(ProjectModel model)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "ManageProject");
                    cmd.Parameters.AddWithValue("@ProjectId", model.ProjectId);
                    cmd.Parameters.AddWithValue("@ProjectName", model.ProjectName);
                    cmd.Parameters.AddWithValue("@Description", model.Description);

                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Convert.ToInt32(reader["RowsAffected"]) > 0;
                        }
                    }
                    return false;
                }
            }
        }




        // ====================================================================
        // MENU MANAGEMENT METHODS
        // ====================================================================

        /// <summary>
        /// Adds a new menu.
        /// </summary>
        public int AddMenu(MenuModel menu)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "AddMenu");
                    cmd.Parameters.AddWithValue("@FullName", menu.MenuName);
                    cmd.Parameters.AddWithValue("@MenuURL", menu.MenuURL);
                    cmd.Parameters.AddWithValue("@Role_Id", 2);
                    cmd.Parameters.AddWithValue("@CreatedOn", DBNull.Value);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int flag = Convert.ToInt32(reader["Flag"]);
                            if (flag == 0)
                            {
                                string message = reader["Message"].ToString();
                                throw new InvalidOperationException(message);
                            }
                            if (reader.NextResult() && reader.Read())
                            {
                                return Convert.ToInt32(reader["NewMenuId"]);
                            }
                        }
                    }
                    return 0;
                }
            }
        }



        /// <summary>
        /// Retrieves all menus from database.
        /// </summary>
        public List<MenuModel> GetAllMenus()
        {
            var menus = new List<MenuModel>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetMenus");

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["ResultType"].ToString() == "AllMenus")
                            {
                                menus.Add(new MenuModel
                                {
                                    MenuId = Convert.ToInt32(reader["Menu_ID"]),
                                    MenuName = reader["MenuName"].ToString(),
                                    MenuURL = reader["MenuURL"].ToString(),
                                    IsActive = Convert.ToBoolean(reader["is_Active"]),
                                    IsDeleted = Convert.ToBoolean(reader["is_Delete"]),
                                    CreatedDate = reader["Created_Date"] as DateTime?
                                });
                            }
                        }
                    }
                }
            }
            return menus;
        }



        /// <summary>
        /// Updates an existing menu.
        /// </summary>
        public bool UpdateMenu(MenuModel menu)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "UpdateMenu");
                    cmd.Parameters.AddWithValue("@Menu_ID", menu.MenuId);
                    cmd.Parameters.AddWithValue("@FullName", menu.MenuName);
                    cmd.Parameters.AddWithValue("@MenuURL", menu.MenuURL);
                    cmd.Parameters.AddWithValue("@IsActive", menu.IsActive);
                    cmd.Parameters.AddWithValue("@UpdaterId", HttpContext.Current?.Session?["User Id"] ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Role_Id", 2);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }


        /// <summary>
        /// Retrieves navigation menus accessible by a specific user.
        /// </summary>
        public List<MenuItem> GetMenusByUser(int userId)
        {
            var menus = new List<MenuItem>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetUserMenus");
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            menus.Add(new MenuItem
                            {
                                Menu_ID = Convert.ToInt32(reader["Menu_ID"]),
                                MenuName = reader["MenuName"].ToString(),
                                MenuUrl = reader["MenuURL"].ToString()
                            });
                        }
                    }
                }
            }
            return menus;
        }



        // ====================================================================
        // LEAD MANAGEMENT METHODS
        // ====================================================================

        /// <summary>
        /// Retrieves all lead columns from the dynamic lead.
        /// </summary>
        public List<LeadColumnModel> GetLeadColumns()
        {
            var dt = ExecSp("GetColumns", new Dictionary<string, object>());
            var list = new List<LeadColumnModel>();
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new LeadColumnModel
                {
                    ColumnName = r["COLUMN_NAME"].ToString(),
                    DataType = r["DATA_TYPE"].ToString(),
                });
            }
            return list;
        }



        /// <summary>
        /// Adds a new column to the dynamic lead system.
        /// </summary>
        public (bool success, string message) AddLeadColumn(string columnName, string dataType)
        {
            var dt = ExecSp("AddColumn", new Dictionary<string, object>
            {
                { "@ColumnName", columnName },
                { "@DataType", dataType }
            });
            if (dt.Rows.Count > 0)
            {
                bool flag = Convert.ToInt32(dt.Rows[0]["Flag"]) == 1;
                string msg = dt.Rows[0]["Message"].ToString();
                return (flag, msg);
            }
            return (false, "Unexpected error");
        }



        /// <summary>
        /// Deletes a column from the dynamic lead.
        /// </summary>
        public (bool success, string message) DeleteLeadColumn(string columnName)
        {
            var dt = ExecSp("DeleteColumn", new Dictionary<string, object>
            {
                { "@ColumnName", columnName }
            });
            if (dt.Rows.Count > 0)
            {
                bool flag = Convert.ToInt32(dt.Rows[0]["Flag"]) == 1;
                string msg = dt.Rows[0]["Message"].ToString();
                return (flag, msg);
            }
            return (false, "Unexpected error");
        }




        /// <summary>
        /// Inserts a new lead application with dynamic form database.
        /// </summary>
        public (bool success, string message) InsertLead(int userId, string leadJson)
        {
            var columns = new List<string>();
            var values = new List<string>();

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(leadJson);

            foreach (var kvp in dict)
            {
                columns.Add("[" + kvp.Key + "]");
                values.Add("'" + kvp.Value.Replace("'", "''") + "'");
            }

            var columnList = string.Join(", ", columns);
            var valueList = string.Join(", ", values);

            var dt = ExecSp("InsertLead", new Dictionary<string, object>
            {
                { "@UserId", userId },
                { "@Columns", columnList },
                { "@Values", valueList }
            });

            if (dt.Rows.Count > 0)
            {
                bool flag = Convert.ToInt32(dt.Rows[0]["Flag"]) == 1;
                string msg = dt.Rows[0]["Message"].ToString();
                return (flag, msg);
            }
            return (false, "Unexpected error");
        }



        /// <summary>
        /// Retrieves leads from the databse, optionally filtered by user.
        /// </summary>
        public List<Dictionary<string, object>> GetLeads(int? userId = null)
        {
            var dictList = new List<Dictionary<string, object>>();
            var dt = ExecSp("GetLeads", new Dictionary<string, object>
            {
                { "@UserId", userId ?? (object)DBNull.Value }
            });
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                dictList.Add(dict);
            }
            return dictList;
        }



        /// <summary>
        /// Retrieves all lead applications with detailed information.
        /// </summary>
        public List<LeadApplicationViewModel> GetAllLeadApplicationsWithDetails()
        {
            var applications = new List<LeadApplicationModel>();
            var leadDataDict = new Dictionary<int, Dictionary<string, object>>();

            // Get applications
            var dtApps = ExecSp("GetAllLeadApplications", new Dictionary<string, object>());
            foreach (DataRow row in dtApps.Rows)
            {
                applications.Add(new LeadApplicationModel
                {
                    Id = Convert.ToInt32(row["Id"]),
                    CreatedByUserId = Convert.ToInt32(row["CreatedByUserId"]),
                    UserName = row["UserName"].ToString(),
                    UserEmail = row["UserEmail"].ToString(),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                });
            }

            foreach (var app in applications)
            {
                var details = GetLeadApplicationDetails(app.Id);
                leadDataDict[app.Id] = details;
            }

            var result = new List<LeadApplicationViewModel>();
            foreach (var app in applications)
            {
                result.Add(new LeadApplicationViewModel
                {
                    Application = app,
                    LeadData = leadDataDict.ContainsKey(app.Id) ? leadDataDict[app.Id] : new Dictionary<string, object>()
                });
            }

            return result;
        }


        // ====================================================================
        // UTILITY & HELPER METHODS
        // ====================================================================

        /// <summary>
        /// Retrieves the full name of a user by their ID.
        /// </summary>
        public string GetUserFullName(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Action", "GetUserData");
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader["FullName"].ToString();
                        }
                    }

                    return "User";
                }
            }
        }



        /// <summary>
        /// Converts menu IDs to comma-separated menu names for display.
        /// </summary>
        private string GetMenuNamesFromIds(string menuIds)
        {
            if (string.IsNullOrEmpty(menuIds)) return string.Empty;

            var menuNames = new List<string>();
            var ids = menuIds.Split(',').Select(id => id.Trim());

            using (var conn = new SqlConnection(connStr))
            {
                using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", "GetMenuDetails");

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var menuId = reader["Menu_ID"].ToString();
                            if (ids.Contains(menuId))
                            {
                                menuNames.Add(reader["MenuName"].ToString());
                            }
                        }
                    }
                }
            }

            return string.Join(", ", menuNames);
        }


        /// <summary>
        /// Retrieves detailed data for a specific lead application.
        /// </summary>
        private Dictionary<string, object> GetLeadApplicationDetails(int leadId)
        {
            var dt = ExecSp("GetLeadApplicationDetails", new Dictionary<string, object>
            {
                { "@LeadId", leadId }
            });

            var details = new Dictionary<string, object>();
            if (dt.Rows.Count > 0)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ColumnName != "Id" && column.ColumnName != "CreatedByUserId" &&
                        column.ColumnName != "CreatedDate" && column.ColumnName != "ApprovedDate" &&
                        column.ColumnName != "ApprovedByUserId" && column.ColumnName != "Status")
                    {
                        details[column.ColumnName] = dt.Rows[0][column];
                    }
                }
            }
            return details;
        }


        /// <summary>
        /// Executes a stored procedure with parameters and returns results as DataTable.
        /// Generic method for database operations using the main stored procedure.
        /// </summary>
        public static DataTable ExecSp(string action, Dictionary<string, object> parameters)
        {
            var dt = new DataTable();
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SUNDAR"].ConnectionString))
            using (var cmd = new SqlCommand("sp_Sundar_CRM_Project", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Action", action);

                if (parameters != null)
                {
                    foreach (var kvp in parameters)
                    {
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                    }
                }

                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        public string GetUserEmail(int userId)
        {
            string email = "";
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT Email FROM Sundar_tbl_UserDetails WHERE User_ID = @UserId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                email = Convert.ToString(cmd.ExecuteScalar());
            }
            return email;
        }
        public string GetProjectName(int projectId)
        {
            string projectName = "";
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT ProjectName FROM Sundar_tbl_Project WHERE Project_Id = @ProjectId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ProjectId", projectId);
                con.Open();
                projectName = Convert.ToString(cmd.ExecuteScalar());
            }
            return projectName;
        }
        public int GetReportingManagerId(int userId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = "SELECT ReportingTo FROM Sundar_tbl_UserDetails WHERE User_ID = @UserId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                object result = cmd.ExecuteScalar();

                return result == DBNull.Value || result == null
                    ? 0
                    : Convert.ToInt32(result);
            }
        }
        public UserProjectEmailModel GetUserProjectApplicationById(int userProjectId)
        {
            UserProjectEmailModel model = new UserProjectEmailModel();

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
            SELECT 
                u.User_ID,
                u.Email,
                u.FullName,
                p.ProjectName
            FROM Sundar_tbl_UserProjects up
            INNER JOIN Sundar_tbl_UserDetails u ON up.User_Id = u.User_ID
            INNER JOIN Sundar_tbl_Project p ON up.Project_Id = p.Project_Id
            WHERE up.UserProject_Id = @UserProjectId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserProjectId", userProjectId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    model.UserId = Convert.ToInt32(reader["User_ID"]);
                    model.UserEmail = reader["Email"].ToString();
                    model.UserName = reader["FullName"].ToString();
                    model.ProjectName = reader["ProjectName"].ToString();
                }
            }

            return model;
        }

    }
}