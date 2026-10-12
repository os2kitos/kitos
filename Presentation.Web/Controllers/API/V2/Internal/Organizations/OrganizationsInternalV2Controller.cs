using System;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Organizations.Write;
using Core.ApplicationServices.UIConfiguration;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Core.ApplicationServices.Model.Organizations;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations.Conflicts;
using Core.DomainModel.ItSystem;
using Core.DomainModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

using System.Net;
namespace Presentation.Web.Controllers.API.V2.Internal.Organizations
{
    /// <summary>
    /// Internal API for the organizations in KITOS
    /// </summary>
    [Route("api/v2/internal/organizations")]
    public class OrganizationsInternalV2Controller : InternalApiV2Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationWriteService _organizationWriteService;
        private readonly IOrganizationResponseMapper _organizationResponseMapper;
        private readonly IOrganizationWriteModelMapper _organizationWriteModelMapper;
        private readonly IUIModuleCustomizationService _uiModuleCustomizationService;

        public OrganizationsInternalV2Controller(IOrganizationService organizationService, IOrganizationResponseMapper organizationResponseMapper, IOrganizationWriteModelMapper organizationWriteModelMapper, IOrganizationWriteService organizationWriteService, IUIModuleCustomizationService uiModuleCustomizationService)
        {
            _organizationService = organizationService;
            _organizationResponseMapper = organizationResponseMapper;
            _organizationWriteModelMapper = organizationWriteModelMapper;
            _organizationWriteService = organizationWriteService;
            _uiModuleCustomizationService = uiModuleCustomizationService;
        }

        [HttpGet]
        [Route("{organizationUuid}/ui-root-config")]
        [ApiResponse(typeof(UIRootConfigResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetUIRootConfig([NonEmptyGuid] Guid organizationUuid)
        {
            return _organizationService.GetUIRootConfig(organizationUuid)
                .Select(_organizationResponseMapper.ToUIRootConfigDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpPatch]
        [Route("{organizationUuid}/ui-root-config")]
        [ApiResponse(typeof(UIRootConfigResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult PatchUIRootConfig([NonEmptyGuid] Guid organizationUuid, [FromBody] UIRootConfigUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _organizationWriteModelMapper.ToUIRootConfigUpdateParameters(dto);

            return _organizationWriteService.PatchUIRootConfig(organizationUuid, updateParameters)
                .Select(_organizationResponseMapper.ToUIRootConfigDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("{organizationUuid}/permissions")]
        [ApiResponse(typeof(OrganizationPermissionsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetPermissions([NonEmptyGuid] Guid organizationUuid)
        {
            return _organizationService.GetPermissions(organizationUuid)
                .Select(_organizationResponseMapper.ToPermissionsDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("{organizationUuid}/ui-customization/{moduleName}")]
        [ApiResponse(typeof(UIModuleCustomizationResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetUIModuleCustomization([NonEmptyGuid] Guid organizationUuid, [FromRoute] string moduleName)
        {
            return _uiModuleCustomizationService.GetModuleCustomizationByOrganizationUuid(organizationUuid, moduleName)
             .Select(_organizationResponseMapper.ToUIModuleCustomizationResponseDTO)
             .Match(Ok, FromOperationError);
        }

        [Route("{organizationUuid}/ui-customization/{moduleName}")]
        [HttpPut]
        [ApiResponse(typeof(UIModuleCustomizationResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult PutUIModuleCustomization([NonEmptyGuid] Guid organizationUuid, [FromRoute] string moduleName,
            [FromBody] UIModuleCustomizationRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParametersResult =
                _organizationWriteModelMapper.ToUIModuleCustomizationParameters(organizationUuid, moduleName, dto);
            if (updateParametersResult.Failed) return FromOperationError(updateParametersResult.Error);

            return _uiModuleCustomizationService.UpdateModuleAndGet(updateParametersResult.Value)
                .Select(_organizationResponseMapper.ToUIModuleCustomizationResponseDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpPost]
        [Route("create")]
        [ApiResponse(typeof(ShallowOrganizationResponseDTO), HttpStatusCode.Created)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult CreateOrganization([FromBody] OrganizationCreateRequestDTO request)
        {
            var parameters = _organizationWriteModelMapper.ToOrganizationCreateParameters(request);
            return _organizationWriteService.CreateOrganization(parameters)
                .Select(x => x.MapShallowOrganizationResponseDTO())
                .Match(MapOrgCreatedResponse, FromOperationError);
        }

        [HttpPatch]
        [Route("{organizationUuid}/patch")]
        [ApiResponse(typeof(OrganizationResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult PatchOrganization([FromRoute][NonEmptyGuid] Guid organizationUuid, [FromBody] OrganizationUpdateRequestDTO requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _organizationWriteModelMapper.ToOrganizationUpdateParameters(requestDto);
            return _organizationWriteService.PatchOrganization(organizationUuid, updateParameters)
                .Select(_organizationResponseMapper.ToOrganizationDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route("{organizationUuid}/delete")]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult DeleteOrganization([FromRoute][NonEmptyGuid] Guid organizationUuid, [FromQuery] bool enforceDeletion)
        {
            return _organizationService.RemoveOrganization(organizationUuid, enforceDeletion)
                    .Match(FromOperationError, NoContent);
        }

        [HttpPatch]
        [Route("{organizationUuid}/disabled-status")]
        [ApiResponse(HttpStatusCode.NoContent)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult ChangeDisabledStatus([FromRoute][NonEmptyGuid] Guid organizationUuid, [FromBody] OrganizationDisabledStatusRequestDTO request)
        {
            return _organizationService.ChangeOrganizationDisabledStatus(organizationUuid, request.Disabled)
                    .Match(FromOperationError, NoContent);
        }

        [HttpGet]
        [Route("{organizationUuid}/conflicts")]
        [ApiResponse(typeof(OrganizationRemovalConflictsResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetConflicts([FromRoute][NonEmptyGuid] Guid organizationUuid)
        {
            return _organizationService.ComputeOrganizationRemovalConflicts(organizationUuid)
                    .Select(MapConflictsToDTO)
                    .Match(Ok, FromOperationError);
        }

        [HttpPatch]
        [Route("{organizationUuid}/master-data")]
        [ApiResponse(typeof(OrganizationMasterDataResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult PatchOrganizationMasterData([FromRoute] [NonEmptyGuid] Guid organizationUuid, [FromBody] OrganizationMasterDataRequestDTO requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _organizationWriteModelMapper.ToMasterDataUpdateParameters(requestDto);
            return _organizationWriteService.PatchMasterData(organizationUuid, updateParameters)
                .Select(_organizationResponseMapper.ToMasterDataDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("{organizationUuid}/master-data")]
        [ApiResponse(typeof(OrganizationMasterDataResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetOrganizationMasterData([FromRoute] [NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _organizationService.GetOrganization(organizationUuid)
                .Select(_organizationResponseMapper.ToMasterDataDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpGet]
        [Route("{organizationUuid}/master-data/roles")]
        [ApiResponse(typeof(OrganizationMasterDataRolesResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetOrganizationMasterDataRoles([FromRoute][NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _organizationWriteService.GetOrCreateOrganizationMasterDataRoles(organizationUuid)
                .Select(_organizationResponseMapper.ToRolesDTO)
                .Match(Ok, FromOperationError);
        }
        
        [HttpPatch]
        [Route("{organizationUuid}/master-data/roles")]
        [ApiResponse(typeof(OrganizationMasterDataRolesResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult UpsertOrganizationMasterDataRoles([FromRoute][NonEmptyGuid] Guid organizationUuid, [FromBody] OrganizationMasterDataRolesRequestDTO requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _organizationWriteModelMapper.ToMasterDataRolesUpdateParameters(organizationUuid, requestDto);
            return _organizationWriteService.PatchOrganizationMasterDataRoles(organizationUuid, updateParameters)
                .Select(_organizationResponseMapper.ToRolesDTO)
                .Match(Ok, FromOperationError);
        }

        private OrganizationRemovalConflictsResponseDTO MapConflictsToDTO(OrganizationRemovalConflicts conflicts)
        {
            return new()
            {
                SystemsWithUsagesOutsideTheOrganization = conflicts.SystemsWithUsagesOutsideTheOrganization.Select(MapSystemToConflictDTO).ToList(),
                InterfacesExposedOnSystemsOutsideTheOrganization = conflicts.InterfacesExposedOnSystemsOutsideTheOrganization.Select(MapExposedInterfacesToConflictDTO).ToList(),
                SystemsExposingInterfacesDefinedInOtherOrganizations = conflicts.SystemsExposingInterfacesDefinedInOtherOrganizations.Select(system => MapToMultipleConflictDTO(system, system.ItInterfaceExhibits.Select(i => i.ItInterface))).ToList(),
                SystemsSetAsParentSystemToSystemsInOtherOrganizations = conflicts.SystemsSetAsParentSystemToSystemsInOtherOrganizations.Select(system => MapToMultipleConflictDTO(system, system.Children)).ToList(),
                DprInOtherOrganizationsWhereOrgIsDataProcessor = conflicts.DprInOtherOrganizationsWhereOrgIsDataProcessor.Select(MapToSimpleConflict).ToList(),
                DprInOtherOrganizationsWhereOrgIsSubDataProcessor = conflicts.DprInOtherOrganizationsWhereOrgIsSubDataProcessor.Select(MapToSimpleConflict).ToList(),
                ContractsInOtherOrganizationsWhereOrgIsSupplier = conflicts.ContractsInOtherOrganizationsWhereOrgIsSupplier.Select(MapToSimpleConflict).ToList(),
                SystemsInOtherOrganizationsWhereOrgIsRightsHolder = conflicts.SystemsInOtherOrganizationsWhereOrgIsRightsHolder.Select(MapToSimpleConflict).ToList(),
                SystemsWhereOrgIsArchiveSupplier = conflicts.SystemUsagesWhereOrgIsArchiveSupplier.Select(systemUsage => new SimpleConflictResponseDTO { EntityName = systemUsage.ItSystem.Name, OrganizationName = systemUsage.Organization.Name}).ToList()
            };
        }

        private SystemWithUsageOutsideOrganizationConflictResponseDTO MapSystemToConflictDTO(ItSystem system)
        {
            return new()
            {
                SystemName = system.Name,
                OrganizationNames = system.Usages.Select(usage => usage.Organization).Where(org => org.Id != system.OrganizationId).Select(org => org.Name).ToList(),
            };
        }

        private InterfacesExposedOutsideTheOrganizationResponseDTO MapExposedInterfacesToConflictDTO(ItInterface itInterface)
        {
            return new()
            {
                ExposedInterfaceName = itInterface.Name,
                ExposingSystemName = itInterface.ExhibitedBy.ItSystem.Name,
                OrganizationName = itInterface.ExhibitedBy.ItSystem.Organization.Name,
            };
        }

        private MultipleConflictsResponseDTO MapToMultipleConflictDTO<T, U>(U mainEntity, IEnumerable<T> subEntities)
            where T: IHasName, IOwnedByOrganization
            where U : IHasName, IOwnedByOrganization

        {
            return new()
            {
                MainEntityName = mainEntity.Name,
                Conflicts = subEntities.Where(subEntity => subEntity.OrganizationId != mainEntity.OrganizationId).Select(MapToSimpleConflict).ToList(),
            };
        }

        private SimpleConflictResponseDTO MapToSimpleConflict<T>(T entity)
            where T: IHasName, IOwnedByOrganization
        {
            return new()
            {
                EntityName = entity.Name,
                OrganizationName = entity.Organization.Name,
            };
        }

        private IActionResult MapOrgCreatedResponse(IdentityNamePairResponseDTO dto)
        {
            return Created($"{new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}").AbsoluteUri.TrimEnd('/')}/{dto.Uuid}", dto);
        }
    }
}



