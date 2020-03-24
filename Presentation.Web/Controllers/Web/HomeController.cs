using System.Web.Mvc;
using Core.ApplicationServices.SSO.Model;
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

            return View();
        }

        public ActionResult SsoError(SsoErrorCode? ssoErrorCode)
        {
            if (ssoErrorCode.HasValue)
            {
                TempData[FeatureToggleKey] = TemporaryFeature.Sso;
                TempData[SsoErrorKey] = ssoErrorCode;
            }
            return RedirectToAction(nameof(Index));
        }

        public ActionResult WithFeature(TemporaryFeature? feature)
        {
            if (feature.HasValue)
            {
                TempData[FeatureToggleKey] = feature;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
