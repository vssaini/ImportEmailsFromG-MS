using System.Configuration;
using System.IO;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace EmailsImporter.Services
{
    public class AttachmentService
    {
        /// <summary>
        /// Create directory as per email address in respective attachments folder path.
        /// </summary>
        /// <returns>Full path of user directory.</returns>
        public string CreateUserAttachDirectory(string emailAddress, bool isGoogleDirectory)
        {
            var settingName = isGoogleDirectory ? "GoogleAttachmentsFolder" : "MicrosoftAttachmentsFolder";

            var attachmentsFolder = ConfigurationManager.AppSettings[settingName];
            var attachDirectoryPath = System.Web.HttpContext.Current.Server.MapPath(attachmentsFolder);

            var userFolderPath = Path.Combine(attachDirectoryPath, emailAddress);
            if (!Directory.Exists(userFolderPath))
                Directory.CreateDirectory(userFolderPath);

            return userFolderPath;
        }

        /// <summary>
        /// Create message folder if not exists.
        /// </summary>
        /// <returns>Full path of message directory.</returns>
        public string CreateMessageFolderIfNotExist(string directoryPath, string msgId)
        {
            var msgFolderPath = Path.Combine(directoryPath, msgId);
            if (!Directory.Exists(msgFolderPath))
                Directory.CreateDirectory(msgFolderPath);

            return msgFolderPath;
        }

        public void SaveAttachmentFile(string msgFolderPath, string attachmentName, byte[] fileBytes)
        {
            var filePath = Path.Combine(msgFolderPath, attachmentName);
            File.WriteAllBytes(filePath, fileBytes);
        }
    }
}