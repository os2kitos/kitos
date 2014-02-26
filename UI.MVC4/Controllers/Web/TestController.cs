using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.MVC4.Controllers.Web
{
    public class TestController : Controller
    {
        //
        // GET: /Test/

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View();
        }

    }
}
