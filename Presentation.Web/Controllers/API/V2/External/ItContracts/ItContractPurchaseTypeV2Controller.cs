using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItContract;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    [RoutePrefix("api/v2/it-contract-purchase-types")]
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
        /// <returns>A list of available It-Contract purchase types</returns>
        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RegularOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid purchaseTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(purchaseTypeUuid, organizationUuid);
        }
    }
}