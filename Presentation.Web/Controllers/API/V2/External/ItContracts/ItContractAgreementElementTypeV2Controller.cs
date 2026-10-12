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
    [Route("api/v2/it-contract-agreement-element-types")]
    public class ItContractAgreementElementTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, AgreementElementType>
    {
        public ItContractAgreementElementTypeV2Controller(IOptionsApplicationService<ItContract, AgreementElementType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract agreement element type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the agreement element types availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It-Contract agreement element types</returns>
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
        /// Returns requested It-Contract agreement element type
        /// </summary>
        /// <param name="agreementElementTypeUuid">agreement element type identifier</param>
        /// <param name="organizationUuid">organization context for the agreement element type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the agreement element type is available in the organization</returns>
        [HttpGet]
        [Route("{agreementElementTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid agreementElementTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(agreementElementTypeUuid, organizationUuid);
        }
    }
}

