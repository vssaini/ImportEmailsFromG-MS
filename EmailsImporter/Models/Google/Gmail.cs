using System.Collections.Generic;

namespace EmailsImporter.Models.Google
{
    public class Gmail
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
        public string Date { get; set; }
        public List<string> Attachments { get; set; }
    }
}