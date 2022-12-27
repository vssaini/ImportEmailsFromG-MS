using EmailsImporter.Models;
using EmailsImporter.Services.Microsoft;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EmailsImporter.Controllers
{
    public class MsAuthController : Controller
    {
        private readonly string _state;
        private readonly MsAuthService _msAuthService;
        private readonly MsMailService _msMailService;

        public MsAuthController()
        {
            _state = ConfigurationManager.AppSettings["MSState"];

            _msAuthService = new MsAuthService();
            _msMailService = new MsMailService();
        }

        public async Task<ActionResult> Index()
        {
            // 2. Get authorization code after redirection from MS Identity Server

            var errorDescription = Request.QueryString["error_description"];
            if (!string.IsNullOrWhiteSpace(errorDescription))
                throw new Exception(errorDescription);

            var state = Request.QueryString["state"];
            if (state != _state)
                throw new Exception("Invalid state.");

            var authCode = Request.QueryString["code"];
            if (string.IsNullOrWhiteSpace(authCode))
                throw new NotFoundException("Authorization code is either null or empty.");

            var response = await _msAuthService.GetTokenAsync(authCode);
            await _msMailService.SaveTokenAsync(response);

            return new RedirectResult(Url.Content("~/"));
        }
    }
}