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
    [Route("api/v2/it-system-usage-archive-types")]
    public class ItSystemUsageArchiveTypeV2Controller : BaseRegularOptionTypeV2Controller<ItSystemUsage, ArchiveType>
    {
        public ItSystemUsageArchiveTypeV2Controller(IOptionsApplicationService<ItSystemUsage, ArchiveType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System Usage archive option types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the archivetype availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It-System Usage archive option types</returns>
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
        /// Returns requested It-System Usage archive option type
        /// </summary>
        /// <param name="archiveTypeUuid">archive type identifier</param>
        /// <param name="organizationUuid">organization context for the archive type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the archive type is available in the organization</returns>
        [HttpGet]
        [Route("{archiveTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid archiveTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(archiveTypeUuid, organizationUuid);
        }
    }
}

