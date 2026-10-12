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
    [Route("api/v2/it-contract-procurement-strategy-types")]
    public class ItContractProcurementStrategyV2Controller : BaseRegularOptionTypeV2Controller<ItContract, ProcurementStrategyType>
    {
        public ItContractProcurementStrategyV2Controller(IOptionsApplicationService<ItContract, ProcurementStrategyType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract procurement strategy type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the procurement strategy types availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It-Contract procurement strategy types</returns>
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
        /// Returns requested It-Contract procurement strategy type
        /// </summary>
        /// <param name="procurementStrategyTypeUuid">procurement strategy type identifier</param>
        /// <param name="organizationUuid">organization context for the procurement strategy type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the procurement strategy type is available in the organization</returns>
        [HttpGet]
        [Route("{procurementStrategyTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid procurementStrategyTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(procurementStrategyTypeUuid, organizationUuid);
        }
    }
}

