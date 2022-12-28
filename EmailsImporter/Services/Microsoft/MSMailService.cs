using EmailsImporter.Models;
using EmailsImporter.Models.Microsoft;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constants = EmailsImporter.Models.Constants;

namespace EmailsImporter.Services.Microsoft
{
    public class MsMailService
    {
        private readonly MsAuthService _msAuthService;

        public MsMailService()
        {
            _msAuthService = new MsAuthService();
        }

        public async Task<List<Email>> GetMessagesAsync(UserToken uToken)
        {
            // STEP 32/3 - Use token for interacting with MS
            var graphClient = await _msAuthService.GetGraphClientByTokenAsync(uToken);
            var messages = await GetMessagesAsync(graphClient);

            var emails = new List<Email>();
            foreach (var msg in messages)
            {
                var email = new Email
                {
                    Subject = msg.Subject,
                    From = msg.From.EmailAddress.Address,
                    Body = msg.Body.Content,
                    Date = msg.SentDateTime.HasValue ? msg.SentDateTime.Value.ToLocalTime().ToString(Constants.DateFormat) : "NA",
                    Attachments = msg.Attachments.OfType<FileAttachment>().Select(a=>a.Name).ToList()
                };

                emails.Add(email);
            }

            return emails;
        }

        private async Task<IMailFolderMessagesCollectionPage> GetMessagesAsync(GraphServiceClient graphClient)
        {
            return await graphClient.Me
                    .MailFolders.Inbox.Messages
                    .Request()
                    .Expand("attachments")
                    .GetAsync();
        }
    }
}