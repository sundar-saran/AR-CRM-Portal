using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace CRM_Buddies_Task.Models
{
    public class MenuModel
    {
        public int MenuId { get; set; }
        [Required(ErrorMessage = "Menu Name is required.")]
        [StringLength(100, ErrorMessage = "Menu Name cannot exceed 100 characters.")]
        public string MenuName { get; set; }
        [Required(ErrorMessage = "Menu URL is required.")]
        [StringLength(200, ErrorMessage = "Menu URL cannot exceed 200 characters.")]
        public string MenuURL { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedDate { get; set; }
    }
}