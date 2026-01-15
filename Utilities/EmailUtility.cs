using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace CRM_Buddies_Task.Utilities
{
    public static class EmailUtility
    {
        private static readonly string _host = ConfigurationManager.AppSettings["SmtpHost"];
        private static readonly int _port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
        private static readonly string _email = ConfigurationManager.AppSettings["SmtpEmail"];
        private static readonly string _password = ConfigurationManager.AppSettings["SmtpPassword"];
        private static readonly bool _enableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);

        public static void SendEmail(string toEmail, string subject, string body)
        {
            using (var client = new SmtpClient(_host, _port))
            {
                client.Credentials = new NetworkCredential(_email, _password);
                client.EnableSsl = _enableSsl;

                var mail = new MailMessage
                {
                    From = new MailAddress(_email, "CRM Buddies"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail);
                client.Send(mail);
            }
        }
    }
}
