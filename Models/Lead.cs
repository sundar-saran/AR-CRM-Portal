using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CRM_Buddies_Task.Models
{
    public class LeadColumnModel
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
    }

    public class LeadUserModel
    {
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
    }
    public class LeadApplicationModel
    {
        public int Id { get; set; }
        public int CreatedByUserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedByUserId { get; set; }
        public string ApprovedByName { get; set; }
        public string Status { get; set; }
    }

    public class LeadApplicationViewModel
    {
        public LeadApplicationModel Application { get; set; }
        public Dictionary<string, object> LeadData { get; set; }
    }

}