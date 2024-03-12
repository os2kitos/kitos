using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystem;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Types.Shared;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace Presentation.Web.Controllers.API.V2.Internal.DataProcessingRegistrations
{
    /// <summary>
    /// Internal API for the data processing registrations stored in KITOS.
    /// </summary>
    [RoutePrefix("api/v2/internal/it-systems")]
    public class DataProcessingRegistrationInternalV2Controler : InternalApiV2Controller
    {
        /// <summary>
        /// Shallow search endpoint returning all IT-Systems available to the current user
        /// </summary>
        /// <param name="rightsHolderUuid">Rightsholder UUID filter</param>
        /// <param name="businessTypeUuid">Business type UUID filter</param>
        /// <param name="kleNumber">KLE number filter ("NN.NN.NN" format)</param>
        /// <param name="kleUuid">KLE UUID number filter</param>
        /// <param name="numberOfUsers">Greater than or equal to number of users filter</param>
        /// <param name="includeDeactivated">If set to true, the response will also include deactivated it-systems</param>
        /// <param name="changedSinceGtEq">Include only changes which were LastModified (UTC) is equal to or greater than the provided value</param>
        /// <param name="nameEquals">Include only systems with a name equal to the parameter</param>
        /// <param name="nameContains">Include only systems with a name that contains the content in the parameter</param>
        /// <param name="orderByProperty">Ordering property</param>
        /// <returns></returns>
        [HttpGet]
        [Route("search")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemSearchResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystems(
            [NonEmptyGuid] Guid organizationUuid,
            string nameContains = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _itSystemService
                .ExecuteItSystemsQuery(rightsHolderUuid, businessTypeUuid, kleNumber, kleUuid, numberOfUsers, includeDeactivated, changedSinceGtEq, nameEquals: nameEquals, nameContains: nameContains, orderByProperty: orderByProperty, paginationQuery: paginationQuery, excludeUuid: excludeUuid, excludeChildrenOfUuid: excludeChildrenOfUuid)
                .Select(Map)
                .Transform(Ok);
        }
    }
}