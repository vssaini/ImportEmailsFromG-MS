using System;

namespace EmailsImporter.Models.Microsoft
{
    public class UserToken
    {
        public Guid UserId { get; set; }

        private DateTime? _date;
        public DateTime? TokenExpireAt
        {
            get => _date;
            set
            {
                _date = value;

                IsExpired = value <= DateTime.UtcNow;
            }
        }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public bool IsExpired { get; set; }
    }
}