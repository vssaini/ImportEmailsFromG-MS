using EmailsImporter.Models;
using EmailsImporter.Services.Google;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EmailsImporter.Controllers
{
    public class HomeController : BaseController
    {
        private readonly GoogleMailService _googleMailService;
        public HomeController()
        {
            _googleMailService = new GoogleMailService(this);
        }

        public async Task<ActionResult> Index()
        {
            var authStatus = new AuthStatus();

            try
            {
                var authResult = await _googleMailService.GetAuthResultAsync(this).ConfigureAwait(false);
                authStatus.IsGoogleAppAuthorized = !string.IsNullOrWhiteSpace(authResult.Credential?.Token.AccessToken);
            }
            catch (Exception)
            {
                authStatus.IsGoogleAppAuthorized = false;
                authStatus.IsMicrosoftAppAuthorized = false;
            }

            return View(authStatus);
        }

        public async Task<ActionResult> AuthorizeGmailApp()
        {
            var authResult = await _googleMailService.GetAuthResultAsync(this).ConfigureAwait(false);
            return Redirect(authResult.RedirectUri);
        }
    }
}