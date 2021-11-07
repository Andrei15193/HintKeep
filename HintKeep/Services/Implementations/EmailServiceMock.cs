using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HintKeep.Services.Implementations
{
    public class EmailServiceMock : IEmailService
    {
        public Task SendAsync(string emailAddress, string subject, string body)
            => SendAsync(emailAddress, subject, body, default);

        public async Task SendAsync(string emailAddress, string subject, string body, CancellationToken cancellationToken)
        {
            var emailsDirectoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory()).CreateSubdirectory(".appData").CreateSubdirectory("emails");
            var emailFileName = Path.Combine(emailsDirectoryInfo.FullName, $"{DateTime.Now:yyyy-MM-dd HH-mm-ss} {subject}.txt");
            using (var emailFileStream = new FileStream(emailFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var emailTextWriter = new StreamWriter(emailFileStream, Encoding.UTF8))
            {
                await emailTextWriter.WriteLineAsync($"Subject: {subject}");
                await emailTextWriter.WriteLineAsync($"To: {emailAddress}");
                await emailTextWriter.WriteLineAsync("Body");
                await emailTextWriter.WriteAsync(body);
            }
        }
    }
}