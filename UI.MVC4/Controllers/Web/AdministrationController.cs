using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.MVC4.Controllers.Web
{
    [Authorize(Roles = "GlobalAdmin")]
    public class AdministrationController : Controller
    {
        //
        // GET: /AdministrationController/

        public ActionResult Index()
        {
            return View();
        }


        //
        // GET: /AdministrationController/
        public ActionResult AddMunicipality()
        {
            return View();
        }

        public ActionResult AddGlobalAdmin()
        {
            return View();
        }

    }
}
