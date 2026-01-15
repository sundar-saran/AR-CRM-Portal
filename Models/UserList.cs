using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRM_Buddies_Task.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class UserDetails
    {
        public int User_ID { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Full Name must be under 100 characters")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City must be under 50 characters")]
        public string City { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public int? ReportingTo { get; set; }
        public string ReportingManager { get; set; }
        public bool is_Delete { get; set; }
        public bool is_Active { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? Created_Date { get; set; }
        public int Role_Id { get; set; }

    }


    public class AccessEntry
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }

    public class UserDetailsViewModel
    {
        public int UserId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$")]
        public string Mobile { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public int? ReportingTo { get; set; }
        public string ReportingManager { get; set; }
        public string City { get; set; }

        public bool IsActive { get; set; }

        public string NavFullName { get; set; }
        public List<MenuItem> Menus { get; set; }
    }

}