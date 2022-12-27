using EmailsImporter.Infrastructure;
using EmailsImporter.Models.Microsoft;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EmailsImporter.Services.Microsoft
{
    public class MsMailService
    {
        private readonly ApplicationDbContext _db;

        public MsMailService()
        {
            _db = new ApplicationDbContext();
        }

        public string GetRedirectUri()
        {
            var clientId = ConfigurationManager.AppSettings["MSClientId"];
            var msIdentityUrl = ConfigurationManager.AppSettings["MSIdentityUrl"];
            var msScopes = ConfigurationManager.AppSettings["MSAppScopes"];
            var msAppRedirectUri = ConfigurationManager.AppSettings["MSAppRedirectUri"];
            var state = ConfigurationManager.AppSettings["MSState"];

            // STEP 1/3 - Make request to MS by which we can get code
            var authorizeUrl = $"{msIdentityUrl}/authorize?client_id={clientId}" +
                               $"&scope={msScopes}" +
                               $"&state={state}" +
                               $"&redirect_uri={msAppRedirectUri}" +
                               "&response_type=code&response_mode=query";
            return authorizeUrl;
        }

        public async Task<UserToken> GetTokenAsync()
        {
            var userId = new Guid(ConfigurationManager.AppSettings["UserGuid"]);
            var userToken = await _db.MicrosoftAuthStore
                .Where(c => c.UserId == userId)
                .Select(c => new UserToken
                {
                    UserId = c.UserId,
                    AccessToken = c.MsAccessToken,
                    RefreshToken = c.MsRefreshToken,
                    TokenExpireAt = c.MsAccessTokenExpiresAt

                }).FirstOrDefaultAsync();

            return userToken;
        }

        public async Task SaveTokenAsync(MSTokenResponse response)
        {
            var userId = new Guid(ConfigurationManager.AppSettings["UserGuid"]);
            var msAuthStore = await _db.MicrosoftAuthStore.FirstOrDefaultAsync(c => c.UserId == userId) ?? new MicrosoftAuthStore();

            msAuthStore.UserId = userId;
            msAuthStore.MsAccessToken = response.AccessToken;
            msAuthStore.MsRefreshToken = response.RefreshToken;
            msAuthStore.MsAccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn);

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
        
        public List<string> GetAllMails(string emailAddress)
        {
            return new List<string>();
        }
    }
}