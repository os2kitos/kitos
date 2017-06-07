using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Presentation.Web.Controllers.API
{
    public class SSOConfigController : BaseApiController
    {
        [AllowAnonymous]
        public HttpResponseMessage Get()
        {
            var SSOGateway = System.Web.Configuration.WebConfigurationManager.AppSettings["SSOGateway"];
            var SSOAudience = System.Web.Configuration.WebConfigurationManager.AppSettings["SSOAudience"];

            var ssoConfig = new { SSOGateway = SSOGateway, SSOAudience = SSOAudience };
            return Ok(ssoConfig);
        }
    }
}