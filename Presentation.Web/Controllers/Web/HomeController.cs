using System.Web.Mvc;
using System.Web.SessionState;
using Core.ApplicationServices.SSO.Model;
using Infrastructure.Services.Types;
using Presentation.Web.Models.FeatureToggle;
using Presentation.Web.Properties;

namespace Presentation.Web.Controllers.Web
{
    [SessionState(SessionStateBehavior.Required)]
    public class HomeController : Controller
    {
        private const string SsoErrorKey = "SSO_ERROR";
        private const string FeatureToggleKey = "FEATURE_TOGGLE";

        public ActionResult Index()
        {
            ViewBag.StylingScheme = Settings.Default.Environment?.ToLowerInvariant().Contains("prod") == true ? "PROD" : "TEST";
            AppendSsoError();
            AppendFeatureToggles();

            return View();
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
