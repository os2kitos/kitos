using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainServices;
using Ninject;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        [Authorize(Roles = "User")]
        public ActionResult UserPage()
        {
            ViewBag.Message = "Hello user: " + User.Identity.Name;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminPage()
        {
            ViewBag.Message = "The truth is out there, Scully!";

            return View();
        }
    }
}
