using System.Web.Mvc;

namespace EmailsImporter.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}