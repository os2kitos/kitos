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
    [Route("api/v2/it-contract-payment-model-types")]
    public class ItContractPaymentModelTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, PaymentModelType>
    {
        public ItContractPaymentModelTypeV2Controller(IOptionsApplicationService<ItContract, PaymentModelType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract payment model type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the payment model types availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It-Contract payment model types</returns>
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
        /// Returns requested It-Contract payment model type
        /// </summary>
        /// <param name="paymentModelTypeUuid">payment model type identifier</param>
        /// <param name="organizationUuid">organization context for the payment model type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the payment model type is available in the organization</returns>
        [HttpGet]
        [Route("{paymentModelTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid paymentModelTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(paymentModelTypeUuid, organizationUuid);
        }
    }
}

