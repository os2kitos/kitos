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
    [Route("api/v2/it-contract-contract-types")]
    public class ItContractContractTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, ItContractType>
    {
        public ItContractContractTypeV2Controller(IOptionsApplicationService<ItContract, ItContractType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract contract type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the contract types availability</param>
        /// <param name="pagination">Optional pagination query</param>
        /// <returns>A list of available It-Contract contract types</returns>
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
        /// Returns requested It-Contract contract type
        /// </summary>
        /// <param name="contractTypeUuid">contract type identifier</param>
        /// <param name="organizationUuid">organization context for the contract type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the contract type is available in the organization</returns>
        [HttpGet]
        [Route("{contractTypeUuid}")]
        [ApiResponse(typeof(RegularOptionExtendedResponseDTO), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.NotFound)]
        public IActionResult Get([NonEmptyGuid] Guid contractTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(contractTypeUuid, organizationUuid);
        }
    }
}

