using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Collections.Generic;
using WebApi2.Models;
using MailKit.Net.Smtp;
using MimeKit;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly UTeM_Student_Attendance_AppEntities ent;
        public EmailController(UTeM_Student_Attendance_AppEntities ent)
        {
            this.ent = ent;
        }

        public class EmailRequest
        {
            public string Email { get; set; }
        }

        [HttpPost("SendEmail")]
        public IActionResult SendEmail(EmailRequest emailRequest)
        { 
            var student = ent.Students.FirstOrDefault(x => x.Email == emailRequest.Email);

            var staff = new Staff();

            if (student == null)
            {
                staff = ent.Staff.FirstOrDefault(x => x.Email == emailRequest.Email);

                if (staff == null)
                {
                    return Ok(new { Status = "failed", Message = "The email address incorrect. " }); 
                }
            }

            var existingResets = ent.PasswordResets.Where(x => x.Email == emailRequest.Email 
                && x.ExpireTime > DateTime.Now).ToList();

            if (existingResets.Any())
            {
                foreach (var reset in existingResets)
                {
                    reset.ExpireTime = DateTime.Now; // expired now 
                }

                ent.SaveChanges();
            }

            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            var newPasswordReset = new PasswordReset
            {
                Email = emailRequest.Email,
                VerificationCode = code,
                CreatedTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddMinutes(30)
            };

            var userName = "";

            if (student != null)
            {
                newPasswordReset.MatricNo = student.MatricNo;
                userName = student.Name;
            }
            else
            {
                newPasswordReset.StaffId = staff.StaffId;
                userName = staff.Name;
            }

            ent.PasswordResets.Add(newPasswordReset);
            int passwordResetRow = ent.SaveChanges();

            if (passwordResetRow > 0)
            {
                // ready mail message 
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("UTeM Student Attendance App", 
                    "utemstudentattendanceapp@gmail.com"));
                message.To.Add(new MailboxAddress("", emailRequest.Email));
                message.Subject = "Your Verification Code - UTeM Student Attendance";
                message.Body = new TextPart("plain")
                {
                    Text = $"Your verification code is: {code} \nRegards, \nUTeM Student Attendance App"
                    //Text = $"Hello, {userName}\n\n" + 
                    // $"Your verification code is: {code}\n\n" + 
                    // "Please enter this code in the app within 30 minutes.\n" + 
                    // "Do not share this code with anyone.\n\n" + 
                    // "Regards,\n" + 
                    // "UTeM Student Attendance App"
                };

                try
                {
                    // use MailKit to send
                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.gmail.com", 587, false);
                        client.Authenticate("utemstudentattendanceapp@gmail.com", "");
                        client.Send(message);
                        client.Disconnect(true);
                    }

                    return Ok(new { Status = "success", Message = "Code sent successfully!" });
                }
                catch (Exception ex)
                {
                    return Ok(new { Status = "failed", Message = "Failed to send email " });

                }
            }
            else
            {
                return Ok(new { Status = "failed", Message = "Failed to insert to password reset table. " });
            }               
        }
    }
}
