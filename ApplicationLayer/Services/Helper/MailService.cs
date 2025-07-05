using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Security;
using MailKit.Net.Smtp;
using MimeKit;

namespace ApplicationLayer.Services.Helper
{
    public interface IMailService
    {
        public Task SendEmailAsync(string email, string subject, string message);
    }
    public class MailService : IMailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public MailService(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword)
        {
            Console.WriteLine($"SMTP Email: {smtpUsername}");
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mimeEmail = new MimeMessage();
            mimeEmail.From.Add(new MailboxAddress("Cinema City", _smtpUsername));
            mimeEmail.To.Add(new MailboxAddress("Receiver name", email));

            mimeEmail.Subject = subject;

            var bodyBuider = new BodyBuilder
            {
                HtmlBody = message
            };

            mimeEmail.Body = bodyBuider.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                await client.SendAsync(mimeEmail);
                await client.DisconnectAsync(true);
            }
        }
    }
}
