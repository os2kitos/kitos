using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [PublicApi]
    [RoutePrefix("api/v2")]
    public partial class GeneralController: ExternalBaseController
    {
        /// <summary>
        /// Returns IT-System business types
        /// </summary>
        /// <returns>A list of IT-System business type specifics formatted as uuid and name pairs</returns>
        [HttpGet]
        [Route("business-types")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public HttpResponseMessage GetBusinessTypes()
        {
            return Ok(new List<IdentityNamePairResponseDTO>());
        }

        /// <summary>
        /// Returns requested IT-System business type
        /// </summary>
        /// <param name="uuid">IT-System identifier</param>
        /// <returns>A uuid and name pair</returns>
        [HttpGet]
        [Route("business-type/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityNamePairResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetBusinessType(Guid uuid)
        {
            return Ok(new IdentityNamePairResponseDTO(Guid.Empty, string.Empty));
        }
    }
}