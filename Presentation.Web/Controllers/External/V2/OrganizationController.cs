using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [PublicApi]
    [RoutePrefix("api/v2")]
    public class OrganizationController: ExternalBaseController
    {
        public OrganizationController()
        {

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
            //TODO: Create domain service to do the query and map the response here!
            return Ok(new List<OrganizationResponseDTO>());
        }
    }
}