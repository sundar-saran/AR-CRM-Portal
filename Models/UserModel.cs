using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM_Buddies_Task.Models
{
    public class UserModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full name can contain only letters and spaces")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; }

        public string City { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, number, and special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public int Role_Id { get; set; } = 1;
        public DateTime CreatedDate { get; set; }
        public string RoleName { get; set; }
        public bool is_Active { get; set; }
    }

    public class ProjectModel
    {
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Project Name is required.")]
        [StringLength(100, ErrorMessage = "Project Name cannot exceed 100 characters.")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "description is required.")]
        [StringLength(500, ErrorMessage = "description cannot exceed 500 characters.")]
        public string Description { get; set; }
        public string Status { get; set; }
        public string is_Active { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class ApplyProjectViewModel
    {
        public List<ProjectModel> AllProjects { get; set; }
        public List<ProjectModel> MyAppliedProjects { get; set; }
    }

    public class UserProjectViewModel
{
    public int UserProjectId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
}

    public class UserDashboardViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string City { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
