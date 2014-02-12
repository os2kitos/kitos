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
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IMailClient _mailClient;

        public AuthorizeController(IUserRepository userRepository, IPasswordResetRepository passwordResetRepository, IMailClient mailClient)
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
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult ForgotPassword(bool success = false)
        {

            //Did we just reset?
            if (success)
            {
                ViewBag.Message = "En email med et link til at nulstille dit password er blevet afsendt!";
            }


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            //Lookup user either by username or email address
            var user = _userRepository.GetByUsername(model.UserIdentifier) ??
                       _userRepository.GetByEmail(model.UserIdentifier);

            if (user != null)
            {
                var time = DateTime.Now;

                //TODO: BETTER HASHING
                var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(time + user.Email, "MD5");
                var passwordReset = new PasswordReset
                {
                    Id = 0,
                    Hash = hash,
                    Time = time,
                    User = user
                };

                _passwordResetRepository.Create(passwordReset);

                var resetLink = "http://kitos.dk/NotYetImplemented?Hash=" + hash;
                var mailContent = "<a href='" + resetLink + "'>Klik her for at nulstille passwordet for din KITOS bruger</a>";

                //TODO: Exception handling, e.g. if user.Email is wrong formatted or something
                _mailClient.Send("KITOS", user.Email, "Nulstilning af dit KITOS password", mailContent);
            }

            //Even if we didn't find a user and sent an email, pretend that we did
            //otherwise, we'd be leaking information about which users exists
            return RedirectToAction("ForgotPassword", routeValues: new { success = true });
        }

        public ActionResult ResetPassword(string hash, bool retry = false)
        {
            var resetModel = new ResetPasswordViewModel { Retry = retry };

            var passwordReset = _passwordResetRepository.Get(hash);
            if (passwordReset != null)
            {
                var timespan = DateTime.Now - passwordReset.Time;
                if (timespan.TotalHours < 2)
                {
                    //successfully found reset request 
                    //TODO: should we use AutoMapper here?? No, right?
                    resetModel.Hash = hash;
                    resetModel.Email = passwordReset.User.Email;
                }
                else
                {
                    //if the reset request is over 2 hours old, delete it
                    _passwordResetRepository.Delete(passwordReset);
                }
            }

            return View(resetModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel resetModel)
        {
            //does the reset request still exist?
            var passwordReset = _passwordResetRepository.Get(resetModel.Hash);
            if (passwordReset == null || !ModelState.IsValid)
            {
                return RedirectToAction("ResetPassword", routeValues: new {hash = resetModel.Hash, retry = true});
            }

            var user = passwordReset.User;

            //TODO: enforce password rules??
            user.Password = resetModel.Password;

            //TODO: exception handling??
            _userRepository.Update(user);

            //delete the (now) used reset request
            _passwordResetRepository.Delete(passwordReset);

            return RedirectToAction("ResetSuccess");
        }

        public ActionResult ResetSuccess()
        {
            return View();
        }
    }
}
