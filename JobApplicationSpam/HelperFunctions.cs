using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace JobApplicationSpam
{
    public class EmailAttachment
    {
        public string Path { get; set; }
        public string Name { get; set; }
    }

    public class EmailData
    {
        public string ToEmail { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public IEnumerable<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();

    }
    public static class HelperFunctions
    {
        public static void SendEmail(EmailData emailData)
        {
            var emailUserName = Startup.Configuration["Data:JobApplicationSpam:Email:UserName"];
            var emailPassword = Startup.Configuration["Data:JobApplicationSpam:Email:Password"];
            var emailServer = Startup.Configuration["Data:JobApplicationSpam:Email:Server"];
            var emailPort = Int32.Parse(Startup.Configuration["Data:JobApplicationSpam:Email:Port"]);
            var fromAddress = new MailAddress(emailData.FromEmail, emailData.FromName, System.Text.Encoding.UTF8);
            fromAddress = new MailAddress("rene.ederer@gmx.de", emailData.FromName, System.Text.Encoding.UTF8);

            using (var smtpClient = new SmtpClient(emailServer, emailPort))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new System.Net.NetworkCredential(emailUserName, emailPassword);
                var toAddress = new MailAddress(emailData.ToEmail);
                using (var message =
                    new MailMessage(fromAddress, toAddress)
                    {
                        SubjectEncoding = System.Text.Encoding.UTF8,
                        Subject = emailData.Subject,
                        Body = emailData.Body,
                        BodyEncoding = System.Text.Encoding.UTF8
                    })
                {
                    foreach (var emailAttachment in emailData.Attachments)
                    {
                        message.Attachments.Add(new Attachment(emailAttachment.Path) { Name = emailAttachment.Name });
                    }
                    smtpClient.Send(message);
                }
            }
        }
    }
}
