using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel.Organization;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.Organization;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.Organizations
{
    [RoutePrefix("api/v2")]
    public class OrganizationV2Controller : ExternalBaseController
    {
        private readonly IRightsHolderSystemService _rightsHolderSystemService;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationalUserContext _userContext;

        public OrganizationV2Controller(IRightsHolderSystemService rightsHolderSystemService, IOrganizationService organizationService, IOrganizationalUserContext userContext)
        {
            _rightsHolderSystemService = rightsHolderSystemService;
            _organizationService = organizationService;
            _userContext = userContext;
        }

        /// <summary>
        /// Returns organizations organizations from KITOS
        /// </summary>
        /// <param name="onlyWhereUserHasMembership">If set to true, only organizations where the user has role(s) will be included.</param>
        /// <param name="nameContent">Optional query for name content</param>
        /// <param name="cvr">Optional query on CVR number</param>
        /// <returns>A list of organizations</returns>
        [HttpGet]
        [Route("organizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [DenyRightsHoldersAccess]
        public IHttpActionResult GetOrganizations(bool onlyWhereUserHasMembership = false, string nameContent = null, string cvr = null, [FromUri] StandardPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refinements = new List<IDomainQuery<Organization>>();

            if (onlyWhereUserHasMembership)
                refinements.Add(new QueryByIds<Organization>(_userContext.OrganizationIds));

            if (string.IsNullOrWhiteSpace(nameContent))
                refinements.Add(new QueryByPartOfName<Organization>(nameContent));

            if (string.IsNullOrWhiteSpace(cvr))
                refinements.Add(new QueryByCvrContent(cvr));

            return _organizationService
                .SearchAccessibleOrganizations(refinements.ToArray())
                .Page(pagination)
                .ToList()
                .Select(ToDTO)
                .Transform(Ok);
        }

        /// <summary>
        /// Returns organization identified by uuid
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <returns>A list of organizations</returns>
        [HttpGet]
        [Route("organizations/{organizationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [DenyRightsHoldersAccess]
        public IHttpActionResult GetOrganization(Guid organizationUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (organizationUuid == Guid.Empty)
                return BadRequest($"{nameof(organizationUuid)} must be a non-empty uuid");

            return _organizationService
                .GetOrganization(organizationUuid)
                .Select(ToDTO)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns organizations in which the current user has "RightsHolderAccess" permission
        /// </summary>
        /// <returns>A list of organizations formatted as uuid, cvr and name pairs</returns>
        [HttpGet]
        [Route("rightsholder/organizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ShallowOrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetOrganizationsAsRightsHolder([FromUri] StandardPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _rightsHolderSystemService
                .ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
                .OrderBy(x => x.Id)
                .Page(pagination)
                .Transform(ToDTOs)
                .Transform(Ok);
        }

        private static IEnumerable<OrganizationResponseDTO> ToDTOs(IQueryable<Organization> organizations)
        {
            return organizations.ToList().Select(ToDTO).ToList();
        }

        private static OrganizationResponseDTO ToDTO(Organization organization)
        {
            return new(organization.Uuid, organization.Name, organization.GetActiveCvr());
        }
    }
}