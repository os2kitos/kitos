using Presentation.Web.Infrastructure.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Presentation.Web.Controllers.API.V1
{
    [AllowAnonymous]
    [InternalApi]
    [Route("api/healthcheck")]
    public class HealthCheckController : ControllerBase
    {
        private readonly string _deploymentVersion;

        public HealthCheckController(IConfiguration configuration)
        {
            _deploymentVersion = configuration["AppSettings:DeploymentVersion"] ?? "unknown";
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_deploymentVersion);
        }
    }
}



