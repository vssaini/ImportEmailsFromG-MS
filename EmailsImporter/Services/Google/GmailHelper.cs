using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EmailsImporter.Services.Google
{
    public class GmailHelper
    {
        private readonly GmailService _gmailService;

        public GmailHelper(GmailService gmailService)
        {
            _gmailService = gmailService;
        }

        public IList<Message> GetUnReadMessages(string emailAddress, bool includeSpamTrash)
        {
            UsersResource.MessagesResource.ListRequest request = _gmailService.Users.Messages.List(emailAddress);
            request.LabelIds = "INBOX";
            request.IncludeSpamTrash = includeSpamTrash;
            request.Q = "is:unread";

            ListMessagesResponse response = request.Execute();
            return response?.Messages ?? new List<Message>();
        }

        public async Task<Message> GetMessageWithPayloadAsync(string emailAddress, string msgId)
        {
            UsersResource.MessagesResource.GetRequest getRequest = _gmailService.Users.Messages.Get(emailAddress, msgId);

            Message msgContent = await getRequest.ExecuteAsync();
            return msgContent;
        }

        public void MarkMessageAsRead(string emailAddress, string messageId)
        {
            var request = new ModifyMessageRequest
            {
                AddLabelIds = null,
                RemoveLabelIds = new List<string> { "UNREAD" }
            };

            _gmailService.Users.Messages.Modify(request, emailAddress, messageId).Execute();
        }

        public string GetMessageBodyData(IList<MessagePart> parts)
        {
            if (parts.Count <= 0)
                return string.Empty;

            var nestedMsg = string.Empty;

            IList<MessagePart> plainParts = parts.Where(x => x.MimeType == "text/plain").ToList();
            if (plainParts.Count > 0)
            {
                foreach (MessagePart msgPart in plainParts)
                {
                    if (msgPart.Parts == null)
                    {
                        if (msgPart.Body?.Data != null)
                        {
                            nestedMsg += msgPart.Body.Data;
                        }
                    }
                    else
                    {
                        return GetMessageBodyData(msgPart.Parts);
                    }
                }
            }

            IList<MessagePart> attachmentParts = parts.Where(x => x.MimeType == "multipart/alternative").ToList();
            if (attachmentParts.Count <= 0) return nestedMsg;

            foreach (MessagePart msgPart in attachmentParts)
            {
                if (msgPart.Parts == null)
                {
                    if (msgPart.Body?.Data != null)
                    {
                        nestedMsg += msgPart.Body.Data;
                    }
                }
                else
                {
                    return GetMessageBodyData(msgPart.Parts);
                }
            }

            return nestedMsg;
        }

        public async Task<List<string>> GetMessageAttachmentsAsync(string userId, Message message)
        {
            var attachmentsFolder = ConfigurationManager.AppSettings["GoogleAttachmentsFolder"];
            var folderFullPath = HttpContext.Current.Server.MapPath(attachmentsFolder);

            var fileNames = new List<string>();
            var msgParts = message.Payload.Parts.Where(p => !string.IsNullOrWhiteSpace(p.Filename));
            foreach (var msgPart in msgParts)
            {
                MessagePartBody msgPartBody = await _gmailService.Users.Messages.Attachments.Get(userId, message.Id, msgPart.Body.AttachmentId).ExecuteAsync();

                SaveAttachment(msgPartBody, folderFullPath, msgPart.Filename);

                fileNames.Add(msgPart.Filename);
            }

            return fileNames;
        }

        private void SaveAttachment(MessagePartBody msgPartBody, string folderPath, string fileName)
        {
            var filePath = Path.Combine(folderPath, fileName);
            byte[] data = msgPartBody.Data.ToBytes();
            File.WriteAllBytes(filePath, data);
        }
    }
}