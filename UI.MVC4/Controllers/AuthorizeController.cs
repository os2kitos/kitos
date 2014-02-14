using System;
using System.Web.Mvc;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class AuthorizeController : Controller
    {
        //TODO: where do these go?
        private const int ResetRequestTTL = 12;
        private const string FromAddress = "kitos@it-minds.dk";

        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetRequestRepository _passwordResetRepository;
        private readonly IMailClient _mailClient;

        public AuthorizeController(IUserRepository userRepository, IPasswordResetRequestRepository passwordResetRepository, IMailClient mailClient)
        {
            _userRepository = userRepository;
            _passwordResetRepository = passwordResetRepository;
            _mailClient = mailClient;
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

                return RedirectToAction("Index", "Home");
            }

            // If we got this far, we couldn't authenticate, redisplay form
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
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
            User user;

            try
            {
                user = _userRepository.GetByEmail(model.Email);
                if (user == null)
                    throw new Exception();
            }
            catch
            {
                //Something went bad when trying to find a user. Report the error

                //TODO: perhaps we shouldn't report that the user wasn't found?
                //TODO: Right now we are leaking information about which users exist
                return RedirectToAction("ForgotPassword", new {userNotFound = true});
            }

            try
            {
                var now = DateTime.Now;

                //TODO: BETTER HASHING???
                var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(now + user.Email, "MD5");
                var passwordReset = new PasswordResetRequest
                {
                    Id = 0,
                    Hash = hash,
                    Time = now,
                    User = user
                };

                _passwordResetRepository.Create(passwordReset);

                var resetLink = "http://kitos.dk/Authorize/ResetPassword?Hash=" + hash;
                var mailContent = "<a href='" + resetLink + "'>Klik her for at nulstille passwordet for din KITOS bruger</a>. Linket udløber om " + ResetRequestTTL + " timer.";

                _mailClient.Send(FromAddress, user.Email, "Nulstilning af dit KITOS password", mailContent);
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

            try
            {
                var passwordReset = _passwordResetRepository.Get(hash);

                var timespan = DateTime.Now - passwordReset.Time;
                if (timespan.TotalHours < ResetRequestTTL)
                {
                    //successfully found valid reset request 

                    //TODO: should we use AutoMapper here??
                    resetModel = new ResetPasswordViewModel {RequestHash = hash, Email = passwordReset.User.Email};
                }
                else
                {
                    //the reset request is too old, delete it
                    _passwordResetRepository.Delete(passwordReset);
                }

            }
            catch
            {
                //TODO: leaving this empty is probably not a good idea??
            }

            return View(resetModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel resetModel)
        {
            try
            {
                //does the reset request still exist?
                var passwordReset = _passwordResetRepository.Get(resetModel.RequestHash);

                if(!ModelState.IsValid)
                    throw new Exception();

                //Everything is cool, set new password

                var user = passwordReset.User;

                //TODO: enforce password rules??
                user.Password = resetModel.Password;

                _userRepository.Update(user);

                //delete the (now) used reset request
                _passwordResetRepository.Delete(passwordReset);

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
