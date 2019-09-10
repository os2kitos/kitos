using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
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

        public ItSystemUsageMigrationController(IItSystemUsageMigrationService itSystemUsageMigrationService, IAuthorizationContext authContext)
            : base(authContext)
        {
            _itSystemUsageMigrationService = itSystemUsageMigrationService;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage GetMigration([FromUri]int usageId, [FromUri]int toSystemId)
        {
            var res = _itSystemUsageMigrationService.GetSystemUsageMigration(usageId, toSystemId);
            switch (res.Status)
            {
                case OperationResult.Ok :
                    return Ok(MapItSystemUsageMigration(res.ResultValue));
                case OperationResult.Forbidden:
                    return Forbidden();
                case OperationResult.NotFound:
                    return NotFound();
                default:
                    return CreateResponse(HttpStatusCode.InternalServerError,
                        "An error occured when trying to get migration consequences");
            }

        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage ExecuteMigration([FromUri]int usageId, [FromUri]int toSystemId)
        {
            //TODO
            var result = _itSystemUsageMigrationService.ExecuteSystemUsageMigration(usageId, toSystemId);
            switch (result.Status)
            {
                case OperationResult.Ok:
                    return Ok(MapItSystemUsageDTO(result.ResultValue));
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
        public HttpResponseMessage GetAccessibilityLevel()
        {
            return Ok(new ItSystemUsageMigrationAccessDTO
            {
                CanExecuteMigration = _itSystemUsageMigrationService.CanExecuteMigration()
            });
        }

        [HttpGet]
        [Route("UnusedItSystems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemSimpleDTO>))]
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
                return Ok(new List<ItSystem>().Select(x => x.MapToNamedEntityDTO()));
            }
            if (numberOfItSystems < 1 || numberOfItSystems > 25)
            {
                return BadRequest();
            }

            var result = _itSystemUsageMigrationService.GetUnusedItSystemsByOrganization(organizationId, nameContent, numberOfItSystems, getPublicFromOtherOrganizations);

            switch (result.Status)
            {
                case OperationResult.Ok:
                    return Ok(result.ResultValue.Select(x => x.MapToNamedEntityDTO()));
                case OperationResult.Forbidden:
                    return Forbidden();
                case OperationResult.NotFound:
                    return NotFound();
                default:
                    return CreateResponse(HttpStatusCode.InternalServerError,
                        "An error occured when trying to get Unused It Systems");
            }

        }

        private static ItSystemUsageMigrationDTO MapItSystemUsageMigration(
            ItSystemUsageMigration input)
        {
            return new ItSystemUsageMigrationDTO
            {
                TargetUsage = new NamedEntityDTO{Id = input.ItSystemUsage.Id,Name = input.ItSystemUsage.LocalCallName ?? input.FromItSystem.Name} ,
                FromSystem = input.FromItSystem.MapToNamedEntityDTO(),
                ToSystem = input.ToItSystem.MapToNamedEntityDTO(),
                AffectedItProjects = input.AffectedProjects.Select(x => x.MapToNamedEntityDTO()).ToList(),
                AffectedContracts = input.AffectedContracts.Select(MapToItContractItSystemUsageDTO).ToList()
            };
        }

        private static ItSystemUsageContractMigrationDTO MapToItContractItSystemUsageDTO(ItContractMigration input)
        {
            return new ItSystemUsageContractMigrationDTO
            {
                Contract = input.Contract.MapToNamedEntityDTO(),
                SystemAssociatedInContract = input.SystemAssociatedInContract,
                AffectedInterfaceUsages = input.AffectedInterfaceUsages.Select(MapToInterfaceUsageDTO).ToList(),
                InterfaceExhibitUsagesToBeDeleted = input.ExhibitUsagesToBeDeleted.Select(MapToInterfaceExhibitUsageDTO).ToList()
            };
        }

        private static NamedEntityDTO MapToInterfaceExhibitUsageDTO(ItInterfaceExhibitUsage interfaceExhibit)
        {
            return new NamedEntityDTO
            {
                Id = interfaceExhibit.ItInterfaceExhibit.ItInterface.Id,
                Name = interfaceExhibit.ItInterfaceExhibit.ItInterface.Name
            };
        }

        private static NamedEntityDTO MapToInterfaceUsageDTO(ItInterfaceUsage interfaceUsage)
        {
            return new NamedEntityDTO
            {
                Id = interfaceUsage.ItInterface.Id,
                Name = interfaceUsage.ItInterface.Name
            };
        }

        private static NamedEntityDTO MapItSystemUsageDTO(ItSystemUsage input)
        {
            return new NamedEntityDTO
            {
                Id = input.Id,
                Name = input.LocalCallName ?? input.ItSystem.Name
            };
        }
    }
}