using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    public interface IAppEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
