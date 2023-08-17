using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using Rolodex.Models;

namespace Rolodex.Services
{
    public class EmailService : IEmailSender
    {
		private readonly EmailSettings _emailSettings;
		public EmailService(IOptions<EmailSettings> emailSettings) 
		{ 
			_emailSettings = emailSettings.Value;
		}
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
			try
			{
				// get email sender settings
				var emailAddress = _emailSettings.EmailAddress 
					?? Environment.GetEnvironmentVariable("EmailAddress");
				var emailPassword = _emailSettings.EmailPassword
					?? Environment.GetEnvironmentVariable("EmailPassword");
				var emailHost = _emailSettings.EmailHost 
					?? Environment.GetEnvironmentVariable("EmailHost");
				var emailPort = _emailSettings.EmailPort != 0
					? _emailSettings.EmailPort
					: int.Parse(Environment.GetEnvironmentVariable("EmailPort")!);

				// instantiate email we're building
				MimeMessage newEmail = new();

				// set sender email
				newEmail.Sender = MailboxAddress.Parse(emailAddress);

				// set email recipient(s)
				foreach (string address in email.Split(";"))
				{
					//newEmail.To.Add(MailboxAddress.Parse(address));
					newEmail.Bcc.Add(MailboxAddress.Parse(address));
				}

				// set email subject
				newEmail.Subject = subject;

				// set email body
				BodyBuilder emailBody = new();
				emailBody.HtmlBody = htmlMessage;
				newEmail.Body = emailBody.ToMessageBody();

				// prep service and send
				using SmtpClient smtpClient = new();

				try
				{
					await smtpClient.ConnectAsync(emailHost, emailPort, SecureSocketOptions.StartTls);
					await smtpClient.AuthenticateAsync(emailAddress, emailPassword);
					await smtpClient.SendAsync(newEmail);
					await smtpClient.DisconnectAsync(true);
				}
				catch (Exception)
				{
                    throw;
				}
			}
			catch (Exception)
			{		
				throw;
			}
        }
    }
}