using EmailsImporter.Models;
using EmailsImporter.Services.Microsoft;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EmailsImporter.Controllers
{
    public class MicrosoftController : BaseController
    {
        private readonly MsAuthService _msAuthService;
        private readonly MsMailService _msMailService;

        public MicrosoftController()
        {
            _msAuthService = new MsAuthService();
            _msMailService = new MsMailService();
        }

        public async Task<ActionResult> GetEmails()
        {
            var result = new Result<string> { Type = ResponseType.Ok };

            try
            {
                var userToken = await _msAuthService.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(userToken.AccessToken))
                {
                    result.RedirectUri = _msAuthService.GetRedirectUri();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var emails = await _msMailService.GetMessagesAsync(userToken);
                result.Data = RenderRazorViewToString("~/Views/Home/_InboxMicrosoft.cshtml", emails);
            }
            catch (Exception e)
            {
                result.Type = ResponseType.Error;
                result.ErrorMessage = e.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}