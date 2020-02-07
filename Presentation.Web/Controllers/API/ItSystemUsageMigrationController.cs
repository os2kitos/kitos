using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Authorization;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.ItSystemUsageMigration;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
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
        public HttpResponseMessage GetMigration([FromUri]int usageId, [FromUri]int toSystemId)
        {
            var res = _itSystemUsageMigrationService.GetSystemUsageMigration(usageId, toSystemId);
            if (res.Ok)
            {
                return Ok(Map(res.Value));
            }

            return FromOperationFailure(res.Error);
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.NoContent,"Migration completed")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage ExecuteMigration([FromUri]int usageId, [FromUri]int toSystemId)
        {
            var result = _itSystemUsageMigrationService.ExecuteSystemUsageMigration(usageId, toSystemId);
            if (result.Ok)
            {
                return NoContent();
            }

            return FromOperationFailure(result.Error);
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemSimpleDTO>>))]
        public HttpResponseMessage GetUnusedItSystemsBySearchAndOrganization(
            [FromUri]int organizationId,
            [FromUri]string nameContent,
            [FromUri]int numberOfItSystems,
            [FromUri]bool getPublicFromOtherOrganizations)
        {
            if (GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.Public)
            {
                return Forbidden();
            }
            if (string.IsNullOrWhiteSpace(nameContent))
            {
                return Ok(Enumerable.Empty<NamedEntityDTO>().ToList());
            }
            if (numberOfItSystems < 1 || numberOfItSystems > 25)
            {
                return BadRequest($"{nameof(numberOfItSystems)} must satisfy constraint: 1 <= n <= 25");
            }

            var result = _itSystemUsageMigrationService.GetUnusedItSystemsByOrganization(organizationId, nameContent, numberOfItSystems, getPublicFromOtherOrganizations);

            if (result.Ok)
            {
                var unusedItSystems = result.Value.Select(DTOMappingExtensions.MapToNamedEntityDTO).ToList();
                return Ok(unusedItSystems);
            }

            return FromOperationFailure(result.Error);
        }

        private static ItSystemUsageMigrationDTO Map(ItSystemUsageMigration input)
        {
            return new ItSystemUsageMigrationDTO
            {
                TargetUsage = new NamedEntityDTO(input.SystemUsage.Id, input.FromItSystem.Name),
                FromSystem = input.FromItSystem.MapToNamedEntityDTO(),
                ToSystem = input.ToItSystem.MapToNamedEntityDTO(),
                AffectedItProjects = input.AffectedProjects.MapToNamedEntityDTOs().ToList(),
                AffectedContracts = input.AffectedContracts.MapToNamedEntityDTOs().ToList(),
                AffectedRelations = input.AffectedSystemRelations.Select(Map).ToList()
            };
        }

        private static RelationMigrationDTO Map(SystemRelation input)
        {
            return new RelationMigrationDTO
            {
                SourceSystem = input.FromSystemUsage.MapToNamedEntityDTO(),
                TargetSystem = input.ToSystemUsage.MapToNamedEntityDTO(),
                Contract = input.AssociatedContract?.MapToNamedEntityDTO(),
                Interface = input.RelationInterface?.MapToNamedEntityDTO()
            };
        }
    }
}