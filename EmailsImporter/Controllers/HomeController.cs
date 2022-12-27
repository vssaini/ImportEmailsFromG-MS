using EmailsImporter.Models;
using EmailsImporter.Services.Google;
using EmailsImporter.Services.Microsoft;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EmailsImporter.Controllers
{
    public class HomeController : BaseController
    {
        private readonly GoogleMailService _googleMailService;
        private readonly MsMailService _msMailService;

        public HomeController()
        {
            _googleMailService = new GoogleMailService(this);
            _msMailService = new MsMailService();
        }

        public async Task<ActionResult> Index()
        {
            var authStatus = new AuthStatus();

            try
            {
                var authResult = await _googleMailService.GetAuthResultAsync(this).ConfigureAwait(false);
                authStatus.IsGoogleAppAuthorized = !string.IsNullOrWhiteSpace(authResult.Credential?.Token.AccessToken);

                var userToken = await _msMailService.GetTokenAsync();
                authStatus.IsMicrosoftAppAuthorized = !string.IsNullOrWhiteSpace(userToken?.AccessToken);
            }
            catch (Exception)
            {
                authStatus.IsGoogleAppAuthorized = false;
                authStatus.IsMicrosoftAppAuthorized = false;

                // TODO: Show error message on front end
            }

            return View(authStatus);
        }

        public async Task<ActionResult> AuthorizeGmailApp()
        {
            var authResult = await _googleMailService.GetAuthResultAsync(this).ConfigureAwait(false);
            return Redirect(authResult.RedirectUri);
        }

        public ActionResult AuthorizeMsMailApp()
        {
            var redirectUri = _msMailService.GetRedirectUri();
            return Redirect(redirectUri);
        }
    }
}