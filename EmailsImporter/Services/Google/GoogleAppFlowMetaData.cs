using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Mvc;

namespace EmailsImporter.Services.Google
{
    /// <summary>
    /// This class maintains a reference to Google OAuth 2.0 authorization code flow that manages and persists end-user credentials.
    /// </summary>
    public class GoogleAppFlowMetaData : FlowMetadata
    {
        private readonly string[] _scopes = { GmailService.Scope.GmailReadonly };

        public override IAuthorizationCodeFlow Flow { get; }
        public override string AuthCallback => "/AuthCallback/IndexAsync";

        public GoogleAppFlowMetaData(IDataStore dataStore)
        {
            var gClientId = WebConfigurationManager.AppSettings["GoogleClientID"];
            var gClientSecret = WebConfigurationManager.AppSettings["GoogleClientSecret"];

            var flowInitializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = gClientId,
                    ClientSecret = gClientSecret
                },
                Scopes = _scopes,
                DataStore = dataStore
            };
            Flow = new ForceOfflineGoogleAuthorizationCodeFlow(flowInitializer);
        }

        public override string GetUserId(Controller controller)
        {
            // In this sample we use the session to store the user identifiers.
            // That's not the best practice, because you should have a logic to identify
            // a user. You might want to use "OpenID Connect".
            // You can read more about the protocol in the following link:
            // https://developers.google.com/accounts/docs/OAuth2Login.

            var user = controller.Session["user"];
            if (user == null)
            {
                user = ConfigurationManager.AppSettings["GoogleUserGuid"];
                controller.Session["user"] = user;
            }

            return user.ToString();
        }
    }
}