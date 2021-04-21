using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2;
using Swashbuckle.Examples;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [PublicApi]
    [RoutePrefix("api/v2/rightsholder")]
    public partial class RightsHolderController: ExternalBaseController
    {
        /// <summary>
        /// Creates a new IT-System based on given input values
        /// </summary>
        /// <param name="itSystemRequestDTO">A collection of specific IT-System values</param>
        /// <returns>Location header is set to uri for newly created IT-System</returns>
        [HttpPost]
        [Route("it-systems")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponseHeader(HttpStatusCode.Created, "Location", "string", "Location of the newly created it-system")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage PostItSystem([FromBody] ItSystemRequestDTO itSystemRequestDTO)
        {
            return Created(new Uri(Request.RequestUri + "/" + itSystemRequestDTO.Uuid));
        }

        /// <summary>
        /// Returns active IT-Systems
        /// </summary>
        /// <param name="page">Page index to be returned (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-systems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItSystemResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public HttpResponseMessage GetItSystems(int? page = 0, int? pageSize = 100)
        {
            return Ok(new List<ItSystemResponseDTO>());
        }

        /// <summary>
        /// Returns requested IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>Specific data related to the IT-System</returns>
        [HttpGet]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetItSystem(Guid uuid)
        {
            return Ok(new ItSystemResponseDTO());
        }

        /// <summary>
        /// Sets individual IT-System values
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <returns>The updated IT-System</returns>
        [HttpPatch]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItSystemResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchItSystem(Guid uuid, [FromBody] ItSystemRequestDTO itSystemRequestDTO)
        {
            return Ok(new ItSystemResponseDTO());
        }

        /// <summary>
        /// Deactivates an IT-System
        /// </summary>
        /// <param name="uuid">Specific IT-System UUID</param>
        /// <param name="deactivationReasonDTO">Reason for deactivation</param>
        /// <returns>No content</returns>
        [HttpDelete]
        [Route("it-systems/{uuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage DeleteItSystem(Guid uuid, [FromBody] DeactivationReasonRequestDTO deactivationReasonDTO)
        {
            return NoContent();
        }
    }
}