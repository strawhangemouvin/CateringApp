using CateringApp.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CateringApp.Services.Impl
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

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("Smtp");
                var host = smtpSettings["Host"] ?? "smtp.gmail.com";
                var port = int.TryParse(smtpSettings["Port"], out int p) ? p : 587;
                var enableSsl = bool.TryParse(smtpSettings["EnableSsl"], out bool ssl) ? ssl : true;
                var senderEmail = smtpSettings["SenderEmail"] ?? "";
                var senderPassword = smtpSettings["SenderPassword"] ?? "";
                var senderName = smtpSettings["SenderName"] ?? "CateringApp";

                if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword))
                {
                    _logger.LogWarning("Kredensial SMTP belum dikonfigurasi di appsettings.json. Pengiriman email dilewati.");
                    return false;
                }

                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(senderEmail, senderName);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = htmlBody;
                mailMessage.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(host, port);
                smtpClient.EnableSsl = enableSsl;
                smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Timeout = 10000; // 10 seconds timeout

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email berhasil dikirim ke {toEmail}.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Gagal mengirim email ke {toEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string recipientName)
        {
            var subject = $"[CateringApp] Kode Verifikasi OTP Reset Password: {otpCode}";

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8f9fa; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 550px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.08); }}
        .header {{ background-color: #d9534f; color: #ffffff; padding: 30px 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; letter-spacing: 1px; }}
        .content {{ padding: 35px 30px; color: #333333; }}
        .otp-box {{ background-color: #fff3f3; border: 2px dashed #d9534f; border-radius: 10px; padding: 20px; text-align: center; margin: 25px 0; }}
        .otp-code {{ font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #d9534f; }}
        .footer {{ background-color: #f1f3f5; padding: 20px; text-align: center; font-size: 12px; color: #868e96; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <h1>🍽️ CateringApp</h1>
            <p style='margin: 5px 0 0 0; opacity: 0.9; font-size: 14px;'>Sistem Layanan Catering Premium</p>
        </div>
        <div class='content'>
            <h3 style='margin-top: 0; color: #212529;'>Halo, {recipientName}!</h3>
            <p style='line-height: 1.6;'>Kami menerima permintaan untuk mereset kata sandi akun CateringApp Anda. Gunakan kode verifikasi OTP di bawah ini untuk melanjutkan:</p>
            
            <div class='otp-box'>
                <span style='font-size: 13px; text-transform: uppercase; color: #6c757d; display: block; margin-bottom: 5px;'>Kode Verifikasi OTP Anda</span>
                <div class='otp-code'>{otpCode}</div>
                <small style='color: #888888; display: block; margin-top: 5px;'>Berlaku selama 15 menit</small>
            </div>

            <p style='line-height: 1.6; font-size: 13px; color: #666666;'>
                <strong>Perhatian Keamanan:</strong> Jangan berikan kode verifikasi ini kepada siapapun termasuk pihak CateringApp. Jika Anda tidak merasa melakukan permintaan ini, silakan abaikan email ini dan akun Anda tetap aman.
            </p>
        </div>
        <div class='footer'>
            &copy; {DateTime.Now.Year} CateringApp Indonesia. Seluruh hak cipta dilindungi.<br>
            Email otomatis, mohon tidak membalas email ini.
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}
