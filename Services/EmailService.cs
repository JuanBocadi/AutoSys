using System.Net;
using System.Net.Mail;

namespace AutoSys.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");

            var host = smtpSettings["Host"] ?? throw new InvalidOperationException("SMTP Host no configurado.");
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var user = smtpSettings["User"] ?? throw new InvalidOperationException("SMTP User no configurado.");
            var password = smtpSettings["Password"] ?? throw new InvalidOperationException("SMTP Password no configurado.");
            var fromEmail = smtpSettings["FromEmail"] ?? user;
            var fromName = smtpSettings["FromName"] ?? "AutoSys";
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(fromEmail, fromName);
                message.To.Add(new MailAddress(to));
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;

                using var client = new SmtpClient(host, port);
                client.Credentials = new NetworkCredential(user, password);
                client.EnableSsl = enableSsl;

                await client.SendMailAsync(message);
                _logger.LogInformation("Correo enviado exitosamente a {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {Email}", to);
                throw;
            }
        }
    }
}
