using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Services
{
    public class EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger) : IEmailService
    {
        private readonly EmailSettings _emailSettings = emailSettings.Value;
        private readonly ILogger<EmailService> _logger = logger;

        public async Task SendEmailVerificationAsync(string email, string userName, string token)
        {
            var subject = "Activate Your Ruminster Account";
            var activationLink = $"{_emailSettings.BaseUrl}/activate?token={token}";
            
            var body = GetEmailVerificationTemplate(userName, activationLink);
            
            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string userName, string token)
        {
            var subject = "Reset Your Ruminster Password";
            var resetLink = $"{_emailSettings.BaseUrl}/reset-password?token={token}";
            
            var body = GetPasswordResetTemplate(userName, resetLink);
            
            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort);
                client.EnableSsl = _emailSettings.EnableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                throw;
            }
        }

        private string GetEmailVerificationTemplate(string userName, string activationLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Activate Your Account</title>
                </head>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px;'>
                        <h1 style='color: #333; text-align: center; margin-bottom: 30px;'>Welcome to Ruminster!</h1>
                        
                        <p style='color: #555; font-size: 16px; line-height: 1.6;'>Hello {userName},</p>
                        
                        <p style='color: #555; font-size: 16px; line-height: 1.6;'>
                            Thank you for signing up! To complete your registration and activate your account, 
                            please click the button below:
                        </p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{activationLink}' 
                               style='background-color: #007bff; color: white; padding: 15px 30px; 
                                      text-decoration: none; border-radius: 5px; font-size: 16px; 
                                      font-weight: bold; display: inline-block;'>
                                Activate Account
                            </a>
                        </div>
                        
                        <p style='color: #777; font-size: 14px; line-height: 1.6;'>
                            If you can't click the button, copy and paste this link into your browser:
                        </p>
                        <p style='color: #007bff; font-size: 14px; word-break: break-all;'>
                            {activationLink}
                        </p>
                        
                        <p style='color: #777; font-size: 14px; margin-top: 30px;'>
                            This link will expire in 30 minutes for security reasons.
                        </p>
                        
                        <p style='color: #777; font-size: 14px;'>
                            If you didn't create an account with us, please ignore this email.
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string GetPasswordResetTemplate(string userName, string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Reset Your Password</title>
                </head>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px;'>
                        <h1 style='color: #333; text-align: center; margin-bottom: 30px;'>Password Reset Request</h1>
                        
                        <p style='color: #555; font-size: 16px; line-height: 1.6;'>Hello {userName},</p>
                        
                        <p style='color: #555; font-size: 16px; line-height: 1.6;'>
                            We received a request to reset your password. If you made this request, 
                            click the button below to reset your password:
                        </p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' 
                               style='background-color: #dc3545; color: white; padding: 15px 30px; 
                                      text-decoration: none; border-radius: 5px; font-size: 16px; 
                                      font-weight: bold; display: inline-block;'>
                                Reset Password
                            </a>
                        </div>
                        
                        <p style='color: #777; font-size: 14px; line-height: 1.6;'>
                            If you can't click the button, copy and paste this link into your browser:
                        </p>
                        <p style='color: #007bff; font-size: 14px; word-break: break-all;'>
                            {resetLink}
                        </p>
                        
                        <p style='color: #777; font-size: 14px; margin-top: 30px;'>
                            This link will expire in 15 minutes for security reasons.
                        </p>
                        
                        <p style='color: #777; font-size: 14px;'>
                            If you didn't request a password reset, please ignore this email. Your password will remain unchanged.
                        </p>
                    </div>
                </body>
                </html>";
        }
    }

    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
