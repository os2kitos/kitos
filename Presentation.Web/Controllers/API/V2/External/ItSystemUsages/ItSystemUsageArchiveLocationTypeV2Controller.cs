using System;
using System.Collections.Generic;
using System.Net;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages
{
    [Route("api/v2/it-system-usage-archive-location-types")]
    public class ItSystemUsageArchiveLocationTypeV2Controller : BaseRegularOptionTypeV2Controller<ItSystemUsage, ArchiveLocation>
    {
        public ItSystemUsageArchiveLocationTypeV2Controller(IOptionsApplicationService<ItSystemUsage, ArchiveLocation> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System Usage archive location options which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the archive locations availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It-System Usage archive locations</returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<RegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-System Usage archive location
        /// </summary>
        /// <param name="archiveLocationUuid">archive location identifier</param>
        /// <param name="organizationUuid">organization context for the archive location availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the archive location is available in the organization</returns>
        [HttpGet]
        [Route("{archiveLocationUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid archiveLocationUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(archiveLocationUuid, organizationUuid);
        }
    }
}

