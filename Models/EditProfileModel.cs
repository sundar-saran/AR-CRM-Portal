using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRM_Buddies_Task.Models
{
    public class EditProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full name can contain only letters and spaces")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits")]
        public string Mobile { get; set; }
        public int? ReportingTo { get; set; }
        public string ReportingManager { get; set; }
        public string City { get; set; }
    }

}