using System.Web.Mvc;
using System.Web.SessionState;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.SSO.Model;
using Core.DomainServices;
using Presentation.Web.Models.Application.FeatureToggle;
using Presentation.Web.Models.Application.RuntimeEnv;

namespace Presentation.Web.Controllers.Web.Old
{
    [SessionState(SessionStateBehavior.Required)]

    [RoutePrefix("old")]
    public class OldHomeController : Controller
    {
        private readonly IAuthenticationContext _userContext;
        private readonly IUserRepository _userRepository;
        private readonly bool _isProd;
        private const string SsoErrorKey = "SSO_ERROR";
        private const string FeatureToggleKey = "FEATURE_TOGGLE";
        private const string SsoAuthenticationCompletedKey = "SSO_PREFERRED_START";

        public OldHomeController(IAuthenticationContext userContext, IUserRepository userRepository)
        {
            _userContext = userContext;
            _userRepository = userRepository;
            _isProd = KitosEnvironmentConfiguration.FromConfiguration().Environment == KitosEnvironment.Production;
        }

        [Route("")]
        public ActionResult Index()
        {
            ViewBag.StylingScheme = _isProd ? "PROD" : "TEST";
            AppendSsoError();
            AppendFeatureToggles();
            AppendSsoLoginInformation();

            return View();
        }

        private void AppendSsoLoginInformation()
        {
            if (PopTempVariable<bool>(SsoAuthenticationCompletedKey).GetValueOrDefault() && _userContext.Method == AuthenticationMethod.Forms)
            {
                var user = _userRepository.GetById(_userContext.UserId.GetValueOrDefault(-1));
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
