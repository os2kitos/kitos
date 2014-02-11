using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
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

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page!";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel loginModel, string returnUrl)
        {
            if (ModelState.IsValid && Membership.ValidateUser(loginModel.Username, loginModel.Password))
            {
                FormsAuthentication.SetAuthCookie(loginModel.Username, loginModel.RememberMe);

                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            // If we got this far, we couldn't authenticate, redisplay form
            return View(loginModel);
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
