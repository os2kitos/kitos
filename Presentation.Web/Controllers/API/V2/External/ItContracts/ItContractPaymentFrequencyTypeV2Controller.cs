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
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Controllers.API.V2.External.ItContracts
{
    [RoutePrefix("api/v2/it-contract-payment-frequency-types")]
    public class ItContractPaymentFrequencyTypeV2Controller : BaseRegularOptionTypeV2Controller<ItContract, PaymentFreqencyType>
    {
        public ItContractPaymentFrequencyTypeV2Controller(IOptionsApplicationService<ItContract, PaymentFreqencyType> optionService)
            : base(optionService)
        {
        }

        /// <summary>
        /// Returns It-Contract payment frequency type options 
        /// </summary>
        /// <param name="organizationUuid">organization context for the payment frequency types availability</param>
        /// <returns>A list of available It-Contract payment frequency types</returns>
        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid organizationUuid, [FromUri] UnboundedPaginationQuery pagination = null)
        {
            return GetAll(organizationUuid, pagination);
        }

        /// <summary>
        /// Returns requested It-Contract payment frequency type
        /// </summary>
        /// <param name="paymentFrequencyTypeUuid">payment frequency type identifier</param>
        /// <param name="organizationUuid">organization context for the payment frequency type availability</param>
        /// <returns>A uuid and name pair with boolean to mark if the payment frequency type is available in the organization</returns>
        [HttpGet]
        [Route("{paymentFrequencyTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RegularOptionExtendedResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult Get([NonEmptyGuid] Guid paymentFrequencyTypeUuid, [NonEmptyGuid] Guid organizationUuid)
        {
            return GetSingle(paymentFrequencyTypeUuid, organizationUuid);
        }
    }
}