using System.Linq;
using System.Web.Http;
using Core.DomainServices.Organizations;
using Core.DomainServices.Repositories.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Properties;

namespace Presentation.Web.Controllers.API.V1
{
    [AllowAnonymous]
    [InternalApi]
    public class HealthCheckController : ApiController
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IStsOrganizationService _stsOrganizationService;
        private static readonly string DeploymentVersion = Settings.Default.DeploymentVersion;

        //TODO: Revert changes in this file
        public HealthCheckController(IOrganizationRepository organizationRepository, IStsOrganizationService stsOrganizationService)
        {
            _organizationRepository = organizationRepository;
            _stsOrganizationService = stsOrganizationService;
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            var organization = _organizationRepository.GetAll().First();
            organization.Cvr = "58271713"; //ballerup
            var result = _stsOrganizationService.ResolveStsOrganizationUuid(organization);
            return Ok(DeploymentVersion);
        }
    }
}