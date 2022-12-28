using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailsImporter.Services.Google
{
    public class GmailHelper
    {
        private readonly GmailService _gmailService;
        private readonly AttachmentService _attachService;

        public GmailHelper(GmailService gmailService, AttachmentService attachService)
        {
            _gmailService = gmailService;
            _attachService = attachService;
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

        public async Task<List<string>> GetMessageAttachmentsAsync(string emailAddress, Message message, string userAttachDirectoryPath)
        {
            var fileNames = new List<string>();

            var msgParts = message.Payload.Parts.Where(p => !string.IsNullOrWhiteSpace(p.Filename)).ToList();
            if (msgParts.Count <= 0) return fileNames;

            var msgFolderPath = _attachService.CreateMessageFolderIfNotExist(userAttachDirectoryPath, message.Id);

            foreach (var msgPart in msgParts)
            {
                MessagePartBody msgPartBody = await _gmailService.Users.Messages.Attachments.Get(emailAddress, message.Id, msgPart.Body.AttachmentId).ExecuteAsync();

                var fileBytes = msgPartBody.Data.ToBytes();
                _attachService.SaveAttachmentFile(msgFolderPath, msgPart.Filename, fileBytes);

                fileNames.Add(msgPart.Filename);
            }

            return fileNames;
        }
    }
}