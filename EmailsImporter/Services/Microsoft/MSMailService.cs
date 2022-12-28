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
        private GraphServiceClient _graphClient;
        private readonly AttachmentService _attachService;

        public MsMailService()
        {
            _msAuthService = new MsAuthService();
            _attachService = new AttachmentService();
        }

        public async Task<List<Email>> GetMessagesAsync(UserToken uToken)
        {
            // STEP 3/3 - Use token for interacting with MS
            _graphClient = await _msAuthService.GetGraphClientByTokenAsync(uToken);

            var emailAddress = await GetUserPrincipalNameAsync();
            var userAttachDirectoryPath = _attachService.CreateUserAttachDirectory(emailAddress, false);

            var messages = await GetMessagesAsync();
            return messages.Select(msg => GetEmail(msg, userAttachDirectoryPath)).ToList();
        }

        private async Task<string> GetUserPrincipalNameAsync()
        {
            var user = await _graphClient.Me
                .Request()
                .Select("userPrincipalName")
                .GetAsync();

            return user.UserPrincipalName;
        }

        private async Task<IMailFolderMessagesCollectionPage> GetMessagesAsync()
        {
            return await _graphClient.Me
                    .MailFolders.Inbox.Messages
                    .Request()
                    .Filter("isRead ne true")
                    .Expand("attachments")
                    .GetAsync();
        }

        private Email GetEmail(Message msg, string userAttachDirectoryPath)
        {
            var email = new Email
            {
                Subject = msg.Subject,
                From = msg.From.EmailAddress.Address,
                Body = msg.Body.Content,
                Date = msg.SentDateTime.HasValue ? msg.SentDateTime.Value.ToLocalTime().ToString(Constants.DateFormat) : "NA"
            };

            if (!msg.HasAttachments.HasValue)
                return email;

            var fileAttachments = msg.Attachments.OfType<FileAttachment>().ToList();
            if (fileAttachments.Count <= 0) return email;

            email.Attachments = fileAttachments.Select(a => a.Name).ToList();
            SaveMessageAttachments(msg.Id, fileAttachments, userAttachDirectoryPath);
            return email;
        }

        private void SaveMessageAttachments(string msgId, List<FileAttachment> fileAttachments, string userAttachDirectoryPath)
        {
            var msgFolderPath = _attachService.CreateMessageFolderIfNotExist(userAttachDirectoryPath, msgId);

            foreach (var attachment in fileAttachments)
            {
                _attachService.SaveAttachmentFile(msgFolderPath, attachment.Name, attachment.ContentBytes);
            }
        }
    }
}