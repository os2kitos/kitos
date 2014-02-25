using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UI.MVC4.Controllers.Web
{
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
        [Authorize(Roles = "Admin")]
        public ActionResult AddMunicipality()
        {
            return View();
        }

    }
}
