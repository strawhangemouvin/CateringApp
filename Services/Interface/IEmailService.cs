using System.Threading.Tasks;

namespace CateringApp.Services.Interface
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task<bool> SendOtpEmailAsync(string toEmail, string otpCode, string recipientName);
    }
}
