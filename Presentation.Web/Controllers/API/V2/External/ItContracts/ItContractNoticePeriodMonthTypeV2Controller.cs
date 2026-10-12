using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItContract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Options;
using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    [Route("api/v2/it-contract-notice-period-month-types")]
    public class ItContractNoticePeriodMonthTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, TerminationDeadlineType>
    {
        public ItContractNoticePeriodMonthTypeV2Controller(IOptionsApplicationService<ItContract, TerminationDeadlineType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract notice period month type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the notice period month types availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It-Contract notice period month types</returns>
        [HttpGet]
        [Route("")]
        [ApiResponse(typeof(IEnumerable<RegularOptionResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-Contract notice period month type
        /// </summary>
        /// <param name="noticePeriodMonthTypeUuid">notice period month type identifier</param>
        /// <param name="organizationUuid">organization context for the notice period month type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the notice period month type is available in the organization</returns>
        [HttpGet]
        [Route("{noticePeriodMonthTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid noticePeriodMonthTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(noticePeriodMonthTypeUuid, organizationUuid);
        }
    }
}

