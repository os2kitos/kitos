using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Presentation.Web.Models.External.V2;
using Swashbuckle.Examples;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    public partial class RightsHolderController
    {
        /// <summary>
        /// Creates a new IT-Interface based on given input values
        /// </summary>
        /// <param name="itInterfaceDTO">A collection of specific IT-Interface values</param>
        /// <returns>Location header is set to uri for newly created IT-Interface</returns>
        [HttpPost]
        [Route("it-interfaces")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponseHeader(HttpStatusCode.Created, "Location", "string", "Location of the newly created it-interface")]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public HttpResponseMessage PostItInterface([FromBody] ItInterfaceRequestDTO itInterfaceDTO)
        {
            return Created(new Uri(Request.RequestUri + "/" + itInterfaceDTO.Uuid));
        }

        /// <summary>
        /// Returns active IT-Interfaces
        /// </summary>
        /// <param name="page">Page index to be returned (zero based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        [HttpGet]
        [Route("it-interfaces")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItInterfaceResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public HttpResponseMessage GetItInterface(int? page = 0, int? pageSize = 100)
        {
            return Ok(new List<ItInterfaceResponseDTO>());
        }

        /// <summary>
        /// Returns requested IT-Interface
        /// </summary>
        /// <param name="uuid">Specific IT-Interface UUID</param>
        /// <returns>Specific data related to the IT-Interface</returns>
        [HttpGet]
        [Route("it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetItInterface(Guid uuid)
        {
            return Ok(new ItInterfaceResponseDTO());
        }

        /// <summary>
        /// Sets individual IT-Interface values
        /// </summary>
        /// <param name="uuid">Specific IT-Interface UUID</param>
        /// <returns>The updated IT-Interface</returns>
        [HttpPatch]
        [Route("it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ItInterfaceResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PatchItInterface(Guid uuid, [FromBody] ItInterfaceRequestDTO itInterfaceRequestDTO)
        {
            return Ok(new ItInterfaceResponseDTO());
        }

        /// <summary>
        /// Deactivates an IT-Interface
        /// </summary>
        /// <param name="uuid">Specific IT-Interface UUID</param>
        /// <param name="deactivationReasonDTO">Reason for deactivation</param>
        /// <returns>No content</returns>
        [HttpDelete]
        [Route("it-interfaces/{uuid}")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage DeleteItInterface(Guid uuid, [FromBody] DeactivationReasonRequestDTO deactivationReasonDTO)
        {
            return NoContent();
        }
    }
}