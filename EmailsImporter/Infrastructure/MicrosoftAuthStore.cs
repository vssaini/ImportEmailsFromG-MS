using System;
using System.ComponentModel.DataAnnotations;

namespace EmailsImporter.Infrastructure
{
    public class MicrosoftAuthStore
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }

        public string MsAccessToken { get; set; }
        public string MsRefreshToken { get; set; }
        public DateTime MsAccessTokenExpiresAt { get; set; }
    }
}