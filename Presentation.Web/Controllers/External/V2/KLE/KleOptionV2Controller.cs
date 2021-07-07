using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response.KLE;
using Presentation.Web.Models.External.V2.Types;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.KLE
{
    /// <summary>
    /// Returns the available KLE options in KITOS
    /// </summary>
    [RoutePrefix("api/v2/kle-options")]
    public class KleOptionV2Controller
    {
        /// <summary>
        /// Returns the KLE reference used by KITOS
        /// </summary>
        /// <param name="parentKleUuid">Query by parent KLE uuid</param>
        /// <param name="parentKleNumber">Query by parent KLE number (exact match)</param>
        /// <param name="kleNumberPrefix">Query by KLE number prefix</param>
        /// <param name="kleDescriptionContent">Query by KLE description content</param>
        /// <param name="kleCategory">Query by KLE category</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(VersionedKLEResponseDTO<IEnumerable<KLEDetailsDTO>>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult Get(
            [NonEmptyGuid] Guid? parentKleUuid = null,
            string parentKleNumber = null,
            string kleNumberPrefix = null,
            string kleDescriptionContent = null,
            KleCategory? kleCategory = null,
            [FromUri] StandardPaginationQuery pagination = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the details of a single KLE
        /// </summary>
        /// <param name="kleUuid">UUID of the KLE number</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{kleUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(VersionedKLEResponseDTO<KLEDetailsDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest,"kleUuid is invalid")]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid kleUuid)
        {
            throw new NotImplementedException();
        }
    }
}