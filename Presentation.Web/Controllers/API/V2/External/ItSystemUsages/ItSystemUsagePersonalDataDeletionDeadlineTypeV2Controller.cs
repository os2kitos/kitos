using System;
using System.Collections.Generic;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using Microsoft.AspNetCore.Mvc;

using System.Net;
namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages
{
    [Route("api/v2/it-system-usage-personal-data-deletion-deadline-types")]
    public class ItSystemUsagePersonalDataDeletionDeadlineTypeV2Controller : BaseRegularOptionTypeV2Controller<ItSystemUsage, PersonalDataDeletionDeadlineType>
    {
        public ItSystemUsagePersonalDataDeletionDeadlineTypeV2Controller(IOptionsApplicationService<ItSystemUsage, PersonalDataDeletionDeadlineType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-System Usage personal data deletion deadline options which are available for new registrations within the organization
        /// </summary>
        /// <param name="organizationUuid">organization context for the personal data deletion deadline types availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It-System Usage personal data deletion deadline types</returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<RegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-System Usage personal data deletion deadline type
        /// </summary>
        /// <param name="personalDataDeletionDeadlineTypeUuid">Personal data deletion deadline type identifier</param>
        /// <param name="organizationUuid">organization context for the personal data deletion deadline type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the personal data deletion deadline type is available in the organization</returns>
        [HttpGet]
        [Route("{personalDataDeletionDeadlineTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid personalDataDeletionDeadlineTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(personalDataDeletionDeadlineTypeUuid, organizationUuid);
        }
    }
}
