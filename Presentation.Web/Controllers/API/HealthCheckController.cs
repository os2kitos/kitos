using System.Web.Http;
using Presentation.Web.Properties;

namespace Presentation.Web.Controllers.API
{
    [AllowAnonymous]
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