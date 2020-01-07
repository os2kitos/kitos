using System.Net;
using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [DeprecatedApi]
    public class SSOConfigController : BaseApiController
    {
        [AllowAnonymous]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<SSOConfigDTO>))]
        public HttpResponseMessage Get()
        {
            var SSOGateway = System.Web.Configuration.WebConfigurationManager.AppSettings["SSOGateway"];
            var SSOAudience = System.Web.Configuration.WebConfigurationManager.AppSettings["SSOAudience"];

            var ssoConfig = new SSOConfigDTO{ SSOGateway = SSOGateway, SSOAudience = SSOAudience };
            return Ok(ssoConfig);
        }
    }
}