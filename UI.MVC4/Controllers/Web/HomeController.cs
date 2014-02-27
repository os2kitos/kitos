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
            return View();
        }
    }
}
