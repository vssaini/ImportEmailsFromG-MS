using EmailsImporter.Models;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;
using Message = Google.Apis.Gmail.v1.Data.Message;

namespace EmailsImporter.Services.Google
{
    public class GoogleMailService
    {
        private readonly Controller _controller;
        private GmailHelper _gmailHelper;
        private readonly AttachmentService _attachService;

        public GoogleMailService(Controller controller)
        {
            _controller = controller;
            _attachService = new AttachmentService();
        }

        /// <summary>
        /// Get AuthResult which contains the user's credentials if it was loaded successfully from the store. Otherwise it contains the redirect URI for the authorization server.
        /// </summary>
        /// <param name="controller">The current controller.</param>
        /// <returns>Instance of AuthResult.</returns>
        public async Task<AuthResult> GetAuthResultAsync(Controller controller)
        {
            var dataStore = new EfDataStore();
            var appFlowMetaData = new GoogleAppFlowMetaData(dataStore);
            var authCodeMvcApp = new AuthorizationCodeMvcApp(controller, appFlowMetaData);

            var authResult = await authCodeMvcApp.AuthorizeAsync(new CancellationToken()).ConfigureAwait(false);
            return authResult;
        }

        public async Task<List<Email>> GetAllEmailsAsync(string emailAddress)
        {
            var emails = new List<Email>();

            var messages = await GetMessagesAsync(emailAddress);
            if (messages.Count <= 0) return emails;

            var userAttachDirectoryPath = _attachService.CreateUserAttachDirectory(emailAddress, true);

            foreach (var msg in messages)
            {
                Email email = null;
                try
                {
                    email = await GetEmailAsync(emailAddress, msg.Id, userAttachDirectoryPath);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error: " + e);
                }

                if (email == null) continue;
                emails.Add(email);
            }

            return emails;
        }

        private async Task<IList<Message>> GetMessagesAsync(string emailAddress)
        {
            var gmailService = await GetGmailServiceAsync(_controller);
            _gmailHelper = new GmailHelper(gmailService, _attachService);

            var messages = _gmailHelper.GetUnReadMessages(emailAddress, false);
            return messages;
        }

        private async Task<GmailService> GetGmailServiceAsync(Controller controller)
        {
            var authResult = await GetAuthResultAsync(controller).ConfigureAwait(false);

            var gmailService = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = authResult.Credential,
                ApplicationName = "Union Manager",
            });

            return gmailService;
        }

        private async Task<Email> GetEmailAsync(string emailAddress, string messageId, string userAttachDirectoryPath)
        {
            Message message = await _gmailHelper.GetMessageWithPayloadAsync(emailAddress, messageId);
            if (message == null) return null;

            var msgTxt = GetMessageText(message);
            if (string.IsNullOrWhiteSpace(msgTxt)) return null;

            var msgHeader = GetMessageHeader(message);
            var attachments = await _gmailHelper.GetMessageAttachmentsAsync(emailAddress, message, userAttachDirectoryPath);

            return new Email
            {
                From = GetTrimmedEmailAddress(msgHeader.FromAddress),
                Subject = msgHeader.Subject,
                Body = msgTxt,
                Date = msgHeader.Date.ToString(Constants.DateFormat),
                Attachments = attachments
            };
        }

        private MessageHeader GetMessageHeader(Message message)
        {
            var msgHeader = new MessageHeader();

            foreach (var msgPartHeader in message.Payload.Headers)
            {
                if (msgPartHeader.Name == "From")
                {
                    msgHeader.FromAddress = msgPartHeader.Value;
                }
                else if (msgPartHeader.Name == "Date")
                {
                    if (DateTime.TryParse(msgPartHeader.Value, out var date))
                        msgHeader.Date = date;
                }
                else if (msgPartHeader.Name == "Subject")
                {
                    msgHeader.Subject = msgPartHeader.Value;
                }
            }

            return msgHeader;
        }

        private string GetMessageText(Message message)
        {
            string msgBodyData;
            if (message.Payload.Parts == null && message.Payload.Body != null)
            {
                msgBodyData = message.Payload.Body.Data;
            }
            else
            {
                msgBodyData = _gmailHelper.GetMessageBodyData(message.Payload.Parts);
            }

            return msgBodyData.ToUTF8Format();
        }

        private string GetTrimmedEmailAddress(string gEmailAddress)
        {
            string[] addressParts = gEmailAddress.Split(' ');
            string emailAddress = addressParts[addressParts.Length - 1];

            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                emailAddress = emailAddress.Replace("<", string.Empty);
                emailAddress = emailAddress.Replace(">", string.Empty);
            }

            return emailAddress;
        }
    }
}