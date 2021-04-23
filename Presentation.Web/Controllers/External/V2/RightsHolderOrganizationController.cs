using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [PublicApi]
    [RoutePrefix("api/v2/rightsholder")]
    public class RightsHolderOrganizationController: ExternalBaseController
    {
        /// <summary>
        /// Returns organizations accessible to the rightsholder user
        /// </summary>
        /// <returns>A list of organizations formatted as uuid and name pairs</returns>
        [HttpGet]
        [Route("organizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetAccessibleOrganizations()
        {
            return Ok(new List<IdentityNamePairResponseDTO>());
        }
    }
}