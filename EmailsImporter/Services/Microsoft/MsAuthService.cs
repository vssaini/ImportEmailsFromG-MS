using EmailsImporter.Models.Microsoft;
using Microsoft.Graph;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Configuration;
using System.IdentityModel;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace EmailsImporter.Services.Microsoft
{
    public class MsAuthService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _scope;
        private readonly string _redirectUrl;
        private readonly string _tokenUrl;

        public MsAuthService()
        {
            _clientId = ConfigurationManager.AppSettings["MSClientId"];
            _clientSecret = ConfigurationManager.AppSettings["MSClientSecret"];
            _scope = ConfigurationManager.AppSettings["MSAppScopes"];
            _redirectUrl = ConfigurationManager.AppSettings["MSAppRedirectUri"];

            _tokenUrl = $"{ConfigurationManager.AppSettings["MSIdentityUrl"]}/token";
        }

        public async Task<MSTokenResponse> GetTokenAsync(string authCode)
        {
            var request = GetRestRequestForAccess(authCode);

            // Ref - https://restsharp.dev/v107/#restclient-and-options
            // Do not instantiate RestClient for each HTTP call. 
            var restClient = new RestClient(_tokenUrl);
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

            request.AddParameter("client_id", _clientId);
            request.AddParameter("client_secret", _clientSecret);
            request.AddParameter("scope", _scope);
            request.AddParameter("redirect_uri", _redirectUrl);

            return request;
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

            var restClient = new RestClient(_tokenUrl);
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

        private async Task SaveTokenAsync(MSTokenResponse tokenResponse)
        {
            var msMailService = new MsMailService();
            await msMailService.SaveTokenAsync(tokenResponse);
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
