using Core.ApplicationServices.LocalOptions;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System;
using Presentation.Web.Models.API.V2.Internal.Response.LocalOptions;
using Core.DomainModel.ItSystem;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits
{
    [RoutePrefix("api/v2/internal/organization-units/{organizationUuid}/local-option-types")]
    public class OrganizationUnitLocalOptionTypesInternalV2Controller: InternalApiV2Controller
    {
        private readonly IGenericLocalOptionsService<LocalOrganizationUnitRole, OrganizationUnitRight, OrganizationUnitRole> _localOrganizationUnitService;
        private readonly ILocalOptionTypeResponseMapper _responseMapper;
        private readonly ILocalOptionTypeWriteModelMapper _writeModelMapper;

        public OrganizationUnitLocalOptionTypesInternalV2Controller(IGenericLocalOptionsService<LocalOrganizationUnitRole, OrganizationUnitRight, OrganizationUnitRole> localOrganizationUnitService, ILocalOptionTypeResponseMapper responseMapper, ILocalOptionTypeWriteModelMapper writeModelMapper)
        {
            _localOrganizationUnitService = localOrganizationUnitService;
            _responseMapper = responseMapper;
            _writeModelMapper = writeModelMapper;
        }

        [HttpGet]
        [Route("organization-unit-roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<LocalRoleOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetLocalOrganizationUnitRoles([NonEmptyGuid][FromUri] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            var organizationUnitRoles = _localOrganizationUnitService.GetLocalOptions(organizationUuid);
            return Ok(_responseMapper.ToLocalRoleOptionDTOs<OrganizationUnitRight, OrganizationUnitRole>(organizationUnitRoles));
        }

        [HttpGet]
        [Route("organization-unit-roles/{optionUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(LocalRoleOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetLocalOrganizationUnitRole([NonEmptyGuid][FromUri] Guid organizationUuid, [FromUri] Guid optionUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _localOrganizationUnitService.GetLocalOption(organizationUuid, optionUuid)
                .Select(_responseMapper.ToLocalRoleOptionDTO<OrganizationUnitRight, OrganizationUnitRole>)
                .Match(Ok, FromOperationError);
        }

        [HttpPost]
        [Route("organization-unit-roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(LocalRoleOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult CreateLocalBusinessType([NonEmptyGuid][FromUri] Guid organizationUuid, LocalOptionCreateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var createParameters = _writeModelMapper.ToLocalOptionCreateParameters(dto);

            return _localOrganizationUnitService.CreateLocalOption(organizationUuid, createParameters)
                .Select(_responseMapper.ToLocalRoleOptionDTO<OrganizationUnitRight, OrganizationUnitRole>)
                .Match(Ok, FromOperationError);
        }

        [HttpPatch]
        [Route("organization-unit-roles/{optionUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(LocalRegularOptionResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult PatchLocalBusinessType([NonEmptyGuid][FromUri] Guid organizationUuid,
            [FromUri] Guid optionUuid,
            LocalRegularOptionUpdateRequestDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = _writeModelMapper.ToLocalOptionUpdateParameters(dto);

            return _localOrganizationUnitService.PatchLocalOption(organizationUuid, optionUuid, updateParameters)
                .Select(_responseMapper.ToLocalRoleOptionDTO<OrganizationUnitRight, OrganizationUnitRole>)
                .Match(Ok, FromOperationError);
        }
    }
}