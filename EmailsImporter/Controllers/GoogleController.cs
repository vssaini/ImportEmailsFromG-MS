using EmailsImporter.Models;
using EmailsImporter.Services.Google;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EmailsImporter.Controllers
{
    public class GoogleController : BaseController
    {
        private readonly GoogleMailService _googleMailService;
        public GoogleController()
        {
            _googleMailService = new GoogleMailService(this);
        }

        public async Task<ActionResult> GetEmails()
        {
            // Ref - https://www.sparkhound.com/blog/google-oauth-integration-using-an-mvc-asp-net-app-1

            var result = new Result<string> { Type = ResponseType.Ok };

            try
            {
                var authResult = await _googleMailService.GetAuthResultAsync(this).ConfigureAwait(false);
                if (authResult.Credential == null)
                {
                    result.RedirectUri = authResult.RedirectUri;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var emails = await _googleMailService.GetAllEmailsAsync("vssaini.dev@gmail.com");
                result.Data = RenderRazorViewToString("~/Views/Home/_InboxGmail.cshtml", emails);
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