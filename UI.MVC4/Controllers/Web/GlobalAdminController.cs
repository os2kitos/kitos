using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.MVC4.Controllers.Web
{
    [Authorize(Roles = "GlobalAdmin")]
    public class GlobalAdminController : Controller
    {
        //
        // GET: /GlobalAdminController/

        public ActionResult Index()
        {
            return View();
        }


        //
        // GET: /GlobalAdminController/
        public ActionResult AddMunicipality()
        {
            return View();
        }

        public ActionResult AddLocalAdmin()
        {
            return View();
        }

        public ActionResult AddGlobalAdmin()
        {
            return View();
        }

    }
}
