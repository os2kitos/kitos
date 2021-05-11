using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [PublicApi]
    [RoutePrefix("api/v2/business-types")]
    public class GeneralController: ExternalBaseController
    {
        /// <summary>
        /// Returns IT-System business types
        /// </summary>
        /// <returns>A list of IT-System business type specifics formatted as uuid and name pairs</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult GetBusinessTypes()
        {
            return Ok(new List<IdentityNamePairResponseDTO>());
        }

        /// <summary>
        /// Returns requested IT-System business type
        /// </summary>
        /// <param name="uuid">IT-System identifier</param>
        /// <returns>A uuid and name pair</returns>
        [HttpGet]
        [Route("{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityNamePairResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetBusinessType(Guid uuid)
        {
            return Ok(new IdentityNamePairResponseDTO(Guid.Empty, string.Empty));
        }
    }
}