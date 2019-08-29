using System.Net;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Properties;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [AllowAnonymous]
    [PublicApi]
    public class HealthCheckController : ApiController
    {
        private static readonly string DeploymentVersion = Settings.Default.DeploymentVersion;

        [HttpGet]
        public IHttpActionResult Get()
        {
            return Ok(DeploymentVersion);
        }
    }
}