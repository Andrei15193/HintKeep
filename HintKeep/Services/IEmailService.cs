using System.Threading;
using System.Threading.Tasks;

namespace HintKeep.Services
{
    public interface IEmailService
    {
        Task SendAsync(EmailMessage emailMessage);
    }
}