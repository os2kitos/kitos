using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.Web
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("/ui");
        }
    }
}
