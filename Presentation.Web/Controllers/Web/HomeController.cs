using System.Web.Mvc;
using System.Web.SessionState;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.SSO.Model;
using Core.DomainServices;
using Infrastructure.Services.Types;
using Ninject;
using Presentation.Web.Models.FeatureToggle;
using Presentation.Web.Properties;

namespace Presentation.Web.Controllers.Web
{
    [SessionState(SessionStateBehavior.Required)]
    public class HomeController : Controller
    {
        [Inject]
        public IUserRepository UserRepository { get; set; }

        private readonly IAuthenticationContext _userContext;
        private const string SsoErrorKey = "SSO_ERROR";
        private const string FeatureToggleKey = "FEATURE_TOGGLE";

        public HomeController(IAuthenticationContext userContext)
        {
            _userContext = userContext;
        }

        public ActionResult Index(bool? postSsoLogin = false)
        {
            ViewBag.StylingScheme = Settings.Default.Environment?.ToLowerInvariant().Contains("prod") == true ? "PROD" : "TEST";
            AppendSsoError();
            AppendFeatureToggles();
            AppendSsoLoginInformation(postSsoLogin == true);

            return View();
        }

        private void AppendSsoLoginInformation(bool loggedInViaSso)
        {
            if (loggedInViaSso && _userContext.Method == AuthenticationMethod.Forms)
            {
                var user = UserRepository.GetById(_userContext.UserId.GetValueOrDefault(-1));
                var userStartPreference = user?.DefaultUserStartPreference;
                if (!string.IsNullOrWhiteSpace(userStartPreference))
                {
                    ViewBag.SsoLoginStartPreference = userStartPreference;
                }
            }
        }

        private void AppendSsoError()
        {
            var ssoError = PopTempVariable<SsoErrorCode>(SsoErrorKey);
            if (ssoError.HasValue)
            {
                ViewBag.SsoErrorCode = ssoError.Value;
            }
        }

        private void AppendFeatureToggles()
        {
            var feature = PopTempVariable<TemporaryFeature>(FeatureToggleKey);
            if (feature.HasValue)
            {
                ViewBag.FeatureToggle = feature.Value;
            }
        }

        public ActionResult SsoError(SsoErrorCode? ssoErrorCode)
        {
            if (ssoErrorCode.HasValue)
            {
                PushTempVariable(ssoErrorCode, SsoErrorKey);
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult WithFeature(TemporaryFeature? feature)
        {
            if (feature.HasValue)
                PushTempVariable(feature, FeatureToggleKey);

            return RedirectToAction(nameof(Index));
        }

        private void PushTempVariable<T>(T ssoErrorCode, string key)
        {
            TempData[key] = ssoErrorCode;
        }

        private Maybe<T> PopTempVariable<T>(string key)
        {
            if (TempData[key] is T value)
            {
                TempData[key] = null;
                return value;
            }

            return Maybe<T>.None;
        }
    }
}
