using System.Web.Mvc;
using Core.ApplicationServices.SSO.Model;
using Core.DomainModel.Result;
using Presentation.Web.Models.FeatureToggle;
using Presentation.Web.Properties;

namespace Presentation.Web.Controllers.Web
{
    public class HomeController : Controller
    {
        private const string SsoErrorKey = "SSO_ERROR";
        private const string FeatureToggleKey = "FEATURE_TOGGLE";

        public ActionResult Index()
        {
            ViewBag.Environment = Settings.Default.Environment;
            AppendSsoError();
            AppendFeatureToggles();
            return View();
        }

        private void AppendSsoError()
        {
            if (TempData[SsoErrorKey] != null)
            {
                var ssoError = (SsoErrorCode)TempData[SsoErrorKey];
                ViewBag.SsoErrorCode = ssoError;
            }
        }

        private void AppendFeatureToggles()
        {
            if (TempData[FeatureToggleKey] != null)
            {
                var feature = (TemporaryFeature)TempData[FeatureToggleKey];
                ViewBag.FeatureToggle = feature;
            }
        }

        public ActionResult SsoError(SsoErrorCode? ssoErrorCode)
        {
            if (ssoErrorCode.HasValue)
            {
                TempData[FeatureToggleKey] = TemporaryFeature.Sso;
                TempData[SsoErrorKey] = ssoErrorCode;
                TempData.Keep();
            }
            return RedirectToAction(nameof(Index));
        }

        public ActionResult WithFeature(TemporaryFeature? feature)
        {
            if (feature.HasValue)
            {
                TempData[FeatureToggleKey] = feature;
                TempData.Keep();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
