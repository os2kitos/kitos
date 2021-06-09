using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel.Organization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [PublicApi]
    [RoutePrefix("api/v2")]
    public class OrganizationController : ExternalBaseController
    {
        private readonly IRightsHoldersService _rightsHoldersService;

        public OrganizationController(IRightsHoldersService rightsHoldersService)
        {
            _rightsHoldersService = rightsHoldersService;
        }

        /// <summary>
        /// Returns organizations in which the current user has "RightsHolderAccess" permission
        /// </summary>
        /// <returns>A list of organizations formatted as uuid, cvr and name pairs</returns>
        [HttpGet]
        [Route("rightsholder/organizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetAccessibleOrganizations()
        {
            //TODO: Pagination standard - add it here
            return _rightsHoldersService
                .ResolveOrganizationsWhereAuthenticatedUserHasRightsHolderAccess()
                .Select(ToDTOs)
                .Match(Ok, FromOperationError);
        }

        private IEnumerable<OrganizationResponseDTO> ToDTOs(IQueryable<Organization> organizations)
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