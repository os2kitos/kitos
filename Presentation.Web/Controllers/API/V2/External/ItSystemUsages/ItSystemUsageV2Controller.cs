using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.SystemUsage;
using Presentation.Web.Controllers.API.V2.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Presentation.Web.Models.API.V2.Types.Shared;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages
{
    /// <summary>
    /// API for the local registrations related to it-systems in KITOS
    /// NOTE: IT-System usages are registrations which extend those of a system within the context of a specific organization.
    /// </summary>
    [RoutePrefix("api/v2/it-system-usages")]
    public class ItSystemUsageV2Controller : ExternalBaseController
    {
        private readonly IItSystemUsageService _itSystemUsageService;

        public ItSystemUsageV2Controller(IItSystemUsageService itSystemUsageService)
        {
            _itSystemUsageService = itSystemUsageService;
        }

        /// <summary>
        /// Returns all IT-System usages available to the current user in the specified organization context
        /// </summary>
        /// <param name="organizationUuid">Query usages within a specific organization</param>
        /// <param name="relatedToSystemUuid">Query by systems related to another system</param>
        /// <param name="relatedToSystemUsageUuid">Query by system usages related to a specific system usage (more narrow search than using system id)</param>
        /// <param name="systemUuid">Query usages of a specific system</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemUsageResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemUsages(
            [NonEmptyGuid] Guid? organizationUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUuid = null,
            [NonEmptyGuid] Guid? relatedToSystemUsageUuid = null,
            [NonEmptyGuid] Guid? systemUuid = null,
            string systemNameContent = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conditions = new List<IDomainQuery<ItSystemUsage>>();

            if (organizationUuid.HasValue)
                conditions.Add(new QueryByOrganizationUuid<ItSystemUsage>(organizationUuid.Value));

            if (relatedToSystemUuid.HasValue)
                conditions.Add(new QueryByRelationToSystem(relatedToSystemUuid.Value));

            if (relatedToSystemUsageUuid.HasValue)
                conditions.Add(new QueryByRelationToSystemUsage(relatedToSystemUsageUuid.Value));

            if (systemUuid.HasValue)
                conditions.Add(new QueryBySystemUuid(systemUuid.Value));

            if (!string.IsNullOrWhiteSpace(systemNameContent))
                conditions.Add(new QueryBySystemNameContent(systemNameContent));

            return _itSystemUsageService
                .Query(conditions.ToArray())
                .Select(usages => usages.OrderBy(itSystemUsage => itSystemUsage.Id))
                .Select(usages => usages.Page(paginationQuery))
                .Select(usages => usages.ToList())
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns a specific IT-System usage (a specific IT-System in a specific Organization)
        /// </summary>
        /// <param name="systemUsageUuid">UUID of the system usage entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemUsage([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemUsageService
                .GetByUuid(systemUsageUuid)
                .Select(ToSystemUsageDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Deletes a system usage.
        /// NOTE: this action also clears any incoming relation e.g. relations from other system usages, contracts, projects or data processing registrations.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteItSystemUsage([NonEmptyGuid] Guid systemUsageUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Creates an IT-System usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict, description: "Another system usage has already been created for the system within the specified organization")]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostItSystemUsage([FromBody] CreateItSystemUsageRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the properties of the system usage.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsage([NonEmptyGuid] Guid systemUsageUuid, [FromBody] UpdateItSystemUsageRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the general properties of the system usage.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/general")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageGeneralProperties([NonEmptyGuid] Guid systemUsageUuid, [FromBody] GeneralDataWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the role assignments of the system usage.
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/roles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageRoleAssignments([NonEmptyGuid] Guid systemUsageUuid, [FromBody] IEnumerable<RoleAssignmentResponseDTO> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the kle deviations of the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/kle")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageKleDeviations([NonEmptyGuid] Guid systemUsageUuid, [FromBody] LocalKLEDeviationsRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the external references of the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/external-references")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageExternalReferences([NonEmptyGuid] Guid systemUsageUuid, [FromBody] IEnumerable<ExternalReferenceDataDTO> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the archiving registrations of the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/archiving")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageArchiving([NonEmptyGuid] Guid systemUsageUuid, [FromBody] ArchivingWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the GDPR registrations of the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/gdpr")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageArchiving([NonEmptyGuid] Guid systemUsageUuid, [FromBody] GDPRWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the organizational references for the system usage
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/organization-usage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemUsageResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageOrganizationUsage([NonEmptyGuid] Guid systemUsageUuid, [FromBody] OrganizationUsageWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Creates a system relation
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{systemUsageUuid}/system-relations")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(SystemRelationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PostSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [FromBody] SystemRelationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets a specific relation
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="systemRelationUuid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{systemUsageUuid}/system-relations/{systemRelationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SystemRelationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Updates the system relation
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="systemRelationUuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{systemUsageUuid}/system-relations/{systemRelationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SystemRelationResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult PutSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid, [FromBody] SystemRelationWriteRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Deletes a system relation
        /// </summary>
        /// <param name="systemUsageUuid"></param>
        /// <param name="systemRelationUuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{systemUsageUuid}/system-relations/{systemRelationUuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult DeleteSystemUsageRelation([NonEmptyGuid] Guid systemUsageUuid, [NonEmptyGuid] Guid systemRelationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            throw new System.NotImplementedException();
        }

        private static ItSystemUsageResponseDTO ToSystemUsageDTO(ItSystemUsage arg)
        {
            return new()
            {
                Uuid = arg.Uuid,
                SystemContext = arg.ItSystem.MapIdentityNamePairDTO(),
                OrganizationContext = arg.Organization.MapShallowOrganizationResponseDTO()
            //TODO - the rest for the best
            };
        }
    }
}