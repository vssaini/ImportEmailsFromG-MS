using EmailsImporter.Infrastructure;
using EmailsImporter.Models.Microsoft;
using Microsoft.Graph;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IdentityModel;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EmailsImporter.Services.Microsoft
{
    public class MsAuthService
    {
        private readonly MSConfig _msConfig;
        private readonly ApplicationDbContext _db;

        public MsAuthService()
        {
            _msConfig = GetMsConfig();
            _db = new ApplicationDbContext();
        }

        private MSConfig GetMsConfig()
        {
            var msConfig = new MSConfig
            {
                ClientId = ConfigurationManager.AppSettings["MSClientId"],
                ClientSecret = ConfigurationManager.AppSettings["MSClientSecret"],
                RedirectUri = ConfigurationManager.AppSettings["MSAppRedirectUri"],
                IdentityUri = ConfigurationManager.AppSettings["MSIdentityUri"],
                Scopes = ConfigurationManager.AppSettings["MSAppScopes"],
                State = ConfigurationManager.AppSettings["MSState"]
            };

            msConfig.TokenUri = $"{msConfig.IdentityUri}/token";

            return msConfig;
        }

        public string GetRedirectUri()
        {
            // STEP 1/3 - Make request to MS by which we can get code
            var authorizeUrl = $"{_msConfig.IdentityUri}/authorize?client_id={_msConfig.ClientId}" +
                               $"&scope={_msConfig.Scopes}" +
                               $"&state={_msConfig.State}" +
                               $"&redirect_uri={_msConfig.RedirectUri}" +
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

            _db.MicrosoftAuthStore.AddOrUpdate(msAuthStore);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<MSTokenResponse> GetTokenAsync(string authCode)
        {
            var request = GetRestRequestForAccess(authCode);

            // Ref - https://restsharp.dev/v107/#restclient-and-options
            // Do not instantiate RestClient for each HTTP call. 
            var restClient = new RestClient(_msConfig.TokenUri);
            var response = await restClient.PostAsync(request);

            return GetTokenResponse(response);
        }

        private RestRequest GetRestRequestForAccess(string authCode)
        {
            var request = GetRestRequest();

            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("code", authCode);

            return request;
        }

        private RestRequest GetRestRequest()
        {
            var request = new RestRequest { RequestFormat = DataFormat.Json };
            request.AddParameter("client_id", _msConfig.ClientId);
            request.AddParameter("client_secret", _msConfig.ClientSecret);
            request.AddParameter("scope", _msConfig.Scopes);
            request.AddParameter("redirect_uri", _msConfig.RedirectUri);

            return request;
        }

        private static MSTokenResponse GetTokenResponse(RestResponseBase response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var tokenResponse = JsonConvert.DeserializeObject<MSTokenResponse>(response.Content);
                return tokenResponse;
            }

            if (response.ErrorException != null)
                throw response.ErrorException;

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                throw new Exception(response.ErrorMessage);

            if (!string.IsNullOrWhiteSpace(response.Content) && response.Content.Contains("error"))
                throw new BadRequestException(response.Content);

            throw new BadRequestException($"Error during request execution. Status code - {response.StatusCode}");
        }

        public async Task<GraphServiceClient> GetGraphClientByTokenAsync(UserToken cToken)
        {
            if (!cToken.IsExpired)
                return GetGraphClientByToken(cToken.AccessToken);

            var response = await GetRefreshedTokenAsync(cToken.RefreshToken);

            await SaveTokenAsync(response);

            return GetGraphClientByToken(response.AccessToken);
        }

        private async Task<MSTokenResponse> GetRefreshedTokenAsync(string refreshToken)
        {
            var request = GetRestRequestForRefresh(refreshToken);

            var restClient = new RestClient(_msConfig.TokenUri);
            var response = await restClient.PostAsync(request);

            return GetTokenResponse(response);
        }

        private RestRequest GetRestRequestForRefresh(string refreshToken)
        {
            var request = GetRestRequest();

            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", refreshToken);

            return request;
        }

        private GraphServiceClient GetGraphClientByToken(string accessToken)
        {
            var authProvider = new DelegateAuthenticationProvider(requestMessage =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return Task.CompletedTask;
            });

            return new GraphServiceClient(authProvider);
        }
    }
}
