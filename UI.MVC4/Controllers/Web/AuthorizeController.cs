using System;
using System.Web.Mvc;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.Web
{
    public class AuthorizeController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;

        public AuthorizeController(IUserRepository userRepository, IUserService userService)
        {
            _userRepository = userRepository;
            _userService = userService;
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel loginModel, string returnUrl)
        {
            //TODO: exception handling??
            if (ModelState.IsValid && Membership.ValidateUser(loginModel.Email, loginModel.Password))
            {
                FormsAuthentication.SetAuthCookie(loginModel.Email, loginModel.RememberMe);

                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "OldHome");
            }

            // If we got this far, we couldn't authenticate, redisplay form
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "OldHome");
        }

        public ActionResult ForgotPassword(bool userNotFound = false)
        {
            if (userNotFound) ViewBag.Message = "Ingen bruger er tilknyttet den email-addresse. Prøv igen.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                var user = _userRepository.GetByEmail(model.Email);
                _userService.IssuePasswordReset(user, null, null);
            }
            catch (ArgumentNullException)
            {
                //Something went bad when trying to find a user. Report the error

                //TODO: perhaps we shouldn't report that the user wasn't found?
                //TODO: Right now we are leaking information about which users exist
                return RedirectToAction("ForgotPassword", new {userNotFound = true});
            }
            catch
            {
                //Something went bad when trying to create the password request/sent the mail
                return RedirectToAction("EmailSent", new {success = false});
            }

            //Everything went fine!
            return RedirectToAction("EmailSent");
        }

        public ActionResult EmailSent(bool success = true)
        {
            ViewBag.Success = success;
            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword(string hash)
        {
            
            ResetPasswordViewModel resetModel = null;

            var resetRequest = _userService.GetPasswordReset(hash);
            if (resetRequest != null)
            {
                resetModel = new ResetPasswordViewModel {RequestHash = hash, Email = resetRequest.User.Email};
            } 
            
            return View(resetModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel resetModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(resetModel);

                var resetRequest = _userService.GetPasswordReset(resetModel.RequestHash);
                _userService.ResetPassword(resetRequest, resetModel.Password);
            }
            catch
            {
                return RedirectToAction("ResetDone", new {success = false});
            }

            return RedirectToAction("ResetDone");
        }

        public ActionResult ResetDone(bool success = true)
        {
            return View();
        }
    }
}
