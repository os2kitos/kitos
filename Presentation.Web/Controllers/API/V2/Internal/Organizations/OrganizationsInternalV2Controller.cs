using System;
using System.Net;
using Core.ApplicationServices.Organizations;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.Swagger.Annotations;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Internal.Response.Roles;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Response.Organization;
using OrganizationType = Presentation.Web.Models.API.V2.Types.Organization.OrganizationType;

namespace Presentation.Web.Controllers.API.V2.Internal.Organizations
{
    /// <summary>
    /// Internal API for the organizations in KITOS
    /// </summary>
    [RoutePrefix("api/v2/internal/organizations")]
    public class OrganizationsInternalV2Controller : InternalApiV2Controller
    {
        private readonly IOrganizationService _organizationService;
        private readonly IResourcePermissionsResponseMapper _permissionsResponseMapper;
        private readonly IOrganizationMapper _organizationMapper;

        public OrganizationsInternalV2Controller(IOrganizationService organizationService, IResourcePermissionsResponseMapper permissionsResponseMapper, IOrganizationMapper organizationMapper)
        {
            _organizationService = organizationService;
            _permissionsResponseMapper = permissionsResponseMapper;
            _organizationMapper = organizationMapper;
        }

        [Route("{organizationUuid}/permissions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ResourcePermissionsResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetPermissions([NonEmptyGuid] Guid organizationUuid)
        {
            return _organizationService.GetPermissions(organizationUuid)
                .Select(_permissionsResponseMapper.Map)
                .Match(Ok, FromOperationError);
        }

        [HttpPatch]
        [Route("{organizationUuid}/masterData")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult UpdateOrganizationMasterData([FromUri] [NonEmptyGuid] Guid organizationUuid, OrganizationMasterDataRequestDTO requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var updateParameters = ToMasterDataUpdateParameters(requestDto);
            return _organizationService.UpdateOrganizationMasterData(organizationUuid, updateParameters)
                .Select(_organizationMapper.ToOrganizationDTO)
                .Match(Ok, FromOperationError);
        }

        [Route("{organizationUuid}/masterData/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationMasterDataRolesResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetOrganizationMasterDataRoles([FromUri][NonEmptyGuid] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _organizationService.GetOrganizationMasterDataRoles(organizationUuid)
                .Select(_organizationMapper.ToRolesDTO)
                .Match(Ok, FromOperationError);
        }
        
        [HttpPatch]
        [Route("{organizationUuid}/masterData/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OrganizationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult UpsertOrganizationMasterDataRoles([FromUri][NonEmptyGuid] Guid organizationUuid, OrganizationMasterDataRolesRequestDTO requestDto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var updateParameters = ToMasterDataRolesUpdateParameters(organizationUuid, requestDto);
            return _organizationService.UpsertOrganizationMasterDataRoles(organizationUuid, updateParameters)
                .Select(_organizationMapper.ToRolesDTO)
                .Match(Ok, FromOperationError);
        }

        private OrganizationMasterDataUpdateParameters ToMasterDataUpdateParameters(OrganizationMasterDataRequestDTO dto)
        {
            return new()
            {
                Cvr = OptionalValueChange<string>.With(dto.Cvr),
                Email = OptionalValueChange<string>.With(dto.Email),
                Address = OptionalValueChange<string>.With(dto.Address),
                Phone = OptionalValueChange<string>.With(dto.Phone),
            };
        }

        private static OrganizationMasterDataRolesUpdateParameters ToMasterDataRolesUpdateParameters(Guid organizationUuid,
            OrganizationMasterDataRolesRequestDTO dto)
        {
            var contactPersonDto = dto.ContactPerson;
            var contactPersonParameters = new ContactPersonUpdateParameters()
            {
                Email = OptionalValueChange<string>.With(contactPersonDto.Email),
                LastName = OptionalValueChange<string>.With(contactPersonDto.LastName),
                Name = OptionalValueChange<string>.With(contactPersonDto.Name),
                PhoneNumber = OptionalValueChange<string>.With(contactPersonDto.PhoneNumber)
            };

            var dataResponsibleDto = dto.DataResponsible;
            var dataResponsibleParameters = new DataResponsibleUpdateParameters()
            {
                Address = OptionalValueChange<string>.With(dataResponsibleDto.Address),
                Cvr = OptionalValueChange<string>.With(dataResponsibleDto.Cvr),
                Email = OptionalValueChange<string>.With(dataResponsibleDto.Email),
                Name = OptionalValueChange<string>.With(dataResponsibleDto.Name),
                Phone = OptionalValueChange<string>.With(dataResponsibleDto.Phone)
            };

            var dataProtectionAdvisorDto = dto.DataProtectionAdvisor;
            var dataProtectionAdvisorParameters = new DataProtectionAdvisorUpdateParameters()
            {
                Address = OptionalValueChange<string>.With(dataProtectionAdvisorDto.Address),
                Cvr = OptionalValueChange<string>.With(dataProtectionAdvisorDto.Cvr),
                Email = OptionalValueChange<string>.With(dataProtectionAdvisorDto.Email),
                Name = OptionalValueChange<string>.With(dataProtectionAdvisorDto.Name),
                Phone = OptionalValueChange<string>.With(dataProtectionAdvisorDto.Phone)
            };

            return new()
            {
                OrganizationUuid = organizationUuid,
                ContactPerson = contactPersonParameters,
                DataResponsible = dataResponsibleParameters,
                DataProtectionAdvisor = dataProtectionAdvisorParameters
            };
        }
    }
}