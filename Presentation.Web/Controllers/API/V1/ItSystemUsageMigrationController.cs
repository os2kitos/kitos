using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Controllers.API.V1.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.ItSystemUsageMigration;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
{
    [InternalApi]
    [RoutePrefix("api/v1/ItSystemUsageMigration")]
    public class ItSystemUsageMigrationController : BaseApiController
    {
        private readonly IItSystemUsageMigrationService _itSystemUsageMigrationService;

        public ItSystemUsageMigrationController(IItSystemUsageMigrationService itSystemUsageMigrationService)
        {
            _itSystemUsageMigrationService = itSystemUsageMigrationService;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<ItSystemUsageMigrationDTO>))]
        public HttpResponseMessage GetMigration([FromUri] int usageId, [FromUri] int toSystemId)
        {
            return _itSystemUsageMigrationService.GetSystemUsageMigration(usageId, toSystemId)
                .Select(Map)
                .Match(Ok, FromOperationError);
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.NoContent, "Migration completed")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage ExecuteMigration([FromUri] int usageId, [FromUri] int toSystemId)
        {
            return _itSystemUsageMigrationService.ExecuteSystemUsageMigration(usageId, toSystemId)
                .Match(_ => NoContent(), FromOperationError);
        }

        [HttpGet]
        [Route("Accessibility")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<ItSystemUsageMigrationAccessDTO>))]
        public HttpResponseMessage GetAccessibilityLevel()
        {
            return Ok(new ItSystemUsageMigrationAccessDTO
            {
                CanExecuteMigration = _itSystemUsageMigrationService.CanExecuteMigration()
            });
        }

        [HttpGet]
        [Route("UnusedItSystems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<NamedEntityWithEnabledStatusDTO>>))]
        public HttpResponseMessage GetUnusedItSystemsBySearchAndOrganization(
            [FromUri] int organizationId,
            [FromUri] string nameContent,
            [FromUri] int numberOfItSystems,
            [FromUri] bool getPublicFromOtherOrganizations)
        {
            if (string.IsNullOrWhiteSpace(nameContent))
            {
                return Ok(Enumerable.Empty<NamedEntityDTO>().ToList());
            }

            return _itSystemUsageMigrationService.GetUnusedItSystemsByOrganizationByName(organizationId, nameContent, numberOfItSystems, getPublicFromOtherOrganizations)
                .Select(x => x.Select(DTOMappingExtensions.MapToNamedEntityWithEnabledStatusDTO).ToList())
                .Match(Ok, FromOperationError);
        }

        private static ItSystemUsageMigrationDTO Map(ItSystemUsageMigration input)
        {
            return new ItSystemUsageMigrationDTO
            {
                TargetUsage = input.SystemUsage.MapToNamedEntityWithEnabledStatusDTO(),
                FromSystem = input.FromItSystem.MapToNamedEntityWithEnabledStatusDTO(),
                ToSystem = input.ToItSystem.MapToNamedEntityWithEnabledStatusDTO(),
                AffectedContracts = input.AffectedContracts.MapToNamedEntityDTOs().ToList(),
                AffectedRelations = input.AffectedSystemRelations.Select(Map).ToList(),
                AffectedDataProcessingRegistrations = input.AffectedDataProcessingRegistrations.MapToNamedEntityDTOs().ToList()
            };
        }

        private static RelationMigrationDTO Map(SystemRelation input)
        {
            return new RelationMigrationDTO
            {
                ToSystemUsage = input.ToSystemUsage.MapToNamedEntityWithEnabledStatusDTO(),
                FromSystemUsage = input.FromSystemUsage.MapToNamedEntityWithEnabledStatusDTO(),
                Description = input.Description,
                Contract = input.AssociatedContract?.MapToNamedEntityDTO(),
                FrequencyType = input.UsageFrequency?.MapToNamedEntityDTO(),
                Interface = input.RelationInterface?.MapToNamedEntityDTO()
            };
        }
    }
}