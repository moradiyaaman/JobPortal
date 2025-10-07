using System.Threading.Tasks;

namespace JobPortal.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlContent);
    }
}
