using Core.Abstractions.Types;
using Core.ApplicationServices;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Organizations;
using Core.DomainModel;
using Core.DomainModel.KendoConfig;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Models.API.V2.Types.KendoGrid;
using System;
using System.Linq;
using System.Net;
using Presentation.Web.Controllers.API.V2.Common.Mapping;

namespace Presentation.Web.Controllers.API.V2.Internal.Organizations
{
    [Route("api/v2/internal/organizations/{organizationUuid}/grid")]
    public class OrganizationGridInternalV2Controller : InternalApiV2Controller
    {

        private readonly IKendoOrganizationalConfigurationService _kendoOrganizationalConfigurationService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IOrganizationService _organizationService;

        public OrganizationGridInternalV2Controller(IKendoOrganizationalConfigurationService kendoOrganizationalConfigurationService, IEntityIdentityResolver entityIdentityResolver, IOrganizationService organizationService)
        {
            _kendoOrganizationalConfigurationService = kendoOrganizationalConfigurationService;
            _entityIdentityResolver = entityIdentityResolver;
            _organizationService = organizationService;
        }

        [HttpPost]
        [Route("{overviewType}/save")]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(typeof(OrganizationGridConfigurationResponseDTO), HttpStatusCode.OK)]
        public IActionResult SaveGridConfiguration([NonEmptyGuid] Guid organizationUuid, [FromRoute] OverviewTypeOptions overviewType, [FromBody] OrganizationGridConfigurationRequestDTO config)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return MapUuidToId(organizationUuid)
                    .Bind(id => _kendoOrganizationalConfigurationService.CreateOrUpdate(id, overviewType.ToDomain(), config.VisibleColumns.Select(MapColumnConfigRequestToKendoColumnConfig)))
                    .Bind(MapKendoConfigToGridConfig)
                    .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("{overviewType}/delete")]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult DeleteGridConfiguration([NonEmptyGuid] Guid organizationUuid, [FromRoute] OverviewTypeOptions overviewType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return MapUuidToId(organizationUuid)
                .Bind(id => _kendoOrganizationalConfigurationService.Delete(id, overviewType.ToDomain()))
                .Bind(MapKendoConfigToGridConfig)
                .Match(Ok, FromOperationError);

        }

        [HttpGet]
        [Route("{overviewType}/get")]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult GetGridConfiguration([NonEmptyGuid] Guid organizationUuid, [FromRoute] OverviewTypeOptions overviewType)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            return MapUuidToId(organizationUuid)
                .Bind(id => _kendoOrganizationalConfigurationService.Get(id, overviewType.ToDomain()))
                .Bind(MapKendoConfigToGridConfig)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("permissions")]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(typeof(OrganizationGridPermissionsResponseDTO), HttpStatusCode.OK)]
        public IActionResult GetOrganizationGridPermissions([NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return MapUuidToId(organizationUuid)
                .Select( _organizationService.GetGridPermissions)
                .Select(MapGridPermissionsToDTO)
                .Match(Ok, FromOperationError);
        }

        private OrganizationGridPermissionsResponseDTO MapGridPermissionsToDTO(GridPermissions permissions)
        {
            return new OrganizationGridPermissionsResponseDTO
            {
                HasConfigModificationPermissions = permissions.ConfigModificationPermission
            };
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
                VisibleColumns = kendoConfig.VisibleColumns.Select(MapKendoColumnConfigToConfigDto).ToList()
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

        private KendoColumnConfiguration MapColumnConfigRequestToKendoColumnConfig(ColumnConfigurationRequestDTO columnConfig)
        {
            return new KendoColumnConfiguration
            {
                PersistId = columnConfig.PersistId,
                Index = columnConfig.Index,
            };
        }
    }
}

