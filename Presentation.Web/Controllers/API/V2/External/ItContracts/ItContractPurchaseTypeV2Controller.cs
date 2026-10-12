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
    [Route("api/v2/it-contract-purchase-types")]
    public class ItContractPurchaseTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, PurchaseFormType>
    {
        public ItContractPurchaseTypeV2Controller(IOptionsApplicationService<ItContract, PurchaseFormType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract purchase type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the purchase types availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It-Contract purchase types</returns>
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
        /// Returns requested It-Contract purchase type
        /// </summary>
        /// <param name="purchaseTypeUuid">purchase type identifier</param>
        /// <param name="organizationUuid">organization context for the purchase type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the purchase type is available in the organization</returns>
        [HttpGet]
        [Route("{purchaseTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid purchaseTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(purchaseTypeUuid, organizationUuid);
        }
    }
}

