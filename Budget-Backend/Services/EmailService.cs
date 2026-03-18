using System.Net;
using System.Net.Mail;

namespace InternalBudgetTracker.Services
{
    public class EmailService
    {
        public static void SendVerificationMail(string email, string token)
        {
            string subject = "Account Verification for InternalBudgetTrack";

            string message =
                "Hello,\n\n" +
                "Thank you for signing up.\n\n" +
                $"To complete your registration, please verify your email address by clicking the link below:\n\n" +
                $"https://localhost:7268/api/users/verify?token={token}\n\n" +
                "If the link does not open, copy and paste it into your browser.\n\n" +
                "If you did not create this account, you can safely ignore this email.\n\n" +
                "Regards,\nTeam";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("kantsakshi03@gmail.com");
                mail.To.Add(email);
                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = false;

                //6050 port
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587 ))
                {
                    smtp.Credentials = new NetworkCredential(
                        "kantsakshi03@gmail.com",
                        "lwthwbhusmkqhqwc"
                    );
                    smtp.EnableSsl = true;

                    smtp.Send(mail);
                }
            }
        }
    }
}
