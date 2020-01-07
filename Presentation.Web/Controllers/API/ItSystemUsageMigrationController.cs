using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
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
    [ControllerEvaluationCompleted]
    [RoutePrefix("api/v1/ItSystemUsageMigration")]
    public class ItSystemUsageMigrationController : BaseApiController
    {
        private readonly IItSystemUsageMigrationService _itSystemUsageMigrationService;

        public ItSystemUsageMigrationController(IItSystemUsageMigrationService itSystemUsageMigrationService, IAuthorizationContext authContext)
            : base(authContext)
        {
            _itSystemUsageMigrationService = itSystemUsageMigrationService;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<ItSystemUsageMigrationDTO>))]
        public HttpResponseMessage GetMigration([FromUri]int usageId, [FromUri]int toSystemId)
        {
            var res = _itSystemUsageMigrationService.GetSystemUsageMigration(usageId, toSystemId);
            switch (res.Status)
            {
                case OperationResult.Ok:
                    return Ok(Map(res.Value));
                case OperationResult.Forbidden:
                    return Forbidden();
                case OperationResult.BadInput:
                    return BadRequest();
                case OperationResult.NotFound:
                    return NotFound();
                default:
                    return CreateResponse(HttpStatusCode.InternalServerError,
                        "An error occured when trying to get migration consequences");
            }
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
            switch (result.Status)
            {
                case OperationResult.Ok:
                    return NoContent();
                case OperationResult.BadInput:
                    return BadRequest();
                case OperationResult.Forbidden:
                    return Forbidden();
                case OperationResult.NotFound:
                    return NotFound();
                default:
                    return CreateResponse(HttpStatusCode.InternalServerError,
                        "An error occured when trying to migrate It System Usage");
            }
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

            switch (result.Status)
            {
                case OperationResult.Ok:
                    var unusedItSystems = result.Value.Select(DTOMappingExtensions.MapToNamedEntityDTO).ToList();
                    return Ok(unusedItSystems);
                case OperationResult.Forbidden:
                    return Forbidden();
                case OperationResult.NotFound:
                    return NotFound();
                default:
                    return CreateResponse(HttpStatusCode.InternalServerError,
                        "An error occured when trying to get Unused It Systems");
            }
        }

        private static ItSystemUsageMigrationDTO Map(ItSystemUsageMigration input)
        {
            return new ItSystemUsageMigrationDTO
            {
                TargetUsage = new NamedEntityDTO { Id = input.SystemUsage.Id, Name = input.SystemUsage.LocalCallName ?? input.FromItSystem.Name },
                FromSystem = input.FromItSystem.MapToNamedEntityDTO(),
                ToSystem = input.ToItSystem.MapToNamedEntityDTO(),
                AffectedItProjects = input.AffectedProjects.Select(DTOMappingExtensions.MapToNamedEntityDTO).ToList(),
                AffectedContracts = input.AffectedContracts.Select(Map).ToList()
            };
        }

        private static ItSystemUsageContractMigrationDTO Map(ItContractMigration input)
        {
            return new ItSystemUsageContractMigrationDTO
            {
                Contract = input.Contract.MapToNamedEntityDTO(),
                SystemAssociatedInContract = input.SystemAssociatedInContract,
                AffectedInterfaceUsages = input.AffectedInterfaceUsages.Select(Map).ToList(),
                InterfaceExhibitUsagesToBeDeleted = input.ExhibitUsagesToBeDeleted.Select(Map).ToList()
            };
        }

        private static NamedEntityDTO Map(ItInterfaceExhibitUsage interfaceExhibit)
        {
            return interfaceExhibit.ItInterfaceExhibit.ItInterface.MapToNamedEntityDTO();
        }

        private static NamedEntityDTO Map(ItInterfaceUsage interfaceUsage)
        {
            return interfaceUsage.ItInterface.MapToNamedEntityDTO();
        }
    }
}