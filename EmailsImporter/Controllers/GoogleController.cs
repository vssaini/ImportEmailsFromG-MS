using EmailsImporter.Models;
using EmailsImporter.Services.Google;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EmailsImporter.Controllers
{
    public class GoogleController : Controller
    {
        private readonly GoogleMailService _googleMailService;
        public GoogleController()
        {
            _googleMailService = new GoogleMailService(this);
        }

        public async Task<ActionResult> GetEmails()
        {
            // Ref - https://www.sparkhound.com/blog/google-oauth-integration-using-an-mvc-asp-net-app-1

            var result = new Result<List<Gmail>> { Type = ResponseType.Ok };

            try
            {
                var authResult = await _googleMailService.GetAuthResultAsync(this).ConfigureAwait(false);
                if (authResult.Credential == null)
                {
                    result.RedirectUri = authResult.RedirectUri;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                var emails = await _googleMailService.GetAllEmailsAsync("vssaini.dev@gmail.com");
                result.Data = emails;
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