using System.Threading;
using System.Threading.Tasks;

namespace HintKeep.Services
{
    public interface IEmailService
    {
        Task SendAsync(string emailAddress, string subject, string body);

        Task SendAsync(string emailAddress, string subject, string body, CancellationToken cancellationToken);
    }
}