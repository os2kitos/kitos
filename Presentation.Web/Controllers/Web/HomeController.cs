using System.Web.Mvc;
using Presentation.Web.Properties;

namespace Presentation.Web.Controllers.Web
{
    public class HomeController : Controller
    {
        //
        // GET: /Angular/
        public ActionResult Index()
        {
            ViewBag.Environment = Settings.Default.Environment;

            return View();
        }
    }
}
