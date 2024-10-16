using Core.ApplicationServices.LocalOptions;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System;

namespace Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits
{
    public class OrganizationUnitLocalRoleOptionTypeInternalV2Controller: InternalApiV2Controller
    {
        private readonly IGenericLocalOptionsService<LocalOrganizationUnitRole, OrganizationUnitRight, OrganizationUnitRole> _localOrganizationUnitService;
        private readonly ILocalOptionTypeResponseMapper _responseMapper;
        private readonly ILocalOptionTypeWriteModelMapper _writeModelMapper;

        [HttpGet]
        [Route("organization-unit-roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<LocalRegularOptionResponseDTO>))] //todo update return type to the new localRoleDto
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetLocalOrganizationUnitRoles([NonEmptyGuid][FromUri] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            var organizationUnitRoles = _localOrganizationUnitService.GetLocalOptions(organizationUuid);
            //todo map to roleoptiondts
            return Ok(_responseMapper.ToLocalRoleOptionDTOs<OrganizationUnitRight, OrganizationUnitRole>(organizationUnitRoles));
        }
    }
}