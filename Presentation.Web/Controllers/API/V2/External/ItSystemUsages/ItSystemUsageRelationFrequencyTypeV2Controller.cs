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
    [Route("api/v2/it-system-usage-relation-frequency-types")]
    public class ItSystemUsageRelationFrequencyTypeV2Controller : BaseRegularOptionTypeV2Controller<SystemRelation, RelationFrequencyType>
    {
        public ItSystemUsageRelationFrequencyTypeV2Controller(IOptionsApplicationService<SystemRelation, RelationFrequencyType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns IT-System usage relation frequency option types which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the relation frequency type availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available IT-System usage relation frequency option types</returns>
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
        /// Returns requested IT-System usage relation frequency option type
        /// </summary>
        /// <param name="relationFrequencyTypeUuid">relation frequency type identifier</param>
        /// <param name="organizationUuid">organization context for the relation frequency type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the relation frequency type is available in the organization</returns>
        [HttpGet]
        [Route("{relationFrequencyTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid relationFrequencyTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(relationFrequencyTypeUuid, organizationUuid);
        }
    }
}

