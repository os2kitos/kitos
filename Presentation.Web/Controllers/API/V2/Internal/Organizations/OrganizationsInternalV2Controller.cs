using System;
using System.Net;
using Core.ApplicationServices.Organizations;
using System.Web.Http;
using Core.ApplicationServices.Organizations.Write;
using Core.ApplicationServices.UIConfiguration;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Elasticsearch.Net;

namespace Presentation.Web.Controllers.API.V2.Internal.Organizations
{
    /// <summary>
    /// Internal API for the organizations in KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/organizations/{organizationUuid}")]
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

        [Route("ui-root-config")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UIRootConfigResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetUIRootConfig([NonEmptyGuid] Guid organizationUuid)
        {
            return _organizationService.GetUIRootConfig(organizationUuid)
                .Select(_organizationResponseMapper.ToUIRootConfigDTO)
                .Match(Ok, FromOperationError);
        }

        [HttpPatch]
        [Route("ui-root-config")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UIRootConfigResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult PatchUIRootConfig([NonEmptyGuid] Guid organizationUuid, UIRootConfigUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _organizationWriteModelMapper.ToUIRootConfigUpdateParameters(dto);

            return _organizationWriteService.PatchUIRootConfig(organizationUuid, updateParameters)
                .Select(_organizationResponseMapper.ToUIRootConfigDTO)
                .Match(Ok, FromOperationError);
        }

        [Route("permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationPermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetPermissions([NonEmptyGuid] Guid organizationUuid)
        {
            return _organizationService.GetPermissions(organizationUuid)
                .Select(_organizationResponseMapper.ToPermissionsDTO)
                .Match(Ok, FromOperationError);
        }

        [Route("ui-customization/{moduleName}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UIModuleCustomizationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetUIModuleCustomization([NonEmptyGuid] Guid organizationUuid, [FromUri] string moduleName)
        {
            return _uiModuleCustomizationService.GetModuleCustomizationByOrganizationUuid(organizationUuid, moduleName)
             .Select(_organizationResponseMapper.ToUIModuleCustomizationResponseDTO)
             .Match(Ok, FromOperationError);
        }

        [Route("ui-customization/{moduleName}")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UIModuleCustomizationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult PutUIModuleCustomization([NonEmptyGuid] Guid organizationUuid, [FromUri] string moduleName,
            UIModuleCustomizationRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParametersResult =
                _organizationWriteModelMapper.ToUIModuleCustomizationParameters(organizationUuid, moduleName, dto);
            if (updateParametersResult.Failed) return FromOperationError(updateParametersResult.Error);

            var updateCustomizationErrorMaybe = _uiModuleCustomizationService.UpdateModule(updateParametersResult.Value);
            
            return updateCustomizationErrorMaybe.Match(
                FromOperationError,
                () => _uiModuleCustomizationService.GetModuleCustomizationByOrganizationUuid(organizationUuid, moduleName)
                    .Select(_organizationResponseMapper.ToUIModuleCustomizationResponseDTO)
                    .Match(Ok, FromOperationError));

        }

        [HttpPatch]
        [Route("patch")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult PatchOrganization([FromUri][NonEmptyGuid] Guid organizationUuid, OrganizationUpdateRequestDTO requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _organizationWriteModelMapper.ToOrganizationUpdateParameters(requestDto);
            return _organizationWriteService.PatchOrganization(organizationUuid, updateParameters)
                .Select(_organizationResponseMapper.ToOrganizationDTO)
                .Match(Ok, FromOperationError);
        }
            
        [HttpPatch]
        [Route("master-data")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationMasterDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult PatchOrganizationMasterData([FromUri] [NonEmptyGuid] Guid organizationUuid, OrganizationMasterDataRequestDTO requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var updateParameters = _organizationWriteModelMapper.ToMasterDataUpdateParameters(requestDto);
            return _organizationWriteService.PatchMasterData(organizationUuid, updateParameters)
                .Select(_organizationResponseMapper.ToMasterDataDTO)
                .Match(Ok, FromOperationError);
        }

        [Route("master-data")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationMasterDataResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetOrganizationMasterData([FromUri] [NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _organizationService.GetOrganization(organizationUuid)
                .Select(_organizationResponseMapper.ToMasterDataDTO)
                .Match(Ok, FromOperationError);
        }

        [Route("master-data/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationMasterDataRolesResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetOrganizationMasterDataRoles([FromUri][NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _organizationWriteService.GetOrCreateOrganizationMasterDataRoles(organizationUuid)
                .Select(_organizationResponseMapper.ToRolesDTO)
                .Match(Ok, FromOperationError);
        }
        
        [HttpPatch]
        [Route("master-data/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationMasterDataRolesResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult UpsertOrganizationMasterDataRoles([FromUri][NonEmptyGuid] Guid organizationUuid, OrganizationMasterDataRolesRequestDTO requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _organizationWriteModelMapper.ToMasterDataRolesUpdateParameters(organizationUuid, requestDto);
            return _organizationWriteService.PatchOrganizationMasterDataRoles(organizationUuid, updateParameters)
                .Select(_organizationResponseMapper.ToRolesDTO)
                .Match(Ok, FromOperationError);
        }
    }
}
