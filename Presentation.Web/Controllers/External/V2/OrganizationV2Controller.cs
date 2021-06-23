using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.RightsHolders;
using Core.DomainModel.Organization;
using Infrastructure.Services.Types;
using Presentation.Web.Extensions;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [RoutePrefix("api/v2")]
    public class OrganizationV2Controller : ExternalBaseController
    {
        private readonly IRightsHolderSystemService _rightsHolderSystemService;

        public OrganizationV2Controller(IRightsHolderSystemService rightsHolderSystemService)
        {
            _rightsHolderSystemService = rightsHolderSystemService;
        }

        /// <summary>
        /// Returns organizations in which the current user has "RightsHolderAccess" permission
        /// </summary>
        /// <returns>A list of organizations formatted as uuid, cvr and name pairs</returns>
        [HttpGet]
        [Route("rightsholder/organizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<OrganizationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetAccessibleOrganizations([FromUri] StandardPaginationQuery pagination = null)
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