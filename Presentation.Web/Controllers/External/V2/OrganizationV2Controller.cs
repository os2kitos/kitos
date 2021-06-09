using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.Shared;
using Core.DomainModel.Organization;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [PublicApi]
    [RoutePrefix("api/v2/organizations")]
    public class OrganizationV2Controller : ExternalBaseController
    {
        private readonly IRightsHoldersService _rightsHoldersService;

        public OrganizationV2Controller(IRightsHoldersService rightsHoldersService)
        {
            _rightsHoldersService = rightsHoldersService;
        }

        /// <summary>
        /// Returns organizations in which the current user has "RightsHolderAccess" permission
        /// </summary>
        /// <returns>A list of organizations formatted as uuid, cvr and name pairs</returns>
        [HttpGet]
        [Route("with-rightsholder-access")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetAccessibleOrganizations([FromUri] StandardPaginationQuery pagination)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _rightsHoldersService
                .ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
                .Select(organizations => organizations.OrderBy(organization => organization.Id))
                .Select(organizations => organizations.Page(pagination))
                .Select(ToDTOs)
                .Match(Ok, FromOperationError);
        }

        private static IEnumerable<OrganizationResponseDTO> ToDTOs(IQueryable<Organization> organizations)
        {
            return organizations.ToList().Select(ToDTO).ToList();
        }

        private static OrganizationResponseDTO ToDTO(Organization arg)
        {
            //TODO: Enforce UUID as non nullable and assign one before changing it
            //TODO: Add IHasUUID to organization
            return new OrganizationResponseDTO(arg.Uuid.GetValueOrDefault(), arg.Name, arg.Cvr ?? arg.ForeignCvr);
        }
    }
}