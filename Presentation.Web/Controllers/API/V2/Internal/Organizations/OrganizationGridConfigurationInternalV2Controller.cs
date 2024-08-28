using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.KendoConfig;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.Organizations
{
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}/grid-configuration/{overviewType}")]
    public class OrganizationGridConfigurationInternalV2Controller : InternalApiV2Controller
    {

        private readonly IKendoOrganizationalConfigurationService _kendoOrganizationalConfigurationService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;

        public OrganizationGridConfigurationInternalV2Controller(IKendoOrganizationalConfigurationService kendoOrganizationalConfigurationService, IEntityIdentityResolver entityIdentityResolver)
        {
            _kendoOrganizationalConfigurationService = kendoOrganizationalConfigurationService;
            _entityIdentityResolver = entityIdentityResolver;
        }

        [HttpPost]
        [Route("save")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationGridConfigurationResponseDTO))]
        public IHttpActionResult SaveGridConfiguration([NonEmptyGuid] Guid organizationUuid, [FromUri] OverviewType overviewType, [FromBody] OrganizationGridConfigurationRequestDTO config)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return MapUuidToId(organizationUuid)
                .Bind(id => _kendoOrganizationalConfigurationService.CreateOrUpdate(id, overviewType, config.VisibleColumns.Select(MapKendoColumnConfigDTOToKendoColumnConfig)))
                .Bind(MapKendoConfigToGridConfig)
                .Match(Ok, FromOperationError);
                
        }

        [HttpDelete]
        [Route("delete")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult DeleteGridConfiguration([NonEmptyGuid] Guid organizationUuid, [FromUri] OverviewType overviewType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return MapUuidToId(organizationUuid)
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
        public IHttpActionResult GetGridConfiguration([NonEmptyGuid] Guid organizationUuid, [FromUri] OverviewType overviewType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return MapUuidToId(organizationUuid)
                .Bind(id => _kendoOrganizationalConfigurationService.Get(id, overviewType))
                .Bind(MapKendoConfigToGridConfig)
                .Match(Ok, FromOperationError);
        }

        private Result<int, OperationError> MapUuidToId(Guid organizationUuid)
        {
            var value = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            if (value.IsNone)
            {
                return new OperationError("The provided organization ID does not exist", OperationFailure.NotFound);
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
                VisibleColumns = kendoConfig.VisibleColumns.Select(MapKendoColumnConfigToConfigDto)
            };
        }

        private static ColumnConfigurationResponseDTO MapKendoColumnConfigToConfigDto(KendoColumnConfiguration columnConfig)
        {
            return new ColumnConfigurationResponseDTO
            {
                PersistId = columnConfig.PersistId,
                Index = columnConfig.Index,

            };
        }

        private KendoColumnConfiguration MapKendoColumnConfigDTOToKendoColumnConfig(KendoColumnConfigurationDTO columnConfig)
        {
            return new KendoColumnConfiguration
            {
                PersistId = columnConfig.PersistId,
                Index = columnConfig.Index,
            };
        }
    }
}