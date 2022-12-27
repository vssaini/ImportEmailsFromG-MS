using System.ComponentModel.DataAnnotations;

namespace EmailsImporter.Infrastructure
{
    public class GoogleAuthStore
    {
        [Key]
        [MaxLength(100)]
        public string UserId { get; set; }

        [MaxLength]
        public string Value { get; set; }
    }
}