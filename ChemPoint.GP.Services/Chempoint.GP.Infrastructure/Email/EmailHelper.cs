using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Net.Mail;

namespace Chempoint.GP.Infrastructure.Email
{
    public class EmailHelper
    {
        /// <summary>
        /// Send Mailto warehouse
        /// <param name="message"> Message </param>
        /// <returns> bool </returns>
        /// </summary>
        public bool SendMail(EMailInformation email)
        {
            bool res;
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(email.EmailFrom);
                mail.To.Add(email.EmailTo);

                if (!email.EmailCc.Length.Equals(0))
                    mail.CC.Add(email.EmailCc);

                if (!email.EmailBcc.Length.Equals(0))
                    mail.Bcc.Add(email.EmailBcc);

                mail.Subject = email.Subject;
                mail.IsBodyHtml = true;
                mail.Body = email.Body;

                SmtpClient objSmtp = new SmtpClient(email.SmtpAddress);
                objSmtp.Credentials = new System.Net.NetworkCredential(email.UserId, email.Password);
                objSmtp.Send(mail);
                res = true;
            }
            catch (Exception ex)
            {
                res = false;
                throw ex;
            }
            return res;
        }
    }
}
