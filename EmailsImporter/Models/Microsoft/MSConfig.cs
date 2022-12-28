namespace EmailsImporter.Models.Microsoft
{
    public class MSConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
        public string IdentityUri { get; set; }
        public string Scopes { get; set; }

        public string TokenUri => $"{IdentityUri}/token";
    }
}