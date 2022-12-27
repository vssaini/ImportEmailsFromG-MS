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
        private readonly MsMailService _msMailService;

        public MicrosoftController()
        {
            _msMailService = new MsMailService();
        }

        public async Task<ActionResult> GetEmails()
        {
            var result = new Result<string> { Type = ResponseType.Ok };

            try
            {
                var userToken = await _msMailService.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(userToken.AccessToken))
                {
                    result.RedirectUri = _msMailService.GetRedirectUri();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var emails = _msMailService.GetAllMails("vssaini.dev@gmail.com");
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