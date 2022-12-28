using EmailsImporter.Models;
using EmailsImporter.Services.Microsoft;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EmailsImporter.Controllers
{
    public class MsAuthController : Controller
    {
        private readonly MsAuthService _msAuthService;

        public MsAuthController()
        {
            _msAuthService = new MsAuthService();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            // STEP 2/3 - Get authorization code after redirection from MS Identity Server

            var errorDescription = Request.QueryString["error_description"];
            if (!string.IsNullOrWhiteSpace(errorDescription))
                throw new Exception(errorDescription);

            var state = Request.QueryString["state"];
            if (state != Constants.State)
                throw new Exception("Invalid state.");

            var authCode = Request.QueryString["code"];
            if (string.IsNullOrWhiteSpace(authCode))
                throw new NotFoundException("Authorization code is either null or empty.");

            var response = await _msAuthService.GetTokenAsync(authCode);
            await _msAuthService.SaveTokenAsync(response);

            return new RedirectResult(Url.Content("~/"));
        }
    }
}