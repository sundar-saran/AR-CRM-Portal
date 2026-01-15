namespace CRM_Buddies_Task.Utilities
{
    public static class EmailTemplates
    {
        public static string ProjectApplied(
            string managerName,
            string employeeName,
            string projectName)
        {
            return $@"
                <p>Dear {managerName},</p>
                <p><b>{employeeName}</b> has applied for the project 
                <b>{projectName}</b>.</p>
                <p>Please review and approve.</p>
                <br/>
                <p>Regards,<br/>CRM Buddies System</p>";
        }

        public static string ProjectApproved(
            string employeeName,
            string projectName)
        {
            return $@"
                <p>Dear {employeeName},</p>
                <p>Your application for project 
                <b>{projectName}</b> has been <b>APPROVED</b>.</p>
                <br/>
                <p>Regards,<br/>CRM Buddies Team</p>";
        }

        public static string ProjectRejected(
            string employeeName,
            string projectName)
        {
            return $@"
                <p>Dear {employeeName},</p>
                <p>Your application for project 
                <b>{projectName}</b> has been <b>REJECTED</b>.</p>
                <br/>
                <p>Regards,<br/>CRM Buddies Team</p>";
        }
    }
}
