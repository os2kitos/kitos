using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.MVC4.Controllers.Web
{
    [Authorize(Roles = "LocalAdmin,GlobalAdmin")]
    public class LocalConfigController : Controller
    {
        //
        // GET: /Configuration/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Project()
        {
            return View();
        }

    }
}
