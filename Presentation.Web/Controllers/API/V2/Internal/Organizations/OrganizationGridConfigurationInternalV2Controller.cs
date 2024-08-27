using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.KendoConfig;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.Organizations
{
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}/grid-configuration")]
    public class OrganizationGridConfigurationInternalV2Controller : InternalApiV2Controller
    {

        private readonly IKendoOrganizationalConfigurationService _kendoOrganizationalConfigurationService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;

        public OrganizationGridConfigurationInternalV2Controller(IKendoOrganizationalConfigurationService kendoOrganizationalConfigurationService, IEntityIdentityResolver entityIdentityResolver)
        {
            _kendoOrganizationalConfigurationService = kendoOrganizationalConfigurationService;
            _entityIdentityResolver = entityIdentityResolver;
        }

        [Route("test")]
        [HttpGet]
        public IHttpActionResult test() { 
            return Ok("Hello world");
        }

        [HttpPost]
        [Route("save")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationGridConfigurationResponseDTO))]
        public IHttpActionResult SaveGridConfiguration([NonEmptyGuid] Guid organizationUuid, OverviewType overviewType, [FromBody] IEnumerable<KendoColumnConfiguration> columns)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return MapUuidToID(organizationUuid)
                .Bind(id => _kendoOrganizationalConfigurationService.CreateOrUpdate(id, overviewType, columns))
                .Bind(MapKendoConfigToGridConfig)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("delete")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeleteGridConfiguration([NonEmptyGuid] Guid organizationUuid, OverviewType overviewType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return MapUuidToID(organizationUuid)
                .Bind(id => _kendoOrganizationalConfigurationService.Delete(id, overviewType))
                .Bind(MapKendoConfigToGridConfig)
                .Match(Ok, FromOperationError);

        }

        [HttpGet]
        [Route("get")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetGridConfiguration([NonEmptyGuid] Guid organizationUuid, OverviewType overviewType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return MapUuidToID(organizationUuid)
                .Bind(id => _kendoOrganizationalConfigurationService.Get(id, overviewType))
                .Bind(MapKendoConfigToGridConfig)
                .Match(Ok, FromOperationError);
        }

        private Result<int, OperationError> MapUuidToID(Guid organizationUuid)
        {
            var value = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            if (value.IsNone)
            {
                return new OperationError("The provided organization ID does not exist", OperationFailure.NotFound);
            }
            return value.Value;
        }

        private Result<Guid, OperationError> mapIDToUuid(int organizationId)
        {
            var value = _entityIdentityResolver.ResolveUuid<Organization>(organizationId);
            if (value.IsNone)
            {
                return new OperationError("The provided organization Uuid does not exist", OperationFailure.NotFound);
            }
            return value.Value;
        }

        private Result<OrganizationGridConfigurationResponseDTO, OperationError> MapKendoConfigToGridConfig(KendoOrganizationalConfiguration kendoConfig)
        {
            var orgUuid = _entityIdentityResolver.ResolveUuid<Organization>(kendoConfig.OrganizationId);
            if (orgUuid.IsNone)
            {
                return new OperationError("The provided organization Uuid does not exist", OperationFailure.NotFound);
            }
            return new OrganizationGridConfigurationResponseDTO
            {
                OrganizationUuid = orgUuid.Value,
                OverviewType = kendoConfig.OverviewType,
                Version = kendoConfig.Version,
                VisibleColumns = kendoConfig.VisibleColumns.Select(mapKendoColumnConfigToConfigDTO)
            };
        }

        private ColumnConfigurationResponseDTO mapKendoColumnConfigToConfigDTO(KendoColumnConfiguration columnConfig)
        {
            return new ColumnConfigurationResponseDTO
            {
                PersistId = columnConfig.PersistId,
                Index = columnConfig.Index,

            };
        }
    }
}