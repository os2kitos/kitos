using System.Web.Mvc;
using System.Web.SessionState;

namespace Presentation.Web.Controllers.Web
{
    [SessionState(SessionStateBehavior.Required)]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/ui");
        }
    }
}
