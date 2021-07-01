using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2.ItSystemUsages
{
    [RoutePrefix("api/v2/it-system-usage-archive-test-location-types")]
    [DenyRightsHoldersAccess]
    public class ItSystemUsageArchiveTestLocationTypeV2Controller : BaseOptionTypeV2Controller<ItSystemUsage, ArchiveTestLocation>
    {
        public ItSystemUsageArchiveTestLocationTypeV2Controller(IOptionsApplicationService<ItSystemUsage, ArchiveTestLocation> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System Usage archive test location options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the archive test locations availability</param>
        /// <returns>A list of available It-System Usage archive test locations</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get(Guid organizationUuid, [FromUri] StandardPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-System Usage archive test location
        /// </summary>
        /// <param name="archiveTestLocationUuid">archive test location identifier</param>
        /// <param name="organizationUuid">organization context for the archive test location availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the archive test location is available in the organization</returns>
        [HttpGet]
        [Route("{archiveTestLocationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(AvailableNamePairResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get(Guid archiveTestLocationUuid, Guid organizationUuid)
        {
            return GetSingle(archiveTestLocationUuid, organizationUuid);
        }
    }
}