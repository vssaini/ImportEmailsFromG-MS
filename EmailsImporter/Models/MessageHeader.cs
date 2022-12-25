using System;

namespace EmailsImporter.Models
{
    public class MessageHeader
    {
        public string FromAddress { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; }
    }
}