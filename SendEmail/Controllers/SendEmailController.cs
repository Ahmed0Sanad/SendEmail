using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using SendEmail.Models;
using MailKit.Net.Smtp;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SendEmail.Controllers
{
    public class SendEmailController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SendEmailController> _logger;

        public SendEmailController(IConfiguration configuration,ILogger<SendEmailController> logger)
        {
            _config = configuration;
            this._logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Email()
        {
            Email request = new Email()
            {
                To = "ahmedsanad880@gmail.com",
                Subject = "Test Email",
                Body = "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <title>Test Email</title>\r\n</head>\r\n<body style=\"font-family: Arial, sans-serif; color: #333;\">\r\n    <h2 style=\"color: #007bff;\">Hello from MailKit!</h2>\r\n    <p>This is a <strong>test email</strong> sent using <em>ASP.NET Core</em> and <em>MailKit</em>.</p>\r\n    <p style=\"margin-top: 20px;\">Have a great day! 😄</p>\r\n    <hr />\r\n    <small>This is an automated message. Please do not reply.</small>\r\n</body>\r\n</html>\r\n",
                IsBodyHtml = true

            };
            try
            {
                
                // create message
                var message = new MimeMessage();
                message.Sender = new MailboxAddress(_config["MailSettings:DisplayName"], _config["MailSettings:EmailFrom"]);
                message.To.Add(MailboxAddress.Parse(request.To));
                message.Subject = request.Subject;

                var builder = new BodyBuilder();
                if (request.IsBodyHtml)
                {
                    builder.HtmlBody = request.Body; // Use HTML body
                }
                else
                {
                    builder.TextBody = request.Body; // Use plain text body
                }

                message.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.Connect(_config["MailSettings:SmtpHost"], Convert.ToInt32(_config["MailSettings:SmtpPort"]), SecureSocketOptions.StartTls);
                smtp.Authenticate(_config["MailSettings:SmtpUser"], _config["MailSettings:SmtpPass"]);
                await smtp.SendAsync(message);
                smtp.Disconnect(true);
            }
            catch (System.Exception ex)
            {
                // Handle exception (you might want to log it)
                _logger.LogError(ex, "Error sending email");
                return BadRequest("Error sending email: " + ex.Message);
            }

            return Ok("done");
        }
    }
}
